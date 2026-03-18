# Vertical Slice Reorganization — Backend

**Date:** 2026-03-18
**Scope:** Backend only (apps/backend)
**Goal:** Reduce cross-project jumping by reorganizing Application and Infrastructure into per-use-case feature folders.

---

## Decision

Reorganize Application and Infrastructure layers into vertical slice feature folders. Keep the existing 4-project structure (Api/Application/Domain/Infrastructure). No MediatR — keep service interfaces split into per-use-case handler interfaces. One folder per use case.

## What Changes

### Application Layer

Each feature gets a folder. Each use case gets a subfolder with a command/query and handler interface. Shared DTOs stay at the feature level.

```
Application/
  Schools/
    CreateSchool/
      CreateSchoolCommand.cs
      ICreateSchoolHandler.cs
    GetSchool/
      GetSchoolQuery.cs
      IGetSchoolHandler.cs
    GetSchools/
      GetSchoolsQuery.cs
      IGetSchoolsHandler.cs
    UpdateSchool/
      UpdateSchoolCommand.cs
      IUpdateSchoolHandler.cs
    DeleteSchool/
      DeleteSchoolCommand.cs
      IDeleteSchoolHandler.cs
    Shared/
      SchoolDto.cs
  Students/
    CreateStudent/
      CreateStudentCommand.cs
      ICreateStudentHandler.cs
    GetStudent/
      GetStudentQuery.cs
      IGetStudentHandler.cs
    UpdateStudent/
      UpdateStudentCommand.cs
      IUpdateStudentHandler.cs
    Shared/
      StudentDto.cs
      StudentListDto.cs
      StudentSearchResultDto.cs
  Common/
    Interfaces/
      ...existing shared interfaces...
```

The monolithic service interfaces (e.g., `ISchoolService`) are split into one interface per use case (`ICreateSchoolHandler`, `IGetSchoolHandler`, etc.).

### Infrastructure Layer

Mirrors Application feature folders. Each handler implements its interface and injects DbContext directly. Shared repositories are optional, only for genuinely reused query logic.

```
Infrastructure/
  Schools/
    CreateSchool/
      CreateSchoolHandler.cs
    GetSchool/
      GetSchoolHandler.cs
    GetSchools/
      GetSchoolsHandler.cs
    UpdateSchool/
      UpdateSchoolHandler.cs
    DeleteSchool/
      DeleteSchoolHandler.cs
    SchoolRepository.cs          (optional, shared queries only)
  Students/
    CreateStudent/
      CreateStudentHandler.cs
    GetStudent/
      GetStudentHandler.cs
    UpdateStudent/
      UpdateStudentHandler.cs
    StudentRepository.cs         (optional)
  Database/
    ...unchanged (DbContext, configs, migrations)...
  DependencyInjection.cs
```

### Api Layer (Minimal Change)

Controllers stay as-is. Only change: inject per-use-case handlers instead of monolithic service interfaces.

```csharp
// Before
public SchoolsController(ISchoolService schoolService)

// After
public SchoolsController(
    ICreateSchoolHandler createHandler,
    IGetSchoolHandler getHandler,
    IGetSchoolsHandler getSchoolsHandler,
    IUpdateSchoolHandler updateHandler,
    IDeleteSchoolHandler deleteHandler)
```

### Domain Layer (No Change)

Entities stay flat in `Domain/Entities/`. They are shared across features and do not need reorganization.

## What Stays Untouched

- Domain project (entities, enums, constants)
- Database folder (DbContext, migrations, configs)
- Frontend (already feature-sliced)
- All API routes and response shapes (no breaking changes)

## Migration Strategy

1. One feature at a time
2. Start with simple maintenance entities: Schools -> Activities -> ClassGroups
3. Then complex features: Students, Attendance, Evaluations, Billing, Families, Trucks
4. For each feature:
   - Create use-case subfolders in Application
   - Split monolithic service interface into per-use-case interfaces
   - Move DTOs into Shared/
   - Create matching subfolders in Infrastructure
   - Split service implementation into per-use-case handlers
   - Update controller constructor to inject individual handlers
   - Update DI registration
   - Run tests to confirm nothing broke
5. Risk: Low — file moves + interface splits, no logic changes

## DI Registration

`Infrastructure/DependencyInjection.cs` registers each handler:

```csharp
services.AddScoped<ICreateSchoolHandler, CreateSchoolHandler>();
services.AddScoped<IGetSchoolHandler, GetSchoolHandler>();
// etc.
```
