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
    // Primary identifier (auto-generated)
    public int Id { get; set; }

    // XSD Field: "Reference" (10 chars max, required) - Unique reference code
    public string Reference { get; set; } = string.Empty;

    // XSD Field: "Child_Name" (50 chars max) - Renamed to FirstName for clarity
    public string? FirstName { get; set; }

    // XSD Field: "Child_Surname" (50 chars max) - Renamed to LastName for clarity
    public string? LastName { get; set; }

    // XSD Field: "Child_birthdate" (datetime)
    public DateTime? DateOfBirth { get; set; }

    // XSD Field: "Sex" (3 chars max) - Gender (M/F)
    public string? Gender { get; set; }

    // XSD Field: "Language" (3 chars max) - Language preference (Afr/Eng)
    public string? Language { get; set; }

    // Account Person Fields (Responsible Adult)
    // XSD Field: "Account_Person_Name" (50 chars max)
    public string? AccountPersonName { get; set; }

    // XSD Field: "Account_Person_Surname" (50 chars max)
    public string? AccountPersonSurname { get; set; }

    // XSD Field: "Account_Person_Idnumber" (20 chars max)
    public string? AccountPersonIdNumber { get; set; }

    // XSD Field: "Account_Person_Cellphone" (20 chars max)
    public string? AccountPersonCellphone { get; set; }

    // XSD Field: "Account_Person_Office" (20 chars max)
    public string? AccountPersonOffice { get; set; }

    // XSD Field: "Account_Person_Home" (20 chars max)
    public string? AccountPersonHome { get; set; }

    // XSD Field: "Account_Person_Email" (100 chars max)
    public string? AccountPersonEmail { get; set; }

    // XSD Field: "Relation" (20 chars max)
    public string? Relation { get; set; }

    // Mother's Details
    // XSD Field: "Mother_Name" (50 chars max)
    public string? MotherName { get; set; }

    // XSD Field: "Mother_Surname" (50 chars max)
    public string? MotherSurname { get; set; }

    // XSD Field: "Mother_Office" (20 chars max)
    public string? MotherOffice { get; set; }

    // XSD Field: "Mother_Cell" (20 chars max)
    public string? MotherCell { get; set; }

    // XSD Field: "Mother_Home" (20 chars max)
    public string? MotherHome { get; set; }

    // XSD Field: "Mother_Email" (100 chars max)
    public string? MotherEmail { get; set; }

    // Father's Details
    // XSD Field: "Father_Name" (50 chars max)
    public string? FatherName { get; set; }

    // XSD Field: "Father_Surname" (50 chars max)
    public string? FatherSurname { get; set; }

    // XSD Field: "Father_Office" (20 chars max)
    public string? FatherOffice { get; set; }

    // XSD Field: "Father_Cell" (20 chars max)
    public string? FatherCell { get; set; }

    // XSD Field: "Father_Home" (20 chars max)
    public string? FatherHome { get; set; }

    // XSD Field: "Father_Email" (100 chars max)
    public string? FatherEmail { get; set; }

    // Address Fields
    // XSD Field: "Address1" (50 chars max)
    public string? Address1 { get; set; }

    // XSD Field: "Address2" (50 chars max)
    public string? Address2 { get; set; }

    // XSD Field: "Code" (10 chars max) - Postal code
    public string? PostalCode { get; set; }

    // Enrollment Fields
    // XSD Field: "School_Name" (50 chars max) - Denormalized school name
    public string? SchoolName { get; set; }

    // Foreign key to School entity
    public int? SchoolId { get; set; }

    // XSD Field: "Class_Group" (10 chars max) - Class group code
    public string? ClassGroupCode { get; set; }

    // Foreign key to ClassGroup entity
    public int? ClassGroupId { get; set; }

    // XSD Field: "Grade" (5 chars max)
    public string? Grade { get; set; }

    // XSD Field: "Teacher" (50 chars max)
    public string? Teacher { get; set; }

    // XSD Field: "Attending_KCOW_at" (50 chars max)
    public string? AttendingKcowAt { get; set; }

    // XSD Field: "Aftercare" (50 chars max)
    public string? Aftercare { get; set; }

    // XSD Field: "Extra" (50 chars max)
    public string? Extra { get; set; }

    // XSD Field: "Home_Time" (datetime)
    public DateTime? HomeTime { get; set; }

    // XSD Field: "Start_Classes" (datetime)
    public DateTime? StartClasses { get; set; }

    // XSD Field: "Terms" (10 chars max)
    public string? Terms { get; set; }

    // XSD Field: "Seat" (5 chars max)
    public string? Seat { get; set; }

    // XSD Field: "Truck" (3 chars max)
    public string? Truck { get; set; }

    // XSD Field: "Family" (50 chars max) - Family grouping code
    public string? Family { get; set; }

    // XSD Field: "Sequence" (50 chars max)
    public string? Sequence { get; set; }

    // Financial Fields
    // XSD Field: "Financial_Code" (10 chars max)
    public string? FinancialCode { get; set; }

    // XSD Field: "Charge" (money)
    public decimal? Charge { get; set; }

    // XSD Field: "Deposit" (50 chars max)
    public string? Deposit { get; set; }

    // XSD Field: "PayDate" (50 chars max)
    public string? PayDate { get; set; }

    // T-Shirt Order Fields (Set 1)
    // XSD Field: "Tshirt_Code" (5 chars max, required)
    public string? TshirtCode { get; set; }

    // XSD Field: "TshirtSize1" (10 chars max)
    public string? TshirtSize1 { get; set; }

    // XSD Field: "TshirtColor1" (20 chars max)
    public string? TshirtColor1 { get; set; }

    // XSD Field: "TshirtDesign1" (20 chars max)
    public string? TshirtDesign1 { get; set; }

    // T-Shirt Order Fields (Set 2)
    // XSD Field: "TshirtSize2" (10 chars max)
    public string? TshirtSize2 { get; set; }

    // XSD Field: "TshirtColor2" (20 chars max)
    public string? TshirtColor2 { get; set; }

    // XSD Field: "TshirtDesign2" (20 chars max)
    public string? TshirtDesign2 { get; set; }

    // Status & Tracking Fields
    // XSD Field: "Indicator_1" (3 chars max)
    public string? Indicator1 { get; set; }

    // XSD Field: "Indicator_2" (3 chars max)
    public string? Indicator2 { get; set; }

    // XSD Field: "General_Note" (255 chars max)
    public string? GeneralNote { get; set; }

    // XSD Field: "Print_Id_Card" (bit, required, default false)
    public bool PrintIdCard { get; set; } = false;

    // XSD Field: "AcceptTermsCond" (50 chars max)
    public string? AcceptTermsCond { get; set; }

    // XSD Field: "Status" (20 chars max)
    public string? Status { get; set; }

    // XSD Field: "SmsOrEmail" (10 chars max) - Contact preference
    public string? SmsOrEmail { get; set; }

    // XSD Field: "Photo" (attachment) - Stored as URL/path
    public string? PhotoUrl { get; set; }

    // XSD Field: "PhotoUpdated" (datetime)
    public DateTime? PhotoUpdated { get; set; }

    // Soft delete flag (not in XSD, application-level)
    public bool IsActive { get; set; } = true;

    // Audit fields (not in XSD, application-level)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
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
