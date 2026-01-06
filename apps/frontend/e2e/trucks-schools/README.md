# Trucks & Schools E2E Tests

Comprehensive end-to-end tests for truck and school management CRUD operations.

## Overview

These tests implement Story 2.7 acceptance criteria for validating CRUD operations, data integrity, and user interactions for trucks and schools management.

## Test Organization

```
trucks-schools/
├── trucks-crud.spec.ts       # Truck CRUD operations (AC #1)
├── schools-crud.spec.ts      # School CRUD + billing settings (AC #2)
├── data-integrity.spec.ts    # Data persistence and validation (AC #3)
└── README.md                 # This file
```

## Test Coverage

### 1. Trucks CRUD (`trucks-crud.spec.ts`)

#### AC #1.1: Navigation and List Display
- Navigate to Trucks page and display truck list
- Display truck registration numbers
- Display truck status indicators

#### AC #1.2: Create New Truck
- Create truck with valid data
- Persist created truck after page refresh

#### AC #1.3: Edit Existing Truck
- Edit truck details
- Persist edited truck details after refresh

#### AC #1.4: Soft-Delete Truck
- Soft-delete truck and remove from active list
- Show confirmation dialog before deletion

#### AC #1.5: Inline Validation Errors
- Show validation errors for empty required fields
- Show inline error for invalid truck name
- Show inline error for invalid year
- Clear validation errors when fixed

#### Additional Features
- Cancel truck creation without saving
- Filter or search trucks
- Handle truck status changes

### 2. Schools CRUD (`schools-crud.spec.ts`)

#### AC #2.1: Navigation and List Display
- Navigate to Schools page and display school list
- Display school names in list

#### AC #2.2: Create School with Contact Information
- Create new school with contact information
- Validate contact information format (email, phone)

#### AC #2.3: Edit School Details Including Contacts
- Edit school contact information
- Edit multiple contact fields simultaneously

#### AC #2.4: Configure Billing Settings
- Configure school billing settings (rate, fee description, formula)
- Validate billing rate is numeric

#### AC #2.5: Archive School
- Archive a school
- Show confirmation dialog before archiving

#### Additional Features
- Cancel school creation without saving
- Filter or search schools
- Display school contact information in list or details

### 3. Data Integrity (`data-integrity.spec.ts`)

#### AC #3.1: School Contacts Persistence
- Persist school contact person after edit and page refresh
- Persist school contact phone number after edit
- Persist school email after edit
- Persist multiple contact fields simultaneously

#### AC #3.2: Billing Settings Persistence
- Persist school billing rate after edit
- Persist billing fee description
- Persist billing formula

#### AC #3.3: Truck Status UI Updates
- Reflect truck status changes in UI
- Update truck status after edit
- Show correct status for multiple trucks

#### AC #3.4: List Filtering and Search
- Filter trucks by search term
- Filter schools by search term
- Handle partial matches in search
- Show no results for non-existent search term
- Clear search and show all results

#### Additional Data Integrity Tests
- Handle concurrent edits gracefully
- Maintain data consistency after page navigation

## Running the Tests

### Run all trucks-schools tests:
```bash
cd apps/frontend
npx playwright test e2e/trucks-schools/
```

### Run specific test file:
```bash
npx playwright test e2e/trucks-schools/trucks-crud.spec.ts
npx playwright test e2e/trucks-schools/schools-crud.spec.ts
npx playwright test e2e/trucks-schools/data-integrity.spec.ts
```

### Run with UI mode:
```bash
npx playwright test e2e/trucks-schools/ --ui
```

### Run with headed mode (see browser):
```bash
npx playwright test e2e/trucks-schools/ --headed
```

### Run specific test by name:
```bash
npx playwright test -g "should create a new truck"
npx playwright test -g "should persist school contact"
```

## Test Data

The tests use seeded test data from the backend's `TestDataSeeder`:

### Trucks (seeded data)
- **Alpha** (KCOW-001) - Active
- **Bravo** (KCOW-002) - Active

### Schools (imported from legacy XML)
- 76 schools imported from `docs/legacy/1_School/School.xml`
- Known schools: Uitstaande, Rinkel Krinkel Kleuterskool, Kiddo Kleuterskool, Akasia Kinderlandgoed, etc.

## Test Cleanup

The tests use a separate E2E test database (`kcow-e2e.db`) that is:
- Recreated on each test run for consistency
- Retained after tests for debugging purposes
- Automatically seeded with test data when `DOTNET_SEED_TEST_DATA=true`

### Manual Cleanup
To clean up test data:
```bash
# Remove test database
rm apps/backend/src/Api/kcow-e2e.db

# Or use the provided script
npm run test:e2e:all
```

## Test Isolation

- Each test runs in isolation
- Authentication is performed in `beforeEach` hook
- Tests use separate test database
- Tests run serially to avoid state sharing issues

## Acceptance Criteria Mapping

| AC | Description | Test File | Test Count |
|----|-------------|-----------|------------|
| AC #1 | Trucks CRUD operations | `trucks-crud.spec.ts` | 15+ tests |
| AC #2 | Schools CRUD + billing | `schools-crud.spec.ts` | 12+ tests |
| AC #3 | Data integrity validation | `data-integrity.spec.ts` | 14+ tests |
| AC #4 | Tests in `e2e/trucks-schools/` | ✅ Organized | - |
| AC #5 | Use seeded test data | ✅ TestDataSeeder | - |
| AC #6 | Test data cleanup | ✅ E2E database | - |
| AC #7 | All tests pass | ✅ CI validation | - |

## Troubleshooting

### Tests fail with "Backend not responding"
- Ensure backend is running on `http://localhost:5039`
- Check that `DOTNET_SEED_TEST_DATA=true` is set
- Verify test database was created: `apps/backend/src/Api/kcow-e2e.db`

### Tests timeout waiting for elements
- Increase timeout in test
- Check if frontend is running on correct port (4200)
- Verify backend is responding and data is seeded

### Authentication failures
- Verify test user exists: `admin@kcow.local` / `Admin123!`
- Check backend authentication is working
- Review test screenshots in `test-results/`

## Best Practices

When adding new tests:
1. Follow the existing test structure and naming conventions
2. Use descriptive test names that map to acceptance criteria
3. Include setup (beforeEach) and cleanup logic
4. Test both positive and negative scenarios
5. Verify data persistence and integrity
6. Add comments explaining complex test logic
7. Update this README with new test coverage

## Test Results

After running tests:
- HTML report: `playwright-report/index.html`
- Test results: `test-results/`
- Screenshots: Available in `test-results/` for failed tests
- Videos: Available in `test-results/` for failed tests
- Test database: `apps/backend/src/Api/kcow-e2e.db` (retained for debugging)
