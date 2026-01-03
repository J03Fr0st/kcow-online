# Story 2.1: Truck Entity & API Endpoints

Status: ready-for-dev

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

- [ ] Task 1: Create Truck entity (AC: #1)
  - [ ] Create Truck.cs in Domain/Entities
  - [ ] Add properties: Id, Name, RegistrationNumber, Status, Notes, IsActive, CreatedAt, UpdatedAt
  - [ ] Use appropriate data types
- [ ] Task 2: Create EF Core configuration (AC: #2)
  - [ ] Create TruckConfiguration.cs in Infrastructure/Data/Configurations
  - [ ] Configure table name as `trucks` (snake_case)
  - [ ] Configure column mappings with snake_case
  - [ ] Add required constraints and indexes
- [ ] Task 3: Create and apply migration (AC: #3)
  - [ ] Run `dotnet ef migrations add AddTrucks`
  - [ ] Review generated migration
  - [ ] Apply migration with `dotnet ef database update`
- [ ] Task 4: Create Truck DTOs (AC: #4)
  - [ ] Create TruckDto.cs in Application/Trucks
  - [ ] Create CreateTruckRequest.cs
  - [ ] Create UpdateTruckRequest.cs
  - [ ] Add validation attributes
- [ ] Task 5: Create TruckService (AC: #4)
  - [ ] Create ITruckService interface in Application/Trucks
  - [ ] Create TruckService implementation in Infrastructure
  - [ ] Implement CRUD operations
  - [ ] Handle soft-delete (IsActive = false)
- [ ] Task 6: Create TrucksController (AC: #4, #5, #6)
  - [ ] Create TrucksController in Api/Controllers
  - [ ] Add [Authorize] attribute
  - [ ] Implement GET endpoints (list, single)
  - [ ] Implement POST endpoint (create)
  - [ ] Implement PUT endpoint (update)
  - [ ] Implement DELETE endpoint (archive)
  - [ ] Return ProblemDetails on errors
- [ ] Task 7: Add tests
  - [ ] Unit test TruckService
  - [ ] Integration test TrucksController endpoints

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

### Debug Log References

### Completion Notes List

### File List
