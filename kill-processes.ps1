# Kill Frontend and Backend Processes
# This script kills all running frontend (Angular) and backend (.NET) processes

Write-Host "Killing Frontend and Backend processes..." -ForegroundColor Yellow

# Kill Backend processes
Write-Host "`n[Backend] Searching for backend processes..." -ForegroundColor Cyan
$backendProcesses = Get-Process -Name "backend" -ErrorAction SilentlyContinue
if ($backendProcesses) {
    foreach ($proc in $backendProcesses) {
        Write-Host "  Killing backend process (PID: $($proc.Id))..." -ForegroundColor Gray
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  Backend processes killed." -ForegroundColor Green
} else {
    Write-Host "  No backend processes found." -ForegroundColor Gray
}

# Kill .NET Host processes (often used by backend)
Write-Host "`n[.NET Host] Searching for .NET Host processes..." -ForegroundColor Cyan
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    foreach ($proc in $dotnetProcesses) {
        Write-Host "  Killing .NET process (PID: $($proc.Id))..." -ForegroundColor Gray
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  .NET processes killed." -ForegroundColor Green
} else {
    Write-Host "  No .NET processes found." -ForegroundColor Gray
}

# Kill Frontend processes (Node.js/Angular)
Write-Host "`n[Frontend] Searching for frontend processes..." -ForegroundColor Cyan

# Kill all node processes (Angular typically runs via node)
$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    foreach ($proc in $nodeProcesses) {
        Write-Host "  Killing Node process (PID: $($proc.Id))..." -ForegroundColor Gray
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  Node processes killed." -ForegroundColor Green
} else {
    Write-Host "  No Node processes found." -ForegroundColor Gray
}

# Kill node processes running on port 4200 (default Angular port)
$port4200Processes = Get-NetTCPConnection -LocalPort 4200 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port4200Processes) {
    foreach ($pid in $port4200Processes) {
        $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
        if ($proc -and $proc.ProcessName -eq "node") {
            Write-Host "  Killing Node process on port 4200 (PID: $pid)..." -ForegroundColor Gray
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

# Kill processes running on port 5039 (backend HTTP port)
$port5039Processes = Get-NetTCPConnection -LocalPort 5039 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port5039Processes) {
    foreach ($pid in $port5039Processes) {
        $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
        if ($proc) {
            Write-Host "  Killing process on port 5039 (PID: $pid, Name: $($proc.ProcessName))..." -ForegroundColor Gray
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

# Kill processes running on port 7029 (backend HTTPS port)
$port7029Processes = Get-NetTCPConnection -LocalPort 7029 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port7029Processes) {
    foreach ($pid in $port7029Processes) {
        $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
        if ($proc) {
            Write-Host "  Killing process on port 7029 (PID: $pid, Name: $($proc.ProcessName))..." -ForegroundColor Gray
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "`nDone! Waiting 2 seconds for processes to fully terminate..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

Write-Host "`nProcess cleanup complete!" -ForegroundColor Green

