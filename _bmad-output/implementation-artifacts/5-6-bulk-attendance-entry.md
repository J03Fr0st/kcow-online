# Story 5.6: Bulk Attendance Entry

Status: ready-for-dev

## Story

As an **admin**,
I want **to record attendance for an entire class group session at once**,
so that **I can efficiently mark attendance for a class**.

## Acceptance Criteria

1. **Given** I am on the Class Groups page
   **When** I click "Take Attendance" on a class group
   **Then** I see a list of all students in the class
   **And** each student has a status toggle (Present/Absent/Late)

2. **Given** I mark attendance for all students
   **When** I click "Save All"
   **Then** attendance records are created for all students
   **And** I see a success confirmation

3. **And** existing attendance for the same date shows pre-filled values

## Tasks / Subtasks

- [ ] Task 1: Add "Take Attendance" action (AC: #1)
  - [ ] Add button to class group row or detail view
  - [ ] Navigate to bulk attendance view
- [ ] Task 2: Create BulkAttendance component (AC: #1)
  - [ ] Load students in class group
  - [ ] Display list with name and status toggle
  - [ ] Default all to "Present"
- [ ] Task 3: Check for existing attendance (AC: #3)
  - [ ] Load attendance for selected date
  - [ ] Pre-fill existing statuses
  - [ ] Indicate which are updates vs new
- [ ] Task 4: Implement bulk save (AC: #2)
  - [ ] Create/update attendance for all students
  - [ ] Use batch endpoint or sequential calls
  - [ ] Show progress and success

## Dev Notes

### Bulk Attendance Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Take Attendance: Class 5A - Monday 2026-01-03               │
│ Date: [2026-01-03 ▼]                                        │
├─────────────────────────────────────────────────────────────┤
│ Student          │ Present │ Absent │ Late │ Notes          │
│ John Smith       │   (●)   │   ○    │  ○   │ [_______]      │
│ Jane Doe         │   ○     │  (●)   │  ○   │ [Sick_____]    │
│ Tom Wilson       │   ○     │   ○    │ (●)  │ [Late 10min]   │
├─────────────────────────────────────────────────────────────┤
│                                           [Cancel] [Save All]│
└─────────────────────────────────────────────────────────────┘
```

### Batch Attendance API

Consider adding a batch endpoint:
```
POST /api/class-groups/{id}/attendance
Body: { sessionDate: "2026-01-03", entries: [...] }
```

### Previous Story Dependencies

- **Story 5.1** provides: Attendance API
- **Story 3.2** provides: Class groups with student roster

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.6]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
