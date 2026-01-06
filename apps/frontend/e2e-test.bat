@echo off
REM E2E Test Runner Script for Windows
REM Starts backend and frontend servers with test data, runs tests, then cleans up

setlocal enabledelayedexpansion

REM Get script directory
set SCRIPT_DIR=%~dp0
set BACKEND_DIR=%SCRIPT_DIR%..\backend\src\Api
set FRONTEND_DIR=%SCRIPT_DIR%..\frontend

REM Set environment for E2E testing (launch profile handles this)
set DOTNET_SEED_TEST_DATA=true

echo ========================================
echo E2E Test Runner with Test Data
echo ========================================
echo.
echo Using E2E launch profile with:
echo - Test database: kcow-e2e.db
echo - Test data seeding enabled
echo.

goto :run

REM Function to check if port is in use
:check_port
netstat -ano | findstr ":%1" >nul 2>&1
goto :eof

REM Function to kill process on port
:kill_port
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":%1"') do (
    echo Killing process %%a on port %1
    taskkill /F /PID %%a >nul 2>&1
)
goto :eof

:run
REM Kill any existing processes on required ports
echo Cleaning up any existing processes...
call :kill_port 5039
call :kill_port 4200
timeout /t 2 /nobreak >nul

REM Remove test database if it exists
echo.
echo [Preparation] Cleaning up test database...
if exist "%BACKEND_DIR%\kcow-e2e.db" (
    del "%BACKEND_DIR%\kcow-e2e.db"
    echo Removed existing test database
)

REM Start backend with test data seeding
echo.
echo [1/3] Starting backend server on port 5039 with test data seeding...
cd /d "%BACKEND_DIR%"
set LOG_SUFFIX=%RANDOM%
start /B dotnet run --launch-profile e2e > "%TEMP%\backend_%LOG_SUFFIX%.log" 2>&1

REM Wait for backend to start
echo Waiting for backend to be ready...
set /a BACKEND_ATTEMPTS=0
:backend_wait
set /a BACKEND_ATTEMPTS+=1
if !BACKEND_ATTEMPTS! gtr 60 (
    echo ERROR: Backend failed to start
    echo.
    echo Backend log:
    type "%TEMP%\backend_%LOG_SUFFIX%.log"
    call :cleanup
    exit /b 1
)

curl -s -o nul http://localhost:5039/health
if errorlevel 1 (
    echo Waiting... !BACKEND_ATTEMPTS!/60
    timeout /t 2 /nobreak >nul
    goto backend_wait
)

echo Backend is ready!

REM Start frontend
echo.
echo [2/3] Starting frontend server on port 4200...
cd /d "%FRONTEND_DIR%"
start /B npm run dev > "%TEMP%\frontend_%LOG_SUFFIX%.log" 2>&1

REM Wait for frontend to start
echo Waiting for frontend to be ready...
set /a FRONTEND_ATTEMPTS=0
:frontend_wait
set /a FRONTEND_ATTEMPTS+=1
if !FRONTEND_ATTEMPTS! gtr 60 (
    echo ERROR: Frontend failed to start
    type "%TEMP%\frontend_%LOG_SUFFIX%.log"
    call :cleanup
    exit /b 1
)

curl -s -o nul http://localhost:4200
if errorlevel 1 (
    echo Waiting... !FRONTEND_ATTEMPTS!/60
    timeout /t 2 /nobreak >nul
    goto frontend_wait
)

echo Frontend is ready!

REM Run E2E tests
echo.
echo [3/3] Running E2E tests...
echo.
cd /d "%FRONTEND_DIR%"
npx playwright test %*

set TEST_EXIT_CODE=%errorlevel%

REM Cleanup
:cleanup
echo.
echo Cleaning up...
call :kill_port 5039
call :kill_port 4200

REM Optionally keep test database for debugging
if exist "%BACKEND_DIR%\kcow-e2e.db" (
    echo.
    echo Test database retained at: %BACKEND_DIR%\kcow-e2e.db
    echo To inspect: SQLite Explorer or DBeaver
)

if not defined TEST_EXIT_CODE set TEST_EXIT_CODE=1

if "%TEST_EXIT_CODE%"=="0" (
    echo.
    echo ========================================
    echo All tests passed!
    echo ========================================
) else (
    echo.
    echo ========================================
    echo Some tests failed (Code: %TEST_EXIT_CODE%)
    echo ========================================
    echo.
    echo To debug:
    echo 1. Check test results: playwright-report\index.html
    echo 2. Check screenshots: test-results\
    echo 3. Backend log: %TEMP%\backend.log
    echo 4. Frontend log: %TEMP%\frontend.log
    echo 5. Test database: %BACKEND_DIR%\kcow-e2e.db
)

exit /b %TEST_EXIT_CODE%
