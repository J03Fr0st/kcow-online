# Story 4.3: Student Assignment to School & Class Group

Status: ready-for-dev

## Story

As an **admin**,
I want **to assign a student to a school, class group, and seat**,
so that **the student's schedule and location are tracked (FR8)**.

## Acceptance Criteria

1. **Given** I am creating or editing a student
   **When** I view the assignment section
   **Then** I can select a school from a dropdown
   **And** I can select a class group (filtered by school)
   **And** I can enter a seat number

2. **Given** I save the student with assignments
   **When** the form submits
   **Then** the assignments are persisted
   **And** the student appears in the class group roster

3. **And** changing school clears the class group selection (validation)

## Tasks / Subtasks

- [ ] Task 1: Create assignment section in student form (AC: #1)
  - [ ] Add school dropdown (load all schools)
  - [ ] Add class group dropdown (filtered by selected school)
  - [ ] Add seat number input
- [ ] Task 2: Implement cascading dropdown logic (AC: #1, #3)
  - [ ] When school changes, fetch class groups for that school
  - [ ] Clear class group selection when school changes
- [ ] Task 3: Persist assignments (AC: #2)
  - [ ] Include schoolId, classGroupId, seatNumber in create/update
  - [ ] Verify data persists correctly
- [ ] Task 4: Add validation rules
  - [ ] Class group must belong to selected school
  - [ ] Seat number optional, must be positive integer

## Dev Notes

### Cascading Dropdown Logic

```typescript
// In student-form component
schoolControl.valueChanges.subscribe(schoolId => {
  classGroupControl.reset();
  if (schoolId) {
    this.loadClassGroupsForSchool(schoolId);
  } else {
    this.classGroups.set([]);
  }
});
```

### Form Fields

| Field | Type | Validation |
|-------|------|------------|
| schoolId | Select | Optional |
| classGroupId | Select | Optional, must match school |
| seatNumber | Number | Optional, >= 1 |

### Previous Story Dependencies

- **Story 4.1** provides: Student entity and API
- **Story 2.4** provides: Schools for dropdown
- **Story 3.2** provides: Class groups for dropdown

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.3]
- [Source: _bmad-output/planning-artifacts/prd.md#FR8]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
