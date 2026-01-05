# E2E Testing Guide

## Quick Start

The easiest way to run E2E tests with automatic test data seeding:

```bash
# Windows
npm run test:e2e:all

# Unix/Mac (make script executable first)
chmod +x e2e-test.sh
./e2e-test.sh
```

This will:
1. Start the backend with test data seeding enabled
2. Seed the database with test data (admin user, sample trucks, etc.)
3. Start the frontend dev server
4. Run all E2E tests
5. Clean up by stopping both servers

## Test Data

When using the automated test scripts, the following data is automatically seeded:

### Authentication
- **Admin User**: `admin@kcow.local` / `Admin123!`
- **Role**: Administrator

### Trucks (5 sample trucks)
- **KCOW-Alpha** (KCOW-001) - Active - Primary truck for downtown visits
- **KCOW-Bravo** (KCOW-002) - Active - Secondary truck for suburban visits
- **KCOW-Charlie** (KCOW-003) - Maintenance - Currently undergoing scheduled maintenance
- **KCOW-Delta** (KCOW-004) - Active - Newest truck in fleet
- **KCOW-Legacy** (KCOW-005) - Inactive - Old truck being phased out

### Test Database
- Uses a separate test database: `kcow-e2e.db`
- Database is recreated on each test run for consistency
- Database is retained after tests for debugging purposes

## Manual Setup

If you prefer to run servers manually:

### 1. Start Backend with Test Data

```bash
cd apps/backend/src/Api

# Use the E2E launch profile (sets environment and seeds data)
dotnet run --launch-profile e2e
```

The `--launch-profile e2e` parameter will:
- Set `ASPNETCORE_ENVIRONMENT=E2E`
- Load `appsettings.E2E.json` with the test database connection
- Enable `DOTNET_SEED_TEST_DATA=true`
- Automatically seed the database with test data

### 2. Start Frontend

```bash
cd apps/frontend
npm run dev
```

### 3. Run Tests

```bash
cd apps/frontend
npx playwright test
```

## Environment Configuration

The E2E tests use ASP.NET Core's environment-specific configuration and launch profiles:

- **appsettings.json** - Default settings (uses `kcow.db`)
- **appsettings.E2E.json** - E2E test settings (uses `kcow-e2e.db`)
- **appsettings.Development.json** - Development overrides
- **launchSettings.json** - Contains "e2e" launch profile with environment variables

When running E2E tests with `--launch-profile e2e`:
1. Sets `ASPNETCORE_ENVIRONMENT=E2E`
2. Loads `appsettings.json` + `appsettings.E2E.json`
3. Enables test data seeding via `DOTNET_SEED_TEST_DATA=true`
4. Runs database initialization service to seed data

## Running Tests

### Run all E2E tests:
```bash
npm run test:e2e:all     # With automatic server startup and test data
npm run test:e2e         # Requires manual server startup
```

### Run with UI mode (interactive):
```bash
npm run test:e2e:ui
```

### Run in headed mode (see browser):
```bash
npm run test:e2e:headed
```

### Run with debug mode:
```bash
npm run test:e2e:debug
```

### Run specific test file:
```bash
npx playwright test login.spec.ts
```

### Run specific test:
```bash
npx playwright test -g "should successfully login"
```

## Test Coverage

### Login Tests (`login.spec.ts`)
- Form display validation
- Empty field validation
- Invalid email format validation
- Short password validation
- Failed login with invalid credentials
- Successful login with valid credentials
- Form state during submission
- Error message on failed login
- Redirect for authenticated users

### Truck CRUD Tests (`trucks.spec.ts`)
- Display trucks list
- Validation errors for empty fields
- Create new truck
- Edit existing truck
- Delete truck
- Cancel truck creation
- Filter/search trucks
- Navigation between list and details
- Year input validation

## Troubleshooting

### Tests fail with "Backend not responding"
- Ensure the backend API is running on `http://localhost:5039`
- Check that `DOTNET_SEED_TEST_DATA=true` is set
- Verify the test database was created: `apps/backend/src/Api/kcow-e2e.db`
- Check backend logs for errors

### Tests timeout waiting for elements
- Increase timeout in test: `await expect(page.locator()).toBeVisible({ timeout: 10000 })`
- Check if the frontend is running on correct port (4200)
- Verify backend is responding and data is seeded

### Login tests fail
- Verify test user exists in database: `admin@kcow.local` / `Admin123!`
- Check backend authentication is working
- Review test screenshots in `test-results/`

### "Database already exists" errors
- The test scripts automatically remove the old test database
- If running manually, delete `kcow-e2e.db` before starting

## Test Results

After running tests:
- HTML report: `playwright-report/index.html`
- Test results: `test-results/`
- Screenshots: Available in `test-results/` for failed tests
- Videos: Available in `test-results/` for failed tests
- Test database: `apps/backend/src/Api/kcow-e2e.db` (retained for debugging)

## Adding New Test Data

To add new seed data for E2E tests:

1. Edit `apps/backend/src/Infrastructure/Data/Seeders/TestDataSeeder.cs`
2. Add data in the appropriate method (e.g., `SeedTrucksAsync`)
3. The data will be automatically seeded when `DOTNET_SEED_TEST_DATA=true`

Example:
```csharp
var newSchool = new School
{
    Name = "Test School for E2E",
    Address = "123 Test Street",
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};
context.Schools.Add(newSchool);
await context.SaveChangesAsync();
```

## Writing New Tests

1. Create test file in `e2e/` directory
2. Use `test.describe()` for grouping
3. Use `test.beforeEach()` for setup (login, navigation)
4. Use Playwright locators: `page.locator('#id')`, `page.locator('text=Text')`
5. Run tests to verify

Example:
```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test.beforeEach(async ({ page }) => {
    // Setup - login, navigate to page
    await page.goto('/login');
    await page.locator('#email').fill('admin@kcow.local');
    await page.locator('#password').fill('Admin123!');
    await page.locator('button[type="submit"]').click();
    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('should do something', async ({ page }) => {
    // Test implementation
    await expect(page.locator('h1')).toBeVisible();
  });
});
```

## Architecture

### Test Data Flow

```
E2E Test Script (e2e-test.bat/e2e-test.sh)
    ↓
dotnet run --launch-profile e2e
    ↓
Sets ASPNETCORE_ENVIRONMENT=E2E
Sets DOTNET_SEED_TEST_DATA=true
    ↓
Loads appsettings.json + appsettings.E2E.json
    ↓
DatabaseInitializationService runs
    ↓
InitializeDatabaseAsync() is called
    ↓
AuthSeeder.SeedAsync() - Seeds admin user
TestDataSeeder.SeedAsync() - Seeds test data
    ↓
Backend is ready with test data
    ↓
E2E tests run against seeded data
```

### Isolation

- E2E tests use a separate database (`kcow-e2e.db`)
- Test data is fresh for each test run
- No impact on development/production databases
- Tests can run in parallel without conflicts

