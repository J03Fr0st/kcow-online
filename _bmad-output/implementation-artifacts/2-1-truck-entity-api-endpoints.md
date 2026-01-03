# Story 2.1: Truck Entity & API Endpoints

Status: done

## Story

As a **developer**,
I want **the Truck domain entity and REST API endpoints**,
so that **truck data can be managed through the API**.

## Acceptance Criteria

1. **Given** the backend project from Epic 1
   **When** the Truck entity is created
   **Then** the `Truck` entity exists in `Domain/Entities` with properties:
   - Id, Name, RegistrationNumber, Status, Notes

2. **And** EF Core configuration exists in `Infrastructure/Data`

3. **And** migration creates the `trucks` table

4. **And** `/api/trucks` endpoints support:
   - GET (list all trucks)
   - GET `/:id` (get single truck)
   - POST (create truck)
   - PUT `/:id` (update truck)
   - DELETE `/:id` (archive/soft-delete truck)

5. **And** all endpoints return ProblemDetails on error

6. **And** endpoints require authentication

## Tasks / Subtasks

- [x] Task 1: Create Truck entity (AC: #1)
  - [x] Create Truck.cs in Domain/Entities
  - [x] Add properties: Id, Name, RegistrationNumber, Status, Notes, IsActive, CreatedAt, UpdatedAt
  - [x] Use appropriate data types
- [x] Task 2: Create EF Core configuration (AC: #2)
  - [x] Create TruckConfiguration.cs in Infrastructure/Data/Configurations
  - [x] Configure table name as `trucks` (snake_case)
  - [x] Configure column mappings with snake_case
  - [x] Add required constraints and indexes
- [x] Task 3: Create and apply migration (AC: #3)
  - [x] Run `dotnet ef migrations add AddTrucks`
  - [x] Review generated migration
  - [x] Apply migration with `dotnet ef database update`
- [x] Task 4: Create Truck DTOs (AC: #4)
  - [x] Create TruckDto.cs in Application/Trucks
  - [x] Create CreateTruckRequest.cs
  - [x] Create UpdateTruckRequest.cs
  - [x] Add validation attributes
- [x] Task 5: Create TruckService (AC: #4)
  - [x] Create ITruckService interface in Application/Trucks
  - [x] Create TruckService implementation in Infrastructure
  - [x] Implement CRUD operations
  - [x] Handle soft-delete (IsActive = false)
- [x] Task 6: Create TrucksController (AC: #4, #5, #6)
  - [x] Create TrucksController in Api/Controllers
  - [x] Add [Authorize] attribute
  - [x] Implement GET endpoints (list, single)
  - [x] Implement POST endpoint (create)
  - [x] Implement PUT endpoint (update)
  - [x] Implement DELETE endpoint (archive)
  - [x] Return ProblemDetails on errors
- [x] Task 7: Add tests
  - [x] Integration test TrucksController endpoints (passing)

## Dev Notes

### Architecture Compliance

- **Entity in Domain layer** - No dependencies, pure POCO
- **Configuration in Infrastructure** - EF Core mapping
- **DTOs in Application layer** - Request/response models
- **Service in Application/Infrastructure** - Business logic
- **Controller in Api layer** - HTTP endpoints only

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| EF Core | Latest 10.x | SQLite provider |
| FluentValidation | Optional | For request validation |

### Critical Rules

- **No EF entities from controllers** - Only return DTOs
- **snake_case for database** - Table: `trucks`, columns: `registration_number`
- **kebab-case for API paths** - `/api/trucks`
- **camelCase for JSON** - `registrationNumber`
- **ProblemDetails for errors** - Consistent error contract

### Truck Entity

```csharp
public class Truck
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active, Maintenance, Retired
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### API Endpoints

| Method | Path | Description | Response |
|--------|------|-------------|----------|
| GET | `/api/trucks` | List all active trucks | `TruckDto[]` |
| GET | `/api/trucks/{id}` | Get single truck | `TruckDto` |
| POST | `/api/trucks` | Create truck | `TruckDto` (201) |
| PUT | `/api/trucks/{id}` | Update truck | `TruckDto` |
| DELETE | `/api/trucks/{id}` | Archive truck | 204 No Content |

### File Structure

```
apps/backend/
├── src/
│   ├── Api/
│   │   └── Controllers/
│   │       └── TrucksController.cs
│   ├── Application/
│   │   └── Trucks/
│   │       ├── ITruckService.cs
│   │       ├── TruckDto.cs
│   │       ├── CreateTruckRequest.cs
│   │       └── UpdateTruckRequest.cs
│   ├── Domain/
│   │   └── Entities/
│   │       └── Truck.cs
│   └── Infrastructure/
│       ├── Data/
│       │   └── Configurations/
│       │       └── TruckConfiguration.cs
│       └── Services/
│           └── TruckService.cs
└── tests/
    ├── Unit/
    │   └── TruckServiceTests.cs
    └── Integration/
        └── TrucksControllerTests.cs
```

### Previous Story Dependencies

- **Story 1.1** provides: Backend scaffold with EF Core configured
- **Story 1.2** provides: Authentication for [Authorize] attribute

### Testing Requirements

- Unit test: TruckService CRUD operations
- Integration test: All API endpoints with auth
- Test soft-delete behavior
- Test ProblemDetails error responses

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#API & Communication Patterns]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.1]
- [Source: docs/project_context.md#Code Quality & Style Rules]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Implementation Plan

- Add the Truck domain entity with defaults aligned to the story spec.
- Add a unit test to lock in Truck default values.

### Debug Log References

- `dotnet test apps/backend/Kcow.Backend.sln` (passed after stopping `Kcow.Api`).

### Completion Notes List

- Implemented Truck entity defaults and fields per story spec
- Added unit coverage for Truck defaults and configuration
- Added EF Core mapping for trucks with snake_case columns and indexes
- Created EF Core migration (20260103215433_AddTrucks) and applied to database
- Implemented complete CRUD API endpoints with authentication
- Added TruckService unit coverage for CRUD and soft-delete behavior
- Ensured auth failures return ProblemDetails responses
- Prevented exception detail leakage in 500 responses
- Added comprehensive integration tests (all passing)
- All endpoints return ProblemDetails on error and require authentication
- Soft-delete implemented using IsActive flag
- Tests: `dotnet test apps/backend/Kcow.Backend.sln` - 27 total tests (7 unit + 20 integration passing)

### File List

**Domain Layer:**
- apps/backend/src/Domain/Entities/Truck.cs

**Application Layer:**
- apps/backend/src/Application/Trucks/ITruckService.cs
- apps/backend/src/Application/Trucks/TruckDto.cs
- apps/backend/src/Application/Trucks/CreateTruckRequest.cs
- apps/backend/src/Application/Trucks/UpdateTruckRequest.cs

**Infrastructure Layer:**
- apps/backend/src/Infrastructure/Data/AppDbContext.cs (added Trucks DbSet)
- apps/backend/src/Infrastructure/Data/Configurations/TruckConfiguration.cs
- apps/backend/src/Infrastructure/Trucks/TruckService.cs
- apps/backend/src/Infrastructure/DependencyInjection.cs (registered ITruckService)
- apps/backend/src/Infrastructure/Migrations/20260103215433_AddTrucks.cs
- apps/backend/src/Infrastructure/Migrations/20260103215433_AddTrucks.Designer.cs
- apps/backend/src/Infrastructure/Migrations/AppDbContextModelSnapshot.cs

**API Layer:**
- apps/backend/src/Api/Program.cs
- apps/backend/src/Api/Controllers/TrucksController.cs

**Tests:**
- apps/backend/tests/Unit/TruckTests.cs
- apps/backend/tests/Unit/TruckConfigurationTests.cs
- apps/backend/tests/Unit/TruckServiceTests.cs
- apps/backend/tests/Integration/Trucks/TrucksControllerTests.cs

### Change Log

- 2026-01-03: Implemented Truck entity and CRUD API endpoints with authentication and soft-delete support
- 2026-01-04: Code review fixes for auth ProblemDetails, controller error handling, and TruckService tests
