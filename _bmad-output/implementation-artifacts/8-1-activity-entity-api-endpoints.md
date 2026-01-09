# Story 8.1: Activity Entity & API Endpoints

Status: ready-for-dev

## Story

As a developer,
I want the Activity domain entity and REST API endpoints,
so that activity data can be managed through the API.

## Acceptance Criteria

1. **Given** the backend project, **When** the Activity entity is created, **Then** the `Activity` entity exists in `Domain/Entities` with ALL XSD fields:
   - Id (ActivityID - int, required, primary key)
   - Code (Program - nvarchar 255, optional)
   - Name (ProgramName - nvarchar 255, optional)
   - Description (Educational_Focus - ntext/memo, optional)
   - Folder (nvarchar 255, optional)
   - GradeLevel (Grade - nvarchar 255, optional)
   - Icon (base64Binary/OLE object - stored as TEXT, optional)
   - IsActive (bool, for soft-delete)
   - CreatedAt (DateTime)
   - UpdatedAt (DateTime, nullable)

2. **And** EF Core configuration exists in `Infrastructure/Data/ActivityConfiguration.cs`
3. **And** migration creates the `activities` table with correct constraints
4. **And** the Icon field is stored as TEXT type in SQLite (large enough for base64 OLE object data)
5. **And** `/api/activities` endpoints support:
   - GET (list all active activities)
   - GET `/:id` (get single activity)
   - POST (create activity)
   - PUT `/:id` (update activity)
   - DELETE `/:id` (archive/soft-delete activity)
6. **And** all endpoints return ProblemDetails on error
7. **And** endpoints require authentication

## Tasks / Subtasks

- [ ] Task 1: Create Activity Domain Entity (AC: #1)
  - [ ] Create `Activity.cs` in `Domain/Entities/`
  - [ ] Include ALL 7 XSD fields plus IsActive, CreatedAt, UpdatedAt
  - [ ] Use English field names per architecture (Program → Code, etc.)

- [ ] Task 2: Create EF Core Configuration (AC: #2, #3, #4)
  - [ ] Create `ActivityConfiguration.cs` in `Infrastructure/Data/`
  - [ ] Configure table name `activities` (snake_case)
  - [ ] Configure field lengths per XSD (255 chars for Code, Name, Folder, GradeLevel)
  - [ ] Configure Icon as TEXT type for large base64 data
  - [ ] Configure soft-delete query filter (IsActive)

- [ ] Task 3: Register Entity in DbContext (AC: #2)
  - [ ] Add `DbSet<Activity>` to `AppDbContext.cs`
  - [ ] Register ActivityConfiguration

- [ ] Task 4: Create Migration (AC: #3)
  - [ ] Run `dotnet ef migrations add AddActivityEntity`
  - [ ] Verify migration creates `activities` table correctly

- [ ] Task 5: Create Application DTOs and Requests (AC: #5)
  - [ ] Create `ActivityDto.cs` in `Application/Activities/`
  - [ ] Create `CreateActivityRequest.cs`
  - [ ] Create `UpdateActivityRequest.cs`

- [ ] Task 6: Create Activity Service Interface (AC: #5)
  - [ ] Create `IActivityService.cs` in `Application/Activities/`
  - [ ] Define GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, ArchiveAsync

- [ ] Task 7: Implement Activity Service (AC: #5, #6)
  - [ ] Create `ActivityService.cs` in `Infrastructure/Activities/`
  - [ ] Implement all CRUD operations
  - [ ] Follow TruckService patterns exactly
  - [ ] Check for duplicate Code on create/update

- [ ] Task 8: Create Activities Controller (AC: #5, #6, #7)
  - [ ] Create `ActivitiesController.cs` in `Api/Controllers/`
  - [ ] Implement GET, GET/:id, POST, PUT/:id, DELETE/:id
  - [ ] Add [Authorize] attribute
  - [ ] Return ProblemDetails for errors

- [ ] Task 9: Register Services in DI (AC: #5)
  - [ ] Register IActivityService/ActivityService in Program.cs

- [ ] Task 10: Test API Endpoints (AC: #5, #6, #7)
  - [ ] Test all endpoints with Swagger/Postman
  - [ ] Verify auth is required
  - [ ] Verify ProblemDetails on errors

## Dev Notes

### XSD Field Mapping (CRITICAL - from docs/legacy/3_Activity/Activity.xsd)

| XSD Field | Entity Field | Type | Max Length | Required |
|-----------|-------------|------|------------|----------|
| ActivityID | Id | int | - | Yes (PK) |
| Program | Code | string | 255 | No |
| ProgramName | Name | string | 255 | No |
| Educational_Focus | Description | string | ntext/memo | No |
| Folder | Folder | string | 255 | No |
| Grade | GradeLevel | string | 255 | No |
| Icon | Icon | string (base64) | TEXT | No |

### Architecture Compliance

- **Follow Truck entity patterns exactly** - Activity is a simple maintenance entity
- **Service Pattern**: ITruckService/TruckService pattern for IActivityService/ActivityService
- **Controller Pattern**: TrucksController pattern for ActivitiesController
- **DTO Pattern**: TruckDto pattern for ActivityDto

### File Locations

| Component | Path |
|-----------|------|
| Entity | `apps/backend/src/Domain/Entities/Activity.cs` |
| Configuration | `apps/backend/src/Infrastructure/Data/ActivityConfiguration.cs` |
| DTOs | `apps/backend/src/Application/Activities/` |
| Service Interface | `apps/backend/src/Application/Activities/IActivityService.cs` |
| Service Impl | `apps/backend/src/Infrastructure/Activities/ActivityService.cs` |
| Controller | `apps/backend/src/Api/Controllers/ActivitiesController.cs` |

### Code Patterns (Follow Existing)

**Entity Pattern** (from Truck.cs):
```csharp
namespace Kcow.Domain.Entities;

public class Activity
{
    public int Id { get; set; }
    public string? Code { get; set; }  // Program from XSD
    public string? Name { get; set; }  // ProgramName from XSD
    public string? Description { get; set; }  // Educational_Focus from XSD
    public string? Folder { get; set; }
    public string? GradeLevel { get; set; }  // Grade from XSD
    public string? Icon { get; set; }  // base64 OLE object
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**DTO Pattern** (from TruckDto.cs):
```csharp
namespace Kcow.Application.Activities;

public class ActivityDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Folder { get; set; }
    public string? GradeLevel { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Testing Requirements

- Unit tests for ActivityService (mock DbContext)
- Integration tests for ActivitiesController (in-memory SQLite)
- Test file: `apps/backend/tests/Integration/Activities/ActivitiesControllerTests.cs`

### Project Structure Notes

- Activity feature module follows Epic 2 Trucks pattern
- API path: `/api/activities` (plural kebab-case)
- Database table: `activities` (snake_case)

### References

- [Source: docs/legacy/3_Activity/Activity.xsd] - Complete XSD field definitions
- [Source: _bmad-output/planning-artifacts/architecture.md#Implementation-Patterns] - Naming conventions
- [Source: apps/backend/src/Infrastructure/Trucks/TruckService.cs] - Service implementation pattern
- [Source: docs/project_context.md#XSD-Alignment] - XSD compliance rules

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
