# Story 3.7: E2E Tests - Class Groups & Scheduling Conflicts

Status: ready-for-dev

## Story

As a developer,
I want E2E tests covering class group scheduling and conflict detection,
So that the scheduling workflow and conflict resolution are validated end-to-end.

## Acceptance Criteria

1. **Given** the application is running and authenticated as admin
   **When** I run E2E tests for class groups CRUD
   **Then** all scenarios pass:
   - User can navigate to Class Groups page and see list
   - User can create a class group with school, truck, day/time
   - User can edit class group schedule
   - User can archive a class group
   - Filters work for school and truck

2. **Given** the application is running
   **When** I run E2E tests for schedule configuration
   **Then** all scenarios pass:
   - User can select day of week, start time, end time, sequence
   - User can assign truck to class group
   - Schedule appears in weekly view
   - Schedule blocks show class group, school, truck details

3. **Given** the application is running (FR6 - Critical)
   **When** I run E2E tests for conflict detection
   **Then** all scenarios pass:
   - User attempts to create overlapping schedule for same truck
   - Schedule Conflict Banner appears with warning
   - Conflicting class group details are shown
   - User is blocked from saving until conflict resolved
   - User can adjust time or truck to resolve conflict
   - After resolution, class group saves successfully

4. **Given** the application is running
   **When** I run E2E tests for weekly view
   **Then** all scenarios pass:
   - Calendar-style grid displays correctly
   - Conflicts are visually highlighted
   - Clicking schedule block navigates to edit

5. **Given** E2E tests are organized
   **Then** tests are in `e2e/class-groups/`

6. **Given** tests execute
   **Then** tests use seeded test data (schools, trucks, existing schedules)

7. **Given** conflict detection scenarios
   **Then** they are thoroughly tested

8. **Given** Epic 3 completion requirements
   **Then** all tests must pass for Epic 3 completion

## Tasks / Subtasks

- [ ] Task 1: Set Up E2E Test Infrastructure for Class Groups (AC: #5, #6)
  - [ ] Create `e2e/class-groups/` test directory
  - [ ] Create test fixtures with schools, trucks, existing schedules
  - [ ] Set up test database seeding with conflict scenarios

- [ ] Task 2: Implement Class Groups CRUD E2E Tests (AC: #1)
  - [ ] Test navigation to Class Groups page
  - [ ] Test class group creation with school/truck/day/time
  - [ ] Test class group editing
  - [ ] Test class group archiving
  - [ ] Test school and truck filters

- [ ] Task 3: Implement Schedule Configuration Tests (AC: #2)
  - [ ] Test day/week/time selection
  - [ ] Test truck assignment
  - [ ] Test schedule visibility in weekly view
  - [ ] Test schedule block details

- [ ] Task 4: Implement Conflict Detection Tests (AC: #3, #7)
  - [ ] Test overlapping schedule detection
  - [ ] Test Schedule Conflict Banner display
  - [ ] Test conflict details visibility
  - [ ] Test save blocking until resolution
  - [ ] Test time adjustment resolution
  - [ ] Test truck change resolution
  - [ ] Test successful save after resolution

- [ ] Task 5: Implement Weekly View Tests (AC: #4)
  - [ ] Test grid rendering
  - [ ] Test conflict highlighting
  - [ ] Test navigation from schedule block

- [ ] Task 6: Validate Epic 3 Completion (AC: #8)
  - [ ] Run full test suite
  - [ ] Document test coverage
  - [ ] Fix any failing tests

## Dev Notes

### Architecture Requirements
- **E2E Test Location**: `apps/frontend/e2e/class-groups/`
- **Test Framework**: Playwright
- **Test Configuration**: `apps/frontend/playwright.config.ts`

### Test Organization Pattern
```
e2e/class-groups/
├── scheduling.spec.ts           # Schedule configuration tests
└── conflict-detection.spec.ts   # FR6 conflict detection tests
```

### Conflict Detection Scenarios (Critical)
This is a critical FR6 requirement. Test these specific scenarios:
1. Same truck, same day, overlapping times → should trigger conflict
2. Same truck, different days → no conflict
3. Different trucks, same time → no conflict
4. Exact same time slot, same truck → should trigger conflict
5. Adjacent time slots, same truck → no conflict (edge case)

### Test Data Strategy
- Seed existing class groups with known schedules
- Include at least one "occupied" time slot per truck for conflict testing
- Clean up test data after completion

### Previous Story Context
- Stories 3-1 to 3-5 implemented Class Groups management
- Story 3-6 added data migration for seed data
- Conflict detection (Story 3-4) is implemented

### Testing Standards
- FR6 tests are marked as critical
- All conflict scenarios must pass
- Schedule Conflict Banner must be verified

### Project Structure Notes
- Align with existing E2E test patterns
- Reuse auth fixtures from auth tests

### References
- [Source: _bmad-output/planning-artifacts/architecture.md#E2E-Testing-Strategy]
- [Source: _bmad-output/planning-artifacts/epics.md#Story-3.7]

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
