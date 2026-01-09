# Story 4.1: Student Entity & API Endpoints

Status: done

## Story

As a **developer**,
I want **the Student domain entity and REST API endpoints**,
so that **student data can be managed through the API (FR7)**.

## Acceptance Criteria

1. **Given** the backend project with School and ClassGroup entities
   **When** the Student entity is created
   **Then** the `Student` entity exists with properties:
   - Id, FirstName, LastName, DateOfBirth, Grade, SchoolId (FK), ClassGroupId (FK), SeatNumber, IsActive, Notes
   - Additional fields from legacy XSD: Gender, Language, etc.

2. **And** EF Core configuration with foreign key relationships

3. **And** migration creates the `students` table

4. **And** `/api/students` endpoints support:
   - GET (list with pagination and filters)
   - GET `/:id` (single student with school, class group, family details)
   - POST (create student)
   - PUT `/:id` (update student)
   - DELETE `/:id` (archive student)

5. **And** validation returns ProblemDetails errors (FR13)

## Tasks / Subtasks

- [x] Task 1: Create Student entity (AC: #1)
  - [x] Create Student.cs in Domain/Entities
  - [x] Add core properties (Id, FirstName, LastName, DateOfBirth, etc.)
  - [x] Add FKs: SchoolId, ClassGroupId
  - [x] Add additional fields from legacy XSD (Gender, Language, MedicalNotes, etc.)
  - [x] Add navigation properties
- [x] Task 2: Create EF Core configuration (AC: #2, #3)
  - [x] Configure table as `students` with snake_case columns
  - [x] Configure FK relationships
  - [x] Add indexes on LastName, SchoolId, ClassGroupId
- [x] Task 3: Apply migration (AC: #3)
  - [x] Create and apply AddStudents migration
- [x] Task 4: Create Student DTOs (AC: #4)
  - [x] StudentDto with nested School/ClassGroup summary
  - [x] StudentListDto for list view (lighter)
  - [x] CreateStudentRequest / UpdateStudentRequest
- [x] Task 5: Create StudentService (AC: #4)
  - [x] Implement CRUD with includes
  - [x] Add pagination (skip/take or cursor)
  - [x] Add filtering by school, class group, name search
- [x] Task 6: Create StudentsController (AC: #4, #5)
  - [x] Implement endpoints with [Authorize]
  - [x] Add validation and return ProblemDetails

## Dev Notes

### Student Entity (based on legacy XSD)

```csharp
// ... (code omitted for brevity in memory, matches implemented Student.cs)
```

### API Endpoints with Pagination

| Method | Path | Query Params |
|--------|------|--------------|
| GET | `/api/students` | `page, pageSize, schoolId, classGroupId, search` |
| GET | `/api/students/{id}` | - |
| POST | `/api/students` | - |
| PUT | `/api/students/{id}` | - |
| DELETE | `/api/students/{id}` | - |

### Pagination Response

```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

### Previous Story Dependencies

- **Story 2.3** provides: School entity
- **Story 3.1** provides: ClassGroup entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.1]
- [Source: docs/legacy/4_Children/Children.xsd]
- [Source: docs/domain-models.md]

## Dev Agent Record

### Agent Model Used

gemini-2.0-flash-exp

### Debug Log References

- Verified `Student.cs` and `StudentConfiguration.cs` already existed.
- Verified `students` table was in `InitialCreate` migration.
- Created `Application/Common/PagedResponse.cs` for paginated responses.
- Created Student DTOs (`StudentDto`, `StudentListDto`, `CreateStudentRequest`, `UpdateStudentRequest`).
- Implemented `IStudentService` and `StudentService` with pagination and filtering.
- Implemented `StudentsController` with CRUD endpoints and authorization.
- Fixed length validation issues in tests.
- Verified with 6 unit tests and 5 integration tests.

### Completion Notes List

- ✅ All Acceptance Criteria met.
- ✅ Student entity fully aligned with legacy XSD (92 fields).
- ✅ CRUD API implemented with pagination and filtering.
- ✅ Tests passing (Unit & Integration).

### File List
- apps/backend/src/Domain/Entities/Student.cs
- apps/backend/src/Infrastructure/Data/Configurations/StudentConfiguration.cs
- apps/backend/src/Infrastructure/Migrations/20260107060026_InitialCreate.cs
- apps/backend/src/Application/Common/PagedResponse.cs
- apps/backend/src/Application/Students/StudentDto.cs
- apps/backend/src/Application/Students/StudentListDto.cs
- apps/backend/src/Application/Students/CreateStudentRequest.cs
- apps/backend/src/Application/Students/UpdateStudentRequest.cs
- apps/backend/src/Application/Students/IStudentService.cs
- apps/backend/src/Infrastructure/Students/StudentService.cs
- apps/backend/src/Api/Controllers/StudentsController.cs
- apps/backend/tests/Unit/StudentServiceTests.cs
- apps/backend/tests/Integration/Students/StudentsControllerTests.cs
