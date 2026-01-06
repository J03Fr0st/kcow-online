# Story 5.8: E2E Tests - Attendance Tracking & Audit Trail

Status: ready-for-dev

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

- [ ] Task 1: Set Up E2E Test Infrastructure (AC: #5, #6)
  - [ ] Create `e2e/attendance-evaluations/` test directory
  - [ ] Create test fixtures with students, class groups, attendance
  - [ ] Include existing attendance for pre-fill testing

- [ ] Task 2: Implement Attendance Tab Tests (AC: #1)
  - [ ] Test navigation to Attendance tab
  - [ ] Test attendance list display
  - [ ] Test status chips colors
  - [ ] Test inline edit activation
  - [ ] Test status and notes editing
  - [ ] Test save confirmation
  - [ ] Test adding new attendance entry

- [ ] Task 3: Implement Bulk Attendance Tests (AC: #2)
  - [ ] Test navigation from Class Groups page
  - [ ] Test "Take Attendance" button
  - [ ] Test student list with status toggles
  - [ ] Test marking all students
  - [ ] Test "Save All" functionality
  - [ ] Test pre-fill with existing data
  - [ ] Test success confirmation

- [ ] Task 4: Implement Audit Trail Tests (AC: #3, #7)
  - [ ] Test audit log creation on update
  - [ ] Test audit entry content (timestamp, values, user)
  - [ ] Test "View History" button
  - [ ] Test Audit Trail Panel display
  - [ ] Test read-only nature of audit trail
  - [ ] Test change detail display

- [ ] Task 5: Implement Evaluations Tab Tests (AC: #4)
  - [ ] Test navigation to Evaluation tab
  - [ ] Test evaluation list display
  - [ ] Test adding new evaluation
  - [ ] Test inline editing
  - [ ] Test visual score indicators

- [ ] Task 6: Validate Epic 5 Completion (AC: #8)
  - [ ] Run full test suite
  - [ ] Document test coverage
  - [ ] Fix any failing tests

## Dev Notes

### Architecture Requirements
- **E2E Test Location**: `apps/frontend/e2e/attendance-evaluations/`
- **Test Framework**: Playwright
- **Test Configuration**: `apps/frontend/playwright.config.ts`

### Test Organization Pattern
```
e2e/attendance-evaluations/
├── attendance.spec.ts        # Attendance tab tests
├── bulk-attendance.spec.ts   # Bulk entry tests
└── audit-trail.spec.ts       # FR14 audit trail tests
```

### Critical Test Scenarios
**FR14 - Audit Trail (Critical):**
- Audit log must be created for every attendance update
- Audit entries must capture: timestamp, old value, new value, user
- Audit Trail Panel must display complete history
- Audit records must be read-only

### Status Chip Colors
Test these exact mappings:
- Present → green
- Absent → red
- Late → yellow

### Test Data Strategy
- Seed existing attendance records for edit/audit testing
- Include class groups with enrolled students for bulk testing
- Include records with audit history for panel testing

### Previous Story Context
- Stories 5-1 to 5-6 implemented full Attendance/Evaluations
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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
