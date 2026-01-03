# Story 2.3: School Entity & API Endpoints

Status: ready-for-dev

## Story

As a **developer**,
I want **the School domain entity and REST API endpoints**,
so that **school data can be managed through the API**.

## Acceptance Criteria

1. **Given** the backend project
   **When** the School entity is created
   **Then** the `School` entity exists with properties:
   - Id, Name, Address, ContactName, ContactPhone, ContactEmail, BillingSettings (JSON or relation), IsActive, Notes

2. **And** EF Core configuration and migration create the `schools` table

3. **And** `/api/schools` endpoints support:
   - GET (list schools)
   - GET `/:id` (single school with contacts)
   - POST (create school)
   - PUT `/:id` (update school)
   - DELETE `/:id` (archive school)

4. **And** endpoints require authentication

5. **And** ProblemDetails errors are returned for validation failures (FR13)

## Tasks / Subtasks

- [ ] Task 1: Create School entity (AC: #1)
  - [ ] Create School.cs in Domain/Entities
  - [ ] Add properties: Id, Name, Address, ContactName, ContactPhone, ContactEmail, Notes, IsActive, CreatedAt, UpdatedAt
  - [ ] Create BillingSettings value object or JSON column
- [ ] Task 2: Create EF Core configuration (AC: #2)
  - [ ] Create SchoolConfiguration.cs in Infrastructure/Data/Configurations
  - [ ] Configure table name as `schools`
  - [ ] Configure BillingSettings as JSON column
  - [ ] Add indexes on Name
- [ ] Task 3: Create and apply migration (AC: #2)
  - [ ] Run `dotnet ef migrations add AddSchools`
  - [ ] Review and apply migration
- [ ] Task 4: Create School DTOs (AC: #3)
  - [ ] Create SchoolDto.cs
  - [ ] Create CreateSchoolRequest.cs
  - [ ] Create UpdateSchoolRequest.cs
  - [ ] Add validation attributes for required fields
- [ ] Task 5: Create SchoolService (AC: #3)
  - [ ] Create ISchoolService interface
  - [ ] Implement CRUD operations
  - [ ] Handle soft-delete
- [ ] Task 6: Create SchoolsController (AC: #3, #4, #5)
  - [ ] Create SchoolsController with [Authorize]
  - [ ] Implement all CRUD endpoints
  - [ ] Add validation and return ProblemDetails

## Dev Notes

### Architecture Compliance

- **Entity in Domain layer** - School.cs with BillingSettings
- **Configuration in Infrastructure** - EF Core mapping
- **DTOs in Application layer** - Separate request/response models
- **Controller in Api layer** - Thin, delegates to service

### School Entity

```csharp
public class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public BillingSettings? BillingSettings { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();
}

public class BillingSettings
{
    public decimal DefaultSessionRate { get; set; }
    public string BillingCycle { get; set; } = "Monthly"; // Monthly, Termly
    public string? BillingNotes { get; set; }
}
```

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/schools` | List all active schools |
| GET | `/api/schools/{id}` | Get single school |
| POST | `/api/schools` | Create school |
| PUT | `/api/schools/{id}` | Update school |
| DELETE | `/api/schools/{id}` | Archive school |

### Validation Rules

- Name: Required, max 200 characters
- ContactEmail: Valid email format if provided
- ContactPhone: Valid phone format if provided
- DefaultSessionRate: >= 0

### File Structure

```
apps/backend/src/
├── Domain/
│   └── Entities/
│       └── School.cs
├── Application/
│   └── Schools/
│       ├── ISchoolService.cs
│       ├── SchoolDto.cs
│       ├── CreateSchoolRequest.cs
│       └── UpdateSchoolRequest.cs
├── Infrastructure/
│   ├── Data/
│   │   └── Configurations/
│   │       └── SchoolConfiguration.cs
│   └── Services/
│       └── SchoolService.cs
└── Api/
    └── Controllers/
        └── SchoolsController.cs
```

### Previous Story Dependencies

- **Story 1.1** provides: Backend scaffold
- **Story 1.2** provides: Authentication

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.3]
- [Source: docs/legacy/1_School/School.xsd] - Legacy schema reference

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
