# Story 4.5: Students List Page

Status: ready-for-dev

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

- [ ] Task 1: Create StudentService for frontend (AC: #1)
  - [ ] Implement getStudents with pagination and filters
  - [ ] Add loading state
- [ ] Task 2: Create Students list page (AC: #1)
  - [ ] Create students-list component
  - [ ] Display table with Name, School, Grade, Class Group
  - [ ] Add pagination controls
  - [ ] Add school and class group filter dropdowns
- [ ] Task 3: Create Student form component (AC: #2)
  - [ ] Create student-form with all fields
  - [ ] Include assignment section (school, class group, seat)
  - [ ] Add validation
- [ ] Task 4: Implement navigation to profile (AC: #3)
  - [ ] On row click, navigate to `/students/{id}`
- [ ] Task 5: Configure routing
  - [ ] Add students feature routes
  - [ ] Include profile route

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
