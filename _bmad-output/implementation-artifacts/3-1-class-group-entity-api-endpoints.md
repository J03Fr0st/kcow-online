# Story 3.1: Class Group Entity & API Endpoints

Status: review

## Story

As a **developer**,
I want **the ClassGroup domain entity and REST API endpoints**,
so that **class group data can be managed through the API**.

## Acceptance Criteria

1. **Given** the backend project with School and Truck entities
   **When** the ClassGroup entity is created
   **Then** the `ClassGroup` entity exists with properties:
   - Id, Name, SchoolId (FK), TruckId (FK), DayOfWeek, StartTime, EndTime, Sequence, IsActive, Notes

2. **And** EF Core configuration with foreign key relationships to School and Truck

3. **And** migration creates the `class_groups` table

4. **And** `/api/class-groups` endpoints support:
   - GET (list class groups with optional school/truck filters)
   - GET `/:id` (single class group with school and truck details)
   - POST (create class group)
   - PUT `/:id` (update class group)
   - DELETE `/:id` (archive class group)

5. **And** endpoints require authentication

6. **And** ProblemDetails errors for validation failures

## Tasks / Subtasks

- [x] Task 1: Create ClassGroup entity (AC: #1)
  - [x] Create ClassGroup.cs in Domain/Entities
  - [x] Add properties and foreign keys
  - [x] Add navigation properties to School and Truck
- [x] Task 2: Create EF Core configuration (AC: #2, #3)
  - [x] Create ClassGroupConfiguration.cs
  - [x] Configure table name as `class_groups`
  - [x] Configure FK relationships
  - [x] Add composite index on SchoolId + DayOfWeek + StartTime
- [x] Task 3: Apply migration (AC: #3)
  - [x] Create and apply AddClassGroups migration
- [x] Task 4: Create DTOs (AC: #4)
  - [x] Create ClassGroupDto with nested School/Truck info
  - [x] Create CreateClassGroupRequest
  - [x] Create UpdateClassGroupRequest
- [x] Task 5: Create ClassGroupService (AC: #4)
  - [x] Implement CRUD with filtering
  - [x] Include School and Truck in queries
- [x] Task 6: Create ClassGroupsController (AC: #4, #5, #6)
  - [x] Implement endpoints with [Authorize]
  - [x] Add query params for filtering
  - [x] Return ProblemDetails on errors

## Dev Notes

### ClassGroup Entity

```csharp
public class ClassGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SchoolId { get; set; }
    public int? TruckId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Sequence { get; set; } = 1;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public School School { get; set; } = null!;
    public Truck? Truck { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
```

### API Endpoints with Filtering

| Method | Path | Query Params | Description |
|--------|------|--------------|-------------|
| GET | `/api/class-groups` | `?schoolId=&truckId=` | List with filters |
| GET | `/api/class-groups/{id}` | - | Single with details |
| POST | `/api/class-groups` | - | Create |
| PUT | `/api/class-groups/{id}` | - | Update |
| DELETE | `/api/class-groups/{id}` | - | Archive |

### Previous Story Dependencies

- **Story 2.1** provides: Truck entity
- **Story 2.3** provides: School entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.1]
- [Source: docs/legacy/2_Class_Group/Class Group.xsd]

## Dev Agent Record

### Agent Model Used

Claude 4 (Sonnet)

### Debug Log References

No critical issues encountered. Implementation proceeded smoothly following existing patterns from Truck and School entities.

### Completion Notes List

✅ Completed Story 3.1: Class Group Entity & API Endpoints

**Implementation Summary:**
- Created ClassGroup entity with all required properties and navigation properties
- Configured EF Core with proper foreign key relationships to School and Truck entities
- Created migration (AddClassGroups) for class_groups and Students tables
- Implemented complete CRUD API with filtering support (by schoolId and truckId)
- All endpoints require JWT authentication ([Authorize] attribute)
- Proper ProblemDetails error responses for validation failures
- Service registered in DI container following established patterns

**Key Files Created/Modified:**
- Domain/Entities/ClassGroup.cs - Entity definition
- Domain/Entities/Student.cs - Minimal Student entity for navigation
- Infrastructure/Data/Configurations/ClassGroupConfiguration.cs - EF Core configuration
- Infrastructure/Data/AppDbContext.cs - Added ClassGroups DbSet
- Application/ClassGroups/ - Complete DTO layer (Dto, Request models, Service interface)
- Infrastructure/ClassGroups/ClassGroupService.cs - Service implementation with CRUD + filtering
- Api/Controllers/ClassGroupsController.cs - REST API controller
- Infrastructure/DependencyInjection.cs - Registered ClassGroupService

**Database Changes:**
- Created class_groups table with proper indexes
- Created Students table (minimal for now, will be expanded in Epic 4)
- Migration generated and manually applied to development database

**Acceptance Criteria Status:**
✅ AC1: ClassGroup entity with all properties created
✅ AC2: EF Core configuration with FK relationships configured
✅ AC3: Migration created and database tables created
✅ AC4: All CRUD endpoints implemented with filtering support
✅ AC5: All endpoints protected with [Authorize]
✅ AC6: ProblemDetails returned for validation errors

### File List

**New Files:**
- apps/backend/src/Domain/Entities/ClassGroup.cs
- apps/backend/src/Domain/Entities/Student.cs
- apps/backend/src/Infrastructure/Data/Configurations/ClassGroupConfiguration.cs
- apps/backend/src/Infrastructure/Migrations/20260106134322_AddClassGroups.cs
- apps/backend/src/Infrastructure/Migrations/20260106134322_AddClassGroups.Designer.cs
- apps/backend/src/Application/ClassGroups/ClassGroupDto.cs
- apps/backend/src/Application/ClassGroups/CreateClassGroupRequest.cs
- apps/backend/src/Application/ClassGroups/UpdateClassGroupRequest.cs
- apps/backend/src/Application/ClassGroups/IClassGroupService.cs
- apps/backend/src/Infrastructure/ClassGroups/ClassGroupService.cs
- apps/backend/src/Api/Controllers/ClassGroupsController.cs

**Modified Files:**
- apps/backend/src/Infrastructure/Data/AppDbContext.cs
- apps/backend/src/Infrastructure/DependencyInjection.cs
- apps/backend/src/Infrastructure/Migrations/AppDbContextModelSnapshot.cs

## Change Log

### 2026-01-06
- Implemented Story 3.1: Class Group Entity & API Endpoints
- Created ClassGroup entity with full CRUD operations
- Added filtering support for school and truck
- Status changed to: review
