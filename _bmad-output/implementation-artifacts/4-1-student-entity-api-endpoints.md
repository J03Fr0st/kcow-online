# Story 4.1: Student Entity & API Endpoints

Status: ready-for-dev

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

- [ ] Task 1: Create Student entity (AC: #1)
  - [ ] Create Student.cs in Domain/Entities
  - [ ] Add core properties (Id, FirstName, LastName, DateOfBirth, etc.)
  - [ ] Add FKs: SchoolId, ClassGroupId
  - [ ] Add additional fields from legacy XSD (Gender, Language, MedicalNotes, etc.)
  - [ ] Add navigation properties
- [ ] Task 2: Create EF Core configuration (AC: #2, #3)
  - [ ] Configure table as `students` with snake_case columns
  - [ ] Configure FK relationships
  - [ ] Add indexes on LastName, SchoolId, ClassGroupId
- [ ] Task 3: Apply migration (AC: #3)
  - [ ] Create and apply AddStudents migration
- [ ] Task 4: Create Student DTOs (AC: #4)
  - [ ] StudentDto with nested School/ClassGroup summary
  - [ ] StudentListDto for list view (lighter)
  - [ ] CreateStudentRequest / UpdateStudentRequest
- [ ] Task 5: Create StudentService (AC: #4)
  - [ ] Implement CRUD with includes
  - [ ] Add pagination (skip/take or cursor)
  - [ ] Add filtering by school, class group, name search
- [ ] Task 6: Create StudentsController (AC: #4, #5)
  - [ ] Implement endpoints with [Authorize]
  - [ ] Add validation and return ProblemDetails

## Dev Notes

### Student Entity (based on legacy XSD)

```csharp
public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Grade { get; set; }
    public string? Gender { get; set; }
    public string? Language { get; set; }
    public string? MedicalNotes { get; set; }
    
    // Assignment
    public int? SchoolId { get; set; }
    public int? ClassGroupId { get; set; }
    public int? SeatNumber { get; set; }
    
    // Status
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public School? School { get; set; }
    public ClassGroup? ClassGroup { get; set; }
    public ICollection<StudentFamily> StudentFamilies { get; set; } = new List<StudentFamily>();
}
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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
