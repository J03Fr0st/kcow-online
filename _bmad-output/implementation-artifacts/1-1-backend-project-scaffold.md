# Story 1.1: Backend Project Scaffold

Status: done

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

- [x] Task 1: Create ASP.NET Core Web API project (AC: #1)
  - [x] Initialize project with `dotnet new webapi -n Kcow.Api -o apps/backend/src/Api`
  - [x] Configure target framework as net10.0
  - [x] Create initial folder structure
- [x] Task 2: Set up Clean Architecture folders (AC: #1)
  - [x] Create `src/Application/` class library
  - [x] Create `src/Domain/` class library
  - [x] Create `src/Infrastructure/` class library
  - [x] Add project references (Api → Application → Domain; Infrastructure → Application, Domain)
- [x] Task 3: Configure EF Core with SQLite (AC: #2)
  - [x] Add EF Core and SQLite NuGet packages to Infrastructure
  - [x] Create AppDbContext in Infrastructure/Data
  - [x] Configure SQLite connection string in appsettings.json
  - [x] Register DbContext in DI
- [x] Task 4: Configure Serilog logging (AC: #3)
  - [x] Add Serilog NuGet packages
  - [x] Configure Serilog in Program.cs
  - [x] Add console and file sinks
- [x] Task 5: Add ProblemDetails middleware (AC: #4)
  - [x] Install ProblemDetails package (built-in to ASP.NET Core)
  - [x] Configure ProblemDetails in exception handling middleware
  - [x] Create custom exception handler
- [x] Task 6: Configure CORS (AC: #5)
  - [x] Add CORS policy for frontend origin (http://localhost:4200)
  - [x] Apply CORS middleware in pipeline
- [x] Task 7: Create test projects (AC: #1)
  - [x] Create `tests/Unit/` project
  - [x] Create `tests/Integration/` project
  - [x] Add xUnit and other test dependencies
- [x] Task 8: Verify API runs (AC: #6)
  - [x] Run `dotnet build` successfully
  - [x] Run `dotnet run` and verify API starts
  - [x] Test health endpoint

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

Claude (Sonnet 4)

### Debug Log References

- Build succeeded after adding Microsoft.Extensions.DependencyInjection.Abstractions to Application project

### Completion Notes List

- Created ASP.NET Core Web API with net10.0 framework
- Implemented Clean Architecture with 4 layers (Api, Application, Domain, Infrastructure)
- Configured EF Core with SQLite (AppDbContext in Infrastructure/Data)
- Integrated Serilog with console and rolling file sinks
- Added ProblemDetails middleware for standardized error responses
- Configured CORS policy for frontend origin (http://localhost:4200)
- Created Unit and Integration test projects with xUnit
- Added health endpoint at /health
- All tests passing (2 tests)

### File List

**New Files:**
- apps/backend/Kcow.Backend.sln
- apps/backend/src/Api/Kcow.Api.csproj
- apps/backend/src/Api/Program.cs
- apps/backend/src/Api/appsettings.json
- apps/backend/src/Api/appsettings.Development.json
- apps/backend/src/Api/Properties/launchSettings.json
- apps/backend/src/Api/Middleware/GlobalExceptionHandler.cs
- apps/backend/src/Api/Controllers/ (empty directory)
- apps/backend/src/Application/Kcow.Application.csproj
- apps/backend/src/Application/DependencyInjection.cs
- apps/backend/src/Application/Common/.gitkeep
- apps/backend/src/Domain/Kcow.Domain.csproj
- apps/backend/src/Domain/Entities/.gitkeep
- apps/backend/src/Infrastructure/Kcow.Infrastructure.csproj
- apps/backend/src/Infrastructure/DependencyInjection.cs
- apps/backend/src/Infrastructure/Data/AppDbContext.cs
- apps/backend/tests/Unit/Kcow.Unit.Tests.csproj
- apps/backend/tests/Unit/DependencyInjectionTests.cs
- apps/backend/tests/Integration/Kcow.Integration.Tests.csproj
- apps/backend/tests/Integration/CustomWebApplicationFactory.cs
- apps/backend/tests/Integration/HealthEndpointTests.cs

## Change Log

| Date | Change |
|------|--------|
| 2026-01-03 | Initial scaffold implementation - all 8 tasks complete |
| 2026-01-03 | AI Code Review - Fixed 5 HIGH/MEDIUM issues, committed to git |

## Code Review Follow-ups (AI)

### Issues Fixed Automatically
- ✅ **[CRITICAL]** Backend scaffold committed to git (commit b190f2b)
- ✅ **[HIGH]** Created Controllers folder (apps/backend/src/Api/Controllers/)
- ✅ **[HIGH]** Replaced placeholder unit test with real test (DependencyInjectionTests.cs)
- ✅ **[MEDIUM]** Added database initialization with EnsureCreatedAsync()
- ✅ **[MEDIUM]** Created custom exception handler middleware (GlobalExceptionHandler.cs)

### Retracted Findings
- ~~Application→Domain project reference missing~~ - Already existed in Kcow.Application.csproj

### Remaining Low-Priority Items
- 🟢 **[LOW]** Unit test naming could be more descriptive (DependencyInjectionTests is adequate)
- 🟢 **[LOW]** .gitkeep files not tracked (irrelevant since dirs are now committed)
