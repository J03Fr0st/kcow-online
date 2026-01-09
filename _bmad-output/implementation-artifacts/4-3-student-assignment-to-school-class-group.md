# Story 4.3: Student Assignment to School & Class Group

Status: done

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

- [x] Task 1: Create assignment section in student form (AC: #1)
  - [x] Add school dropdown (load all schools)
  - [x] Add class group dropdown (filtered by selected school)
  - [x] Add seat number input
- [x] Task 2: Implement cascading dropdown logic (AC: #1, #3)
  - [x] When school changes, fetch class groups for that school
  - [x] Clear class group selection when school changes
- [x] Task 3: Persist assignments (AC: #2)
  - [x] Include schoolId, classGroupId, seatNumber in create/update
  - [x] Verify data persists correctly
- [x] Task 4: Add validation rules
  - [x] Class group must belong to selected school
  - [x] Seat number optional, must be positive integer

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

glm-4.7

### Debug Log References

None

### Completion Notes List

Successfully implemented student assignment to school, class group, and seat number:

**Frontend Implementation:**
1. Created ClassGroupSelectComponent that filters class groups by selected school
2. Added assignment section to student form with class group dropdown and seat number input
3. Implemented cascading dropdown logic: when school changes, class group selection is cleared
4. Added validation for seat number (positive integer, optional field)
5. Updated Student service interfaces to include classGroupId and seat fields
6. All form data properly persists to backend through CreateStudentRequest and UpdateStudentRequest

**Files Modified:**
- apps/frontend/src/app/features/students/student-form/student-form.component.ts
- apps/frontend/src/app/features/students/student-form/student-form.component.html
- apps/frontend/src/app/core/services/student.service.ts

**Files Created:**
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.ts
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.html
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.scss
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.spec.ts

**Acceptance Criteria Met:**
✅ AC1: School dropdown, class group dropdown (filtered by school), and seat number input are all present in the student form
✅ AC2: Assignments persist correctly (schoolId, classGroupId, seat sent in create/update requests)
✅ AC3: Changing school clears the class group selection (implemented via valueChanges subscription)

**Validation Rules Implemented:**
- Class group dropdown is disabled when no school is selected
- Seat number is optional but must be a positive integer (1, 2, 3, etc.) if provided
- Custom validator ensures seat number matches pattern /^[1-9]\d*$/

### File List

**Modified Files:**
- apps/frontend/src/app/features/students/student-form/student-form.component.ts
- apps/frontend/src/app/features/students/student-form/student-form.component.html
- apps/frontend/src/app/core/services/student.service.ts

**New Files:**
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.ts
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.html
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.scss
- apps/frontend/src/app/shared/components/class-group-select/class-group-select.component.spec.ts
