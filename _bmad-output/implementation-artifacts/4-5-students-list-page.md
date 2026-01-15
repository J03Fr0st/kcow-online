# Story 4.5: Students List Page

Status: done

## Story

As an **admin**,
I want **a students list page to browse and filter all students**,
so that **I can find students by various criteria**.

## Acceptance Criteria

1. **Given** I click "Students" in the sidebar
   **When** the Students page loads
   **Then** I see a paginated table with Name, School, Grade, Class Group columns
   **And** I can filter by school and class group

2. **Given** I click "Add Student"
   **When** the create form appears
   **Then** I can create a new student record

3. **Given** I click on a student row
   **When** I navigate
   **Then** I am taken to the student profile page

## Tasks / Subtasks

- [x] Task 1: Create StudentService for frontend (AC: #1)
  - [x] Implement getStudents with pagination and filters
  - [x] Add loading state
- [x] Task 2: Create Students list page (AC: #1)
  - [x] Create students-list component
  - [x] Display table with Name, School, Grade, Class Group
  - [x] Add pagination controls
  - [x] Add school and class group filter dropdowns
- [x] Task 3: Create Student form component (AC: #2)
  - [x] Create student-form with all fields
  - [x] Include assignment section (school, class group, seat)
  - [x] Add validation
- [x] Task 4: Implement navigation to profile (AC: #3)
  - [x] On row click, navigate to `/students/{id}`
- [x] Task 5: Configure routing
  - [x] Add students feature routes
  - [x] Include profile route

## Dev Notes

### Students List Component

```typescript
interface StudentsListState {
  students: Student[];
  totalCount: number;
  page: number;
  pageSize: number;
  schoolFilter?: number;
  classGroupFilter?: number;
}
```

### Table Columns

| Column | Source |
|--------|--------|
| Name | `${lastName}, ${firstName}` |
| School | `school.name` |
| Grade | `grade` |
| Class Group | `classGroup.name` |

### File Structure

```
apps/frontend/src/app/features/students/
├── routes.ts
├── students-list/
├── student-form/
├── student-profile/  (placeholder for 4.6)
└── models/
    └── student.model.ts
```

### Previous Story Dependencies

- **Story 4.1** provides: Students API with pagination

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.5]

## Dev Agent Record

### Agent Model Used

glm-4.7

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

**Implementation Summary:**
- Student list page already existed with pagination and sorting from previous development
- Added school and class group filters to existing student-list component
- Updated StudentService to pass filter parameters to backend
- Made table rows clickable to navigate to student profile
- Backend already supported schoolId/classGroupId filtering - only frontend needed updates

**Acceptance Criteria Status:**
1. AC #1: Paginated table with filters ✅ - Added school and class group filter dropdowns
2. AC #2: Add Student button ✅ - Already existed, student-form component already in place
3. AC #3: Row click navigation ✅ - Added navigateToProfile() method

**Technical Decisions:**
- Used Angular signals for reactive filter state
- Filter dropdowns reset appropriately (class group resets when school changes)
- Table rows have cursor-pointer hover effect
- Edit button has stopPropagation to prevent navigation when clicking edit

**File List:**

**Frontend:**
- `apps/frontend/src/app/features/students/student-list/student-list.component.ts` (MODIFIED - added filter state and methods)
- `apps/frontend/src/app/features/students/student-list/student-list.component.html` (MODIFIED - added filter dropdowns and row click)
- `apps/frontend/src/app/core/services/student.service.ts` (MODIFIED - extended GetStudentsParams with schoolId, classGroupId)
