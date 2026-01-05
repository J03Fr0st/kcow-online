#!/bin/bash

# E2E Test Runner Script
# Starts backend and frontend servers with test data, runs tests, then cleans up

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/../backend/src/Api"
FRONTEND_DIR="$SCRIPT_DIR/../frontend"

# Set environment for E2E testing (launch profile handles this)
export DOTNET_SEED_TEST_DATA=true

# Store PIDs for cleanup
BACKEND_PID=""
FRONTEND_PID=""

# Cleanup function
cleanup() {
    echo -e "${YELLOW}Cleaning up...${NC}"

    if [ -n "$BACKEND_PID" ]; then
        echo "Stopping backend (PID: $BACKEND_PID)"
        kill $BACKEND_PID 2>/dev/null
    fi

    if [ -n "$FRONTEND_PID" ]; then
        echo "Stopping frontend (PID: $FRONTEND_PID)"
        kill $FRONTEND_PID 2>/dev/null
    fi

    # Wait for processes to terminate
    sleep 2

    # Force kill if still running
    pkill -f "dotnet.*Api" 2>/dev/null
    pkill -f "vite.*4200" 2>/dev/null

    echo -e "${GREEN}Cleanup complete${NC}"

    # Note about test database
    if [ -f "$BACKEND_DIR/kcow-e2e.db" ]; then
        echo ""
        echo -e "${BLUE}Test database retained at: $BACKEND_DIR/kcow-e2e.db${NC}"
        echo "To inspect: sqlite3 $BACKEND_DIR/kcow-e2e.db"
    fi
}

# Set trap for cleanup on script exit
trap cleanup EXIT INT TERM

# Function to wait for server to be ready
wait_for_server() {
    local url=$1
    local name=$2
    local max_attempts=$3
    local attempt=1

    echo -e "${YELLOW}Waiting for $name to be ready...${NC}"

    while [ $attempt -le $max_attempts ]; do
        if curl -s "$url" > /dev/null 2>&1; then
            echo -e "${GREEN}$name is ready!${NC}"
            return 0
        fi
        echo "Waiting... ($attempt/$max_attempts)"
        sleep 2
        attempt=$((attempt + 1))
    done

    echo -e "${RED}ERROR: $name failed to start${NC}"
    return 1
}

echo -e "${BLUE}========================================"
echo "E2E Test Runner with Test Data"
echo -e "========================================${NC}"
echo ""
echo "Using E2E launch profile with:"
echo "  - Test database: kcow-e2e.db"
echo "  - Test data seeding enabled"
echo ""

# Kill any existing processes on required ports
echo "Cleaning up any existing processes..."
pkill -f "dotnet.*Api" 2>/dev/null
pkill -f "vite.*4200" 2>/dev/null
sleep 2

# Remove test database if it exists
echo ""
echo "[Preparation] Cleaning up test database..."
if [ -f "$BACKEND_DIR/kcow-e2e.db" ]; then
    rm "$BACKEND_DIR/kcow-e2e.db"
    echo "Removed existing test database"
fi

# Start backend
echo ""
echo "[1/3] Starting backend server on port 5039 with test data seeding..."
cd "$BACKEND_DIR"
dotnet run --launch-profile e2e > /tmp/backend-e2e.log 2>&1 &
BACKEND_PID=$!

echo "Backend PID: $BACKEND_PID"

# Wait for backend to be ready (increased timeout for data seeding)
if ! wait_for_server "http://localhost:5039/health" "Backend" 45; then
    echo ""
    echo "Backend log:"
    tail -50 /tmp/backend-e2e.log
    exit 1
fi

# Start frontend
echo ""
echo "[2/3] Starting frontend server on port 4200..."
cd "$FRONTEND_DIR"
npm run dev > /tmp/frontend-e2e.log 2>&1 &
FRONTEND_PID=$!

echo "Frontend PID: $FRONTEND_PID"

# Wait for frontend to be ready
if ! wait_for_server "http://localhost:4200" "Frontend" 30; then
    echo ""
    echo "Frontend log:"
    tail -50 /tmp/frontend-e2e.log
    exit 1
fi

# Run E2E tests
echo ""
echo "[3/3] Running E2E tests..."
echo ""
cd "$FRONTEND_DIR"
npx playwright test "$@"

TEST_EXIT_CODE=$?

if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo ""
    echo -e "${GREEN}========================================"
    echo "All tests passed!"
    echo -e "========================================${NC}"
else
    echo ""
    echo -e "${RED}========================================"
    echo "Some tests failed"
    echo -e "========================================${NC}"
    echo ""
    echo "To debug:"
    echo "  1. Check test results: playwright-report/index.html"
    echo "  2. Check screenshots: test-results/"
    echo "  3. Backend log: tail -100 /tmp/backend-e2e.log"
    echo "  4. Frontend log: tail -100 /tmp/frontend-e2e.log"
    echo "  5. Test database: sqlite3 $BACKEND_DIR/kcow-e2e.db"
fi

exit $TEST_EXIT_CODE
