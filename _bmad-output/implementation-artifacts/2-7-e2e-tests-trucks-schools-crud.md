# Story 2.7: E2E Tests - Trucks & Schools CRUD

Status: ready-for-dev

## Story

As a developer,
I want E2E tests covering truck and school management workflows,
So that CRUD operations and validation are validated end-to-end.

## Acceptance Criteria

1. **Given** the application is running and authenticated as admin
   **When** I run E2E tests for trucks and schools
   **Then** all truck CRUD scenarios pass:
   - User can navigate to Trucks page and see truck list
   - User can create a new truck with valid data
   - User can edit existing truck details
   - User can soft-delete a truck (removed from active list)
   - Validation errors display inline for invalid data

2. **Given** the application is running and authenticated as admin
   **When** I run E2E tests for schools
   **Then** all school CRUD scenarios pass:
   - User can navigate to Schools page and see school list
   - User can create a new school with contact information
   - User can edit school details including contacts
   - User can configure school billing settings (default rate, cycle)
   - User can archive a school

3. **Given** the E2E tests complete
   **Then** data integrity is validated:
   - School contacts and billing settings persist correctly
   - Truck status changes are reflected in UI
   - Search and filtering work on list pages

4. **Given** E2E tests are organized
   **Then** tests are in `e2e/trucks-schools/`

5. **Given** tests execute
   **Then** tests use seeded test data (multiple trucks, schools)

6. **Given** tests complete
   **Then** tests clean up test data after completion

7. **Given** Epic 2 completion requirements
   **Then** all tests must pass for Epic 2 completion

## Tasks / Subtasks

- [ ] Task 1: Set Up E2E Test Infrastructure for Trucks & Schools (AC: #4, #5)
  - [ ] Create `e2e/trucks-schools/` test directory
  - [ ] Create test fixtures with seeded trucks and schools data
  - [ ] Set up test database seeding script

- [ ] Task 2: Implement Trucks CRUD E2E Tests (AC: #1)
  - [ ] Test navigation to Trucks page
  - [ ] Test truck creation with valid data
  - [ ] Test truck editing
  - [ ] Test truck soft-delete
  - [ ] Test inline validation errors

- [ ] Task 3: Implement Schools CRUD E2E Tests (AC: #2)
  - [ ] Test navigation to Schools page
  - [ ] Test school creation with contacts
  - [ ] Test school editing including contacts
  - [ ] Test billing settings configuration
  - [ ] Test school archiving

- [ ] Task 4: Implement Data Integrity Tests (AC: #3)
  - [ ] Test school contacts persistence
  - [ ] Test billing settings persistence
  - [ ] Test truck status UI updates
  - [ ] Test list filtering and search

- [ ] Task 5: Implement Test Cleanup and Validation (AC: #6, #7)
  - [ ] Add test data cleanup after each test
  - [ ] Verify all tests pass reliably
  - [ ] Document test execution in CI pipeline

## Dev Notes

### Architecture Requirements
- **E2E Test Location**: `apps/frontend/e2e/trucks-schools/`
- **Test Framework**: Playwright
- **Test Configuration**: `apps/frontend/playwright.config.ts`

### Test Organization Pattern
```
e2e/trucks-schools/
├── trucks-crud.spec.ts      # Truck CRUD operations
└── schools-crud.spec.ts     # School CRUD + billing settings
```

### Test Data Strategy
- Use dedicated test database (SQLite in-memory or separate file)
- Seed scripts create baseline data for each test suite
- Tests run in isolation; database reset between tests
- Clean up test data after completion

### Playwright Configuration
- Tests run on Chromium
- Screenshots on failure
- Trace on first retry
- Base URL: http://localhost:4200

### Previous Story Context
- Stories 2-1 to 2-5 implemented Trucks and Schools CRUD
- Story 2-6 added data migration for seed data
- All API endpoints operational

### Testing Standards
- Each test should be independent
- Use Page Object Model where appropriate
- Assert on visible UI elements, not internal state
- Validate against acceptance criteria

### Project Structure Notes
- Align with existing E2E test patterns in `apps/frontend/e2e/`
- Reuse existing test helpers and fixtures

### References
- [Source: _bmad-output/planning-artifacts/architecture.md#E2E-Testing-Strategy]
- [Source: apps/frontend/playwright.config.ts]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
