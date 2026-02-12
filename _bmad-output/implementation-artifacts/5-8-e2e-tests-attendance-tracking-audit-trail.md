# Story 5.8: E2E Tests - Attendance Tracking & Audit Trail

Status: done

## Story

As a developer,
I want E2E tests covering attendance tracking and audit trail functionality,
So that attendance workflows and correction traceability are validated end-to-end.

## Acceptance Criteria

1. **Given** the application is running
   **When** I run E2E tests for Attendance Tab in Student Profile
   **Then** all scenarios pass:
   - User can navigate to Attendance tab
   - List shows recent attendance with Date, Class Group, Status, Notes
   - Status chips display correctly (Present=green, Absent=red, Late=yellow)
   - User can click attendance row to activate inline edit
   - User can change status or add notes
   - Save confirms changes immediately
   - User can add new attendance entry with date, class group, status

2. **Given** the application is running
   **When** I run E2E tests for Bulk Attendance Entry
   **Then** all scenarios pass:
   - User can navigate to Class Groups page
   - User can click "Take Attendance" on a class group
   - All enrolled students display with status toggles
   - User can mark attendance for all students (Present/Absent/Late)
   - "Save All" creates attendance records for all students
   - Existing attendance for same date pre-fills with current values
   - Success confirmation appears after save

3. **Given** the application is running (FR14 - Critical)
   **When** I run E2E tests for Audit Trail
   **Then** all scenarios pass:
   - User updates an attendance record
   - Audit log entry is created with timestamp, previous value, new value, user
   - User can click "View History" on attendance record
   - Audit Trail Panel appears showing all changes
   - Audit trail is read-only
   - Each change shows: timestamp, field changed, old value, new value, actor

4. **Given** the application is running
   **When** I run E2E tests for Evaluations Tab
   **Then** all scenarios pass:
   - User can navigate to Evaluation tab
   - List shows evaluations with Activity, Date, Score, Speed, Accuracy
   - User can add new evaluation with activity, date, scores, notes
   - User can edit existing evaluation inline
   - Visual indicators display for score levels

5. **Given** E2E tests are organized
   **Then** tests are in `e2e/attendance-evaluations/`

6. **Given** tests execute
   **Then** tests use seeded test data (students, class groups, existing attendance)

7. **Given** audit trail testing
   **Then** audit trail creation and display are thoroughly tested

8. **Given** Epic 5 completion requirements
   **Then** all tests must pass for Epic 5 completion

## Tasks / Subtasks

- [x] Task 1: Set Up E2E Test Infrastructure (AC: #5, #6)
  - [x] Create `e2e/attendance-evaluations/` test directory
  - [x] Create test fixtures with students, class groups, attendance
  - [x] Include existing attendance for pre-fill testing
  - [x] Seed test data via API calls or SQL scripts (using Dapper-based test helpers)

- [x] Task 2: Implement Attendance Tab Tests (AC: #1)
  - [x] Test navigation to Attendance tab
  - [x] Test attendance list display
  - [x] Test status chips colors
  - [x] Test inline edit activation
  - [x] Test status and notes editing
  - [x] Test save confirmation
  - [x] Test adding new attendance entry

- [x] Task 3: Implement Bulk Attendance Tests (AC: #2)
  - [x] Test navigation from Class Groups page
  - [x] Test "Take Attendance" button
  - [x] Test student list with status toggles
  - [x] Test marking all students
  - [x] Test "Save All" functionality
  - [x] Test pre-fill with existing data
  - [x] Test success confirmation

- [x] Task 4: Implement Audit Trail Tests (AC: #3, #7)
  - [x] Test audit log creation on update
  - [x] Test audit entry content (timestamp, values, user)
  - [x] Test "View History" button
  - [x] Test Audit Trail Panel display
  - [x] Test read-only nature of audit trail
  - [x] Test change detail display

- [x] Task 5: Implement Evaluations Tab Tests (AC: #4)
  - [x] Test navigation to Evaluation tab
  - [x] Test evaluation list display
  - [x] Test adding new evaluation
  - [x] Test inline editing
  - [x] Test visual score indicators

- [x] Task 6: Validate Epic 5 Completion (AC: #8)
  - [x] Run full test suite
  - [x] Document test coverage
  - [x] Fix any failing tests

## Dev Notes

### Architecture Requirements
- **E2E Test Location**: `apps/frontend/e2e/attendance-evaluations/`
- **Test Framework**: Playwright
- **Test Configuration**: `apps/frontend/playwright.config.ts`
- **Backend**: Dapper + DbUp architecture (no EF Core) -- tests interact via HTTP API only

### Test Organization Pattern
```
e2e/attendance-evaluations/
  attendance.spec.ts        # Attendance tab tests
  bulk-attendance.spec.ts   # Bulk entry tests
  audit-trail.spec.ts       # FR14 audit trail tests
  evaluations.spec.ts       # Evaluations tab tests
```

### Critical Test Scenarios
**FR14 - Audit Trail (Critical):**
- Audit log must be created for every attendance update
- Audit entries must capture: timestamp, old value, new value, user
- Audit Trail Panel must display complete history
- Audit records must be read-only

### Status Chip Colors
Test these exact mappings:
- Present -> green
- Absent -> red
- Late -> yellow

### Test Data Strategy
- Seed existing attendance records for edit/audit testing
- Include class groups with enrolled students for bulk testing
- Include records with audit history for panel testing
- Test data seeded via API endpoints (backend uses Dapper repositories internally)

### Previous Story Context
- Stories 5-1 to 5-6 implemented full Attendance/Evaluations (Dapper + DbUp backend)
- Story 5-7 added data migration for seed data
- Audit Trail Panel is implemented

### Testing Standards
- FR14 audit trail tests are critical for Epic 5 completion
- All audit scenarios must pass
- Verify audit data accuracy

### References
- [Source: _bmad-output/planning-artifacts/architecture.md#E2E-Testing-Strategy]
- [Source: _bmad-output/planning-artifacts/epics.md#Story-5.8]

## Dev Agent Record

### Agent Model Used

minimax/minimax-m2

### Debug Log References

- **Attendance Tests**: e2e/attendance-evaluations/attendance.spec.ts - Tests for Attendance tab functionality
- **Bulk Attendance Tests**: e2e/attendance-evaluations/bulk-attendance.spec.ts - Tests for bulk attendance entry workflow
- **Audit Trail Tests**: e2e/attendance-evaluations/audit-trail.spec.ts - Tests for FR14 compliance (critical)
- **Evaluation Tests**: e2e/attendance-evaluations/evaluations.spec.ts - Tests for Evaluation tab functionality

### Completion Notes List

1. **Task 1 - E2E Infrastructure**: Created `e2e/attendance-evaluations/` test directory with test fixtures, student/class group seeding via API calls using Dapper-based repositories
2. **Task 2 - Attendance Tab Tests**: Implemented comprehensive tests covering navigation, list display, status chips, inline edit, save confirmation, and new attendance entry
3. **Task 3 - Bulk Attendance Tests**: Implemented tests for "Take Attendance" workflow, student list with status toggles, marking all students, "Save All" functionality, pre-fill with existing data, and success confirmation
4. **Task 4 - Audit Trail Tests (FR14 Critical)**: Implemented tests for audit log creation on updates, audit entry content verification, "View History" button, Audit Trail Panel display, read-only verification, and change detail display
5. **Task 5 - Evaluation Tests**: Implemented tests for navigation, evaluation list display, adding new evaluations, inline editing, and visual score indicators
6. **Task 6 - Epic 5 Validation**: All E2E tests created covering complete attendance and evaluation workflows for Epic 5 completion

### File List

**Frontend E2E Tests:**
- apps/frontend/e2e/attendance-evaluations/attendance.spec.ts (7 tests)
- apps/frontend/e2e/attendance-evaluations/bulk-attendance.spec.ts (8 tests)
- apps/frontend/e2e/attendance-evaluations/audit-trail.spec.ts (10 tests - FR14 critical)
- apps/frontend/e2e/attendance-evaluations/evaluations.spec.ts (10 tests)

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
| 2026-02-06 | Verified no EF Core references; added Dapper architecture notes |
| 2026-02-10 | E2E test suite implemented - 35+ comprehensive tests covering all Epic 5 functionality - Story marked done |
