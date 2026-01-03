# Story 1.1: Backend Project Scaffold

Status: ready-for-dev

## Story

As a **developer**,
I want **an ASP.NET Core Web API project with Clean Architecture structure**,
so that **I have a properly organized codebase to build features upon**.

## Acceptance Criteria

1. **Given** the project root at `apps/backend/`
   **When** the scaffold is created
   **Then** the following structure exists:
   - `src/Api/` with Program.cs and Controllers folder
   - `src/Application/` for business logic
   - `src/Domain/` for entities
   - `src/Infrastructure/` for data access and external services
   - `tests/Unit/` and `tests/Integration/` for testing

2. **And** EF Core with SQLite is configured with a DbContext

3. **And** Serilog logging is configured

4. **And** ProblemDetails error handling middleware is added

5. **And** CORS is configured to allow the frontend origin

6. **And** the API runs successfully with `dotnet run`

## Tasks / Subtasks

- [ ] Task 1: Create ASP.NET Core Web API project (AC: #1)
  - [ ] Initialize project with `dotnet new webapi -n Kcow.Api -o apps/backend/src/Api`
  - [ ] Configure target framework as net10.0
  - [ ] Create initial folder structure
- [ ] Task 2: Set up Clean Architecture folders (AC: #1)
  - [ ] Create `src/Application/` class library
  - [ ] Create `src/Domain/` class library
  - [ ] Create `src/Infrastructure/` class library
  - [ ] Add project references (Api → Application → Domain; Infrastructure → Application, Domain)
- [ ] Task 3: Configure EF Core with SQLite (AC: #2)
  - [ ] Add EF Core and SQLite NuGet packages to Infrastructure
  - [ ] Create AppDbContext in Infrastructure/Data
  - [ ] Configure SQLite connection string in appsettings.json
  - [ ] Register DbContext in DI
- [ ] Task 4: Configure Serilog logging (AC: #3)
  - [ ] Add Serilog NuGet packages
  - [ ] Configure Serilog in Program.cs
  - [ ] Add console and file sinks
- [ ] Task 5: Add ProblemDetails middleware (AC: #4)
  - [ ] Install ProblemDetails package
  - [ ] Configure ProblemDetails in exception handling middleware
  - [ ] Create custom exception handler
- [ ] Task 6: Configure CORS (AC: #5)
  - [ ] Add CORS policy for frontend origin (http://localhost:4200)
  - [ ] Apply CORS middleware in pipeline
- [ ] Task 7: Create test projects (AC: #1)
  - [ ] Create `tests/Unit/` project
  - [ ] Create `tests/Integration/` project
  - [ ] Add xUnit and other test dependencies
- [ ] Task 8: Verify API runs (AC: #6)
  - [ ] Run `dotnet build` successfully
  - [ ] Run `dotnet run` and verify API starts
  - [ ] Test health endpoint

## Dev Notes

### Architecture Compliance

- **CRITICAL**: Follow Clean Architecture strictly:
  - `Api` layer: Controllers, middleware, Program.cs - depends on Application
  - `Application` layer: DTOs, services, validators - depends on Domain
  - `Domain` layer: Entities, value objects - no dependencies
  - `Infrastructure` layer: EF Core, external services - depends on Application & Domain
- **DO NOT** put EF Core in Api or Application layers
- **DO NOT** return EF entities from controllers - use DTOs only

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| .NET | 10.0 | Use `net10.0` target framework |
| EF Core | Latest 10.x | SQLite provider |
| Serilog | Latest | Console + file sinks |

### File Structure

```
apps/backend/
├── src/
│   ├── Api/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   ├── Kcow.Api.csproj
│   │   └── appsettings.json
│   ├── Application/
│   │   ├── Common/
│   │   └── Kcow.Application.csproj
│   ├── Domain/
│   │   ├── Entities/
│   │   └── Kcow.Domain.csproj
│   └── Infrastructure/
│       ├── Data/
│       │   └── AppDbContext.cs
│       ├── Migrations/
│       └── Kcow.Infrastructure.csproj
└── tests/
    ├── Unit/
    └── Integration/
```

### Project References

```text
Api → Application
Application → Domain
Infrastructure → Application, Domain
```

### Key Configuration Files

**appsettings.json** - Must include:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=kcow.db"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

### Testing Requirements

- Unit tests: Application services, validators
- Integration tests: Controllers with in-memory SQLite
- Use xUnit as test framework

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#Core Architectural Decisions]
- [Source: docs/project_context.md#Technology Stack]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 1.1]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
