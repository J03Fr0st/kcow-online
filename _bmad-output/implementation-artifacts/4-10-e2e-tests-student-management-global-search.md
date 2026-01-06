# Story 4.10: E2E Tests - Student Management & Global Search

Status: ready-for-dev

## Story

As a developer,
I want E2E tests covering student management and the global search workflow,
So that the primary user journey (student lookup and profile) is validated end-to-end.

## Acceptance Criteria

1. **Given** the application is running (FR11 - Critical)
   **When** I run E2E tests for Global Student Search
   **Then** all scenarios pass:
   - User can access global search bar from any page
   - Typing in search shows typeahead results with students
   - Each result shows: Full Name, School, Grade, Class Group
   - Search disambiguates students with similar names
   - Selecting a result navigates to student profile
   - Search returns results in under 2 seconds (NFR1 validation)
   - "No results found" displays when no matches

2. **Given** the application is running
   **When** I run E2E tests for student profile navigation
   **Then** all scenarios pass:
   - User can navigate to student profile from list or search
   - Profile loads with 3-column header (photo, demographics, school, status)
   - Page loads in under 2 seconds (NFR2 validation)

3. **Given** the application is running (FR10 - Critical)
   **When** I run E2E tests for single-screen student profile
   **Then** all scenarios pass:
   - Profile header shows: basic info, school assignment, status indicators
   - Tabbed sections visible: Child Info, Financial, Attendance, Evaluation
   - User can switch between tabs without page reload
   - Inline editing works in Child Info tab
   - Save confirmation appears after edits
   - Validation errors display inline

4. **Given** the application is running
   **When** I run E2E tests for student CRUD
   **Then** all scenarios pass:
   - User can create new student with school/class group/seat assignment
   - Class group dropdown filters by selected school
   - User can edit student demographics
   - User can archive a student

5. **Given** the application is running
   **When** I run E2E tests for family management
   **Then** all scenarios pass:
   - Family Grid displays at bottom of profile
   - User can add family member with relationship type
   - User can edit family contact information
   - Sibling students show in Family Grid

6. **Given** E2E tests are organized
   **Then** tests are in `e2e/students/`

7. **Given** tests execute
   **Then** tests use seeded test data (schools, class groups, students, families)

8. **Given** performance testing
   **Then** global search performance is validated

9. **Given** Epic 4 completion requirements
   **Then** all tests must pass for Epic 4 completion

## Tasks / Subtasks

- [ ] Task 1: Set Up E2E Test Infrastructure for Students (AC: #6, #7)
  - [ ] Create `e2e/students/` test directory
  - [ ] Create test fixtures with schools, class groups, students, families
  - [ ] Include students with similar names for disambiguation testing

- [ ] Task 2: Implement Global Search E2E Tests (AC: #1, #8)
  - [ ] Test search bar accessibility from all pages
  - [ ] Test typeahead results display
  - [ ] Test result details (name, school, grade, class group)
  - [ ] Test disambiguation of similar names
  - [ ] Test navigation to profile on selection
  - [ ] Test performance <2 seconds (NFR1)
  - [ ] Test "No results found" display

- [ ] Task 3: Implement Profile Navigation Tests (AC: #2)
  - [ ] Test navigation from student list
  - [ ] Test navigation from search
  - [ ] Test 3-column header display
  - [ ] Test performance <2 seconds (NFR2)

- [ ] Task 4: Implement Single-Screen Profile Tests (AC: #3)
  - [ ] Test profile header content
  - [ ] Test tabbed sections visibility
  - [ ] Test tab switching without reload
  - [ ] Test inline editing in Child Info
  - [ ] Test save confirmation
  - [ ] Test inline validation errors

- [ ] Task 5: Implement Student CRUD Tests (AC: #4)
  - [ ] Test student creation with assignments
  - [ ] Test class group filtering by school
  - [ ] Test student editing
  - [ ] Test student archiving

- [ ] Task 6: Implement Family Management Tests (AC: #5)
  - [ ] Test Family Grid display
  - [ ] Test adding family member
  - [ ] Test editing family contact
  - [ ] Test sibling display

- [ ] Task 7: Validate Epic 4 Completion (AC: #9)
  - [ ] Run full test suite
  - [ ] Document test coverage
  - [ ] Fix any failing tests

## Dev Notes

### Architecture Requirements
- **E2E Test Location**: `apps/frontend/e2e/students/`
- **Test Framework**: Playwright
- **Test Configuration**: `apps/frontend/playwright.config.ts`

### Test Organization Pattern
```
e2e/students/
├── global-search.spec.ts      # FR11 search tests
├── student-profile.spec.ts    # FR10 profile tests
└── family-management.spec.ts  # Family grid tests
```

### Critical Test Scenarios
**FR11 - Global Search (Critical):**
- Performance must be validated: search results in <2 seconds
- Disambiguation is critical for similar names (show school/grade)

**FR10 - Single-Screen Profile (Critical):**
- Page load performance: <2 seconds
- Tab switching must not trigger full reload
- Inline editing must work seamlessly

### NFR Performance Testing
- Use Playwright's timing assertions
- Measure from search input to results display
- Measure from navigation to profile fully loaded
- Target: <2000ms for both scenarios

### Test Data Strategy
- Include 10-15 sample students
- Include students with similar names (e.g., "John Smith" at different schools)
- Include students with families and siblings
- Include students in different class groups

### Previous Story Context
- Stories 4-1 to 4-8 implemented full Student management
- Story 4-9 added data migration for seed data
- Global search and profile UI are functional

### Testing Standards
- FR10 and FR11 tests are critical for Epic 4 completion
- Performance targets must be validated
- All search scenarios must pass

### References
- [Source: _bmad-output/planning-artifacts/architecture.md#E2E-Testing-Strategy]
- [Source: _bmad-output/planning-artifacts/epics.md#Story-4.10]

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
