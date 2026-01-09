# Story 8.4: E2E Tests - Activities CRUD

Status: ready-for-dev

## Story

As a developer,
I want E2E tests covering activity management workflows,
so that CRUD operations and validation are validated end-to-end.

## Acceptance Criteria

1. **Given** the application is running and authenticated as admin, **When** I run E2E tests for activities, **Then** the following scenarios are covered:

**Activities Management:**
- User can navigate to Activities page and see activity list
- User can create a new activity with valid data
- User can edit existing activity details
- User can soft-delete an activity (removed from active list)
- Validation errors display inline for invalid data

**Data Integrity:**
- Activity code uniqueness is enforced
- Grade level mapping persists correctly
- Search and filtering work on list page
- Icon base64 data is correctly stored and retrieved
- Icon thumbnails display correctly in the list

2. **And** E2E tests are organized in `e2e/activities/`
3. **And** Tests use seeded test data (multiple activities)
4. **And** Tests clean up test data after completion
5. **And** All tests must pass for Epic 8 completion

## Tasks / Subtasks

- [ ] Task 1: Create E2E Test Structure (AC: #2)
  - [ ] Create `apps/frontend/e2e/activities/` directory
  - [ ] Create `activities-crud.spec.ts` file
  - [ ] Set up test fixtures and data

- [ ] Task 2: Create Test Data Fixtures (AC: #3, #4)
  - [ ] Create activity test fixtures in `e2e/fixtures/activities.ts`
  - [ ] Define sample activities with all fields
  - [ ] Include activities with and without icons
  - [ ] Set up database seeding for tests

- [ ] Task 3: Write Navigation Tests (AC: #1)
  - [ ] Test navigation to Activities page from sidebar
  - [ ] Verify page title and structure
  - [ ] Verify table displays with correct columns
  - [ ] Verify "Add Activity" button is visible

- [ ] Task 4: Write Create Activity Tests (AC: #1)
  - [ ] Test opening create form
  - [ ] Test filling form with valid data
  - [ ] Test form submission and success message
  - [ ] Test new activity appears in list
  - [ ] Test icon upload and preview
  - [ ] Test validation errors for invalid data

- [ ] Task 5: Write Edit Activity Tests (AC: #1)
  - [ ] Test clicking activity row opens edit form
  - [ ] Test form populated with existing data
  - [ ] Test updating fields and saving
  - [ ] Test success confirmation
  - [ ] Test updated data reflects in list

- [ ] Task 6: Write Delete Activity Tests (AC: #1)
  - [ ] Test delete button triggers confirmation
  - [ ] Test confirming delete removes activity
  - [ ] Test canceling delete keeps activity
  - [ ] Test archived activity not in list

- [ ] Task 7: Write Validation Tests (AC: #1)
  - [ ] Test Code max length validation (255 chars)
  - [ ] Test Name max length validation (255 chars)
  - [ ] Test duplicate Code handling (if enforced)
  - [ ] Test inline error display

- [ ] Task 8: Write Data Integrity Tests (AC: #1)
  - [ ] Test Icon base64 data persists correctly
  - [ ] Test Icon thumbnail displays in table
  - [ ] Test missing Icon shows placeholder
  - [ ] Test all fields persist on create/update

- [ ] Task 9: Write Test Cleanup (AC: #4)
  - [ ] Implement afterEach cleanup for created activities
  - [ ] Ensure tests don't leave stale data
  - [ ] Reset database state between test runs

- [ ] Task 10: Run Full Test Suite (AC: #5)
  - [ ] Execute all E2E tests
  - [ ] Fix any flaky tests
  - [ ] Verify all tests pass consistently

## Dev Notes

### Test File Structure

```
apps/frontend/e2e/
└── activities/
    ├── activities-crud.spec.ts    # Main CRUD tests
    └── fixtures/
        └── activities.ts          # Test data fixtures
```

### Test Fixtures (e2e/fixtures/activities.ts)

```typescript
export const testActivities = {
  basic: {
    code: 'TEST-001',
    name: 'Test Activity',
    description: 'A test activity for E2E testing',
    folder: 'TEST',
    gradeLevel: '1',
    icon: null,
  },
  withIcon: {
    code: 'TEST-002',
    name: 'Activity With Icon',
    description: 'Test activity with icon',
    folder: 'TEST',
    gradeLevel: '2',
    icon: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==', // 1x1 red pixel
  },
  forUpdate: {
    code: 'TEST-003',
    name: 'Activity to Update',
    description: 'This will be updated',
    folder: 'UPDATE',
    gradeLevel: '3',
  },
  forDelete: {
    code: 'TEST-004',
    name: 'Activity to Delete',
    description: 'This will be deleted',
    folder: 'DELETE',
    gradeLevel: '4',
  },
};
```

### Playwright Test Pattern

```typescript
import { test, expect } from '@playwright/test';
import { testActivities } from './fixtures/activities';

test.describe('Activities CRUD', () => {
  test.beforeEach(async ({ page }) => {
    // Login and navigate to activities page
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'admin@kcow.com');
    await page.fill('[data-testid="password"]', 'password');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('/dashboard');
    await page.click('[data-testid="nav-activities"]');
    await page.waitForURL('/activities');
  });

  test('should display activities list', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('Activities');
    await expect(page.locator('table')).toBeVisible();
    await expect(page.locator('[data-testid="add-activity-btn"]')).toBeVisible();
  });

  test('should create a new activity', async ({ page }) => {
    await page.click('[data-testid="add-activity-btn"]');

    // Fill form
    await page.fill('[data-testid="activity-code"]', testActivities.basic.code);
    await page.fill('[data-testid="activity-name"]', testActivities.basic.name);
    await page.fill('[data-testid="activity-description"]', testActivities.basic.description);
    await page.fill('[data-testid="activity-folder"]', testActivities.basic.folder);
    await page.fill('[data-testid="activity-grade"]', testActivities.basic.gradeLevel);

    // Submit
    await page.click('[data-testid="submit-activity"]');

    // Verify success
    await expect(page.locator('.toast-success')).toBeVisible();
    await expect(page.locator(`text=${testActivities.basic.code}`)).toBeVisible();
  });

  test('should edit an existing activity', async ({ page }) => {
    // Click on existing activity
    await page.click(`[data-testid="activity-row-${testActivities.forUpdate.code}"]`);

    // Modify name
    await page.fill('[data-testid="activity-name"]', 'Updated Name');
    await page.click('[data-testid="submit-activity"]');

    // Verify update
    await expect(page.locator('.toast-success')).toBeVisible();
    await expect(page.locator('text=Updated Name')).toBeVisible();
  });

  test('should delete an activity', async ({ page }) => {
    // Click delete button
    await page.click(`[data-testid="delete-activity-${testActivities.forDelete.code}"]`);

    // Confirm deletion
    await page.click('[data-testid="confirm-delete"]');

    // Verify removal
    await expect(page.locator('.toast-success')).toBeVisible();
    await expect(page.locator(`text=${testActivities.forDelete.code}`)).not.toBeVisible();
  });

  test('should display validation errors', async ({ page }) => {
    await page.click('[data-testid="add-activity-btn"]');

    // Enter string that's too long for Code field
    const longCode = 'A'.repeat(256);
    await page.fill('[data-testid="activity-code"]', longCode);
    await page.click('[data-testid="submit-activity"]');

    // Verify validation error
    await expect(page.locator('[data-testid="code-error"]')).toBeVisible();
  });

  test('should display icon thumbnail', async ({ page }) => {
    // Find activity with icon and verify thumbnail
    const iconImg = page.locator(`[data-testid="activity-icon-${testActivities.withIcon.code}"]`);
    await expect(iconImg).toBeVisible();
    await expect(iconImg).toHaveAttribute('src', /^data:image/);
  });
});
```

### Test Data Seeding

```typescript
// In test setup or fixtures
async function seedTestActivities(apiUrl: string, authToken: string) {
  const activities = Object.values(testActivities);

  for (const activity of activities) {
    await fetch(`${apiUrl}/api/activities`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify(activity),
    });
  }
}

async function cleanupTestActivities(apiUrl: string, authToken: string) {
  const testCodes = Object.values(testActivities).map(a => a.code);

  for (const code of testCodes) {
    // Delete by code (need API endpoint or direct DB cleanup)
    await fetch(`${apiUrl}/api/activities/by-code/${code}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${authToken}`,
      },
    });
  }
}
```

### Test Selectors (data-testid)

| Selector | Element |
|----------|---------|
| `nav-activities` | Sidebar Activities link |
| `add-activity-btn` | Add Activity button |
| `activity-code` | Code input field |
| `activity-name` | Name input field |
| `activity-description` | Description textarea |
| `activity-folder` | Folder input field |
| `activity-grade` | Grade Level input field |
| `activity-icon-upload` | Icon file input |
| `submit-activity` | Form submit button |
| `activity-row-{code}` | Table row by code |
| `delete-activity-{code}` | Delete button by code |
| `confirm-delete` | Delete confirmation button |
| `cancel-delete` | Delete cancel button |
| `activity-icon-{code}` | Icon thumbnail by code |
| `code-error` | Code validation error |

### References

- [Source: apps/frontend/e2e/] - Existing E2E test examples
- [Source: apps/frontend/playwright.config.ts] - Playwright configuration
- [Source: _bmad-output/planning-artifacts/architecture.md#E2E-Testing-Strategy] - Testing strategy
- [Source: apps/frontend/e2e/trucks-schools/] - Reference CRUD test patterns

### Quality Gates

- [ ] All CRUD operations tested
- [ ] Validation scenarios covered
- [ ] Icon handling tested (upload, display, missing)
- [ ] Tests run reliably without flakiness
- [ ] Test data cleaned up after each run
- [ ] Tests complete in reasonable time (<2 minutes)

### Project Structure Notes

- Tests organized under `e2e/activities/`
- Follow existing test patterns from trucks-schools
- Use `data-testid` attributes for reliable selectors
- Tests run as part of CI pipeline

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
