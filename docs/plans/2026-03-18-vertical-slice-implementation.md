# Vertical Slice Backend Reorganization — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Reorganize the backend Application and Infrastructure layers into per-use-case vertical slice folders, starting with Schools as the pilot, then rolling out to all other features.

**Architecture:** Keep the 4-project structure (Api/Application/Domain/Infrastructure). Split monolithic service interfaces into per-use-case handler interfaces in Application. Move handler implementations into matching per-use-case folders in Infrastructure. Controllers inject individual handlers instead of monolithic services. Repositories stay as shared data access, referenced by handlers.

**Tech Stack:** ASP.NET Core, C#, Dapper, SQLite, xUnit integration tests

---

## Phase 1: Schools (Pilot)

Schools has 5 use cases: GetAll, GetById, Create, Update, Archive. This is the simplest feature and proves the pattern.

### Task 1: Create Application/Schools use-case folder structure

**Files:**
- Create: `apps/backend/src/Application/Schools/GetSchools/GetSchoolsQuery.cs`
- Create: `apps/backend/src/Application/Schools/GetSchools/IGetSchoolsHandler.cs`
- Create: `apps/backend/src/Application/Schools/GetSchool/GetSchoolQuery.cs`
- Create: `apps/backend/src/Application/Schools/GetSchool/IGetSchoolHandler.cs`
- Create: `apps/backend/src/Application/Schools/CreateSchool/CreateSchoolCommand.cs`
- Create: `apps/backend/src/Application/Schools/CreateSchool/ICreateSchoolHandler.cs`
- Create: `apps/backend/src/Application/Schools/UpdateSchool/UpdateSchoolCommand.cs`
- Create: `apps/backend/src/Application/Schools/UpdateSchool/IUpdateSchoolHandler.cs`
- Create: `apps/backend/src/Application/Schools/ArchiveSchool/ArchiveSchoolCommand.cs`
- Create: `apps/backend/src/Application/Schools/ArchiveSchool/IArchiveSchoolHandler.cs`
- Move: `apps/backend/src/Application/Schools/SchoolDto.cs` → `apps/backend/src/Application/Schools/Shared/SchoolDto.cs`

**Step 1: Create GetSchools use case**

```csharp
// Application/Schools/GetSchools/GetSchoolsQuery.cs
namespace Kcow.Application.Schools.GetSchools;

/// <summary>
/// Query to retrieve all active schools.
/// </summary>
public class GetSchoolsQuery
{
    // No parameters needed — retrieves all active schools
}
```

```csharp
// Application/Schools/GetSchools/IGetSchoolsHandler.cs
using Kcow.Application.Schools.Shared;

namespace Kcow.Application.Schools.GetSchools;

public interface IGetSchoolsHandler
{
    Task<List<SchoolDto>> HandleAsync(CancellationToken cancellationToken = default);
}
```

**Step 2: Create GetSchool use case**

```csharp
// Application/Schools/GetSchool/GetSchoolQuery.cs
namespace Kcow.Application.Schools.GetSchool;

/// <summary>
/// Query to retrieve a single school by ID.
/// </summary>
public class GetSchoolQuery
{
    public int Id { get; init; }
}
```

```csharp
// Application/Schools/GetSchool/IGetSchoolHandler.cs
using Kcow.Application.Schools.Shared;

namespace Kcow.Application.Schools.GetSchool;

public interface IGetSchoolHandler
{
    Task<SchoolDto?> HandleAsync(int id, CancellationToken cancellationToken = default);
}
```

**Step 3: Create CreateSchool use case**

The existing `CreateSchoolRequest.cs` becomes the command. Move it into the `CreateSchool/` folder and rename the class to `CreateSchoolCommand`.

```csharp
// Application/Schools/CreateSchool/CreateSchoolCommand.cs
// Copy the EXACT content of the current CreateSchoolRequest.cs but:
// 1. Change namespace to Kcow.Application.Schools.CreateSchool
// 2. Rename class from CreateSchoolRequest to CreateSchoolCommand
// 3. Keep ALL validation attributes and properties identical
```

```csharp
// Application/Schools/CreateSchool/ICreateSchoolHandler.cs
using Kcow.Application.Schools.Shared;

namespace Kcow.Application.Schools.CreateSchool;

public interface ICreateSchoolHandler
{
    Task<SchoolDto> HandleAsync(CreateSchoolCommand command, CancellationToken cancellationToken = default);
}
```

**Step 4: Create UpdateSchool use case**

Same approach — move `UpdateSchoolRequest.cs` into `UpdateSchool/` folder and rename to `UpdateSchoolCommand`.

```csharp
// Application/Schools/UpdateSchool/UpdateSchoolCommand.cs
// Copy the EXACT content of the current UpdateSchoolRequest.cs but:
// 1. Change namespace to Kcow.Application.Schools.UpdateSchool
// 2. Rename class from UpdateSchoolRequest to UpdateSchoolCommand
// 3. Keep ALL validation attributes and properties identical
```

```csharp
// Application/Schools/UpdateSchool/IUpdateSchoolHandler.cs
using Kcow.Application.Schools.Shared;

namespace Kcow.Application.Schools.UpdateSchool;

public interface IUpdateSchoolHandler
{
    Task<SchoolDto?> HandleAsync(int id, UpdateSchoolCommand command, CancellationToken cancellationToken = default);
}
```

**Step 5: Create ArchiveSchool use case**

```csharp
// Application/Schools/ArchiveSchool/ArchiveSchoolCommand.cs
namespace Kcow.Application.Schools.ArchiveSchool;

/// <summary>
/// Command to archive (soft-delete) a school.
/// </summary>
public class ArchiveSchoolCommand
{
    public int Id { get; init; }
}
```

```csharp
// Application/Schools/ArchiveSchool/IArchiveSchoolHandler.cs
namespace Kcow.Application.Schools.ArchiveSchool;

public interface IArchiveSchoolHandler
{
    Task<bool> HandleAsync(int id, CancellationToken cancellationToken = default);
}
```

**Step 6: Move SchoolDto to Shared folder**

Move `Application/Schools/SchoolDto.cs` to `Application/Schools/Shared/SchoolDto.cs`. Update the namespace to `Kcow.Application.Schools.Shared`.

**Step 7: Delete old files**

Delete the now-replaced files from `Application/Schools/`:
- `ISchoolService.cs`
- `CreateSchoolRequest.cs`
- `UpdateSchoolRequest.cs`
- `SchoolDto.cs` (moved to Shared/)

**Step 8: Build to verify Application compiles (expect failures in Infrastructure/Api — that's fine)**

Run: `cd apps/backend/src/Application && dotnet build`
Expected: SUCCESS (Application has no dependency on Infrastructure)

**Step 9: Commit**

```bash
git add apps/backend/src/Application/Schools/
git commit -m "refactor(schools): split Application layer into vertical slice use-case folders"
```

---

### Task 2: Create Infrastructure/Schools handler implementations

**Files:**
- Create: `apps/backend/src/Infrastructure/Schools/GetSchools/GetSchoolsHandler.cs`
- Create: `apps/backend/src/Infrastructure/Schools/GetSchool/GetSchoolHandler.cs`
- Create: `apps/backend/src/Infrastructure/Schools/CreateSchool/CreateSchoolHandler.cs`
- Create: `apps/backend/src/Infrastructure/Schools/UpdateSchool/UpdateSchoolHandler.cs`
- Create: `apps/backend/src/Infrastructure/Schools/ArchiveSchool/ArchiveSchoolHandler.cs`
- Create: `apps/backend/src/Infrastructure/Schools/Shared/SchoolMapper.cs`
- Delete: `apps/backend/src/Infrastructure/Schools/SchoolService.cs`
- Modify: `apps/backend/src/Infrastructure/DependencyInjection.cs`

**Step 1: Create shared SchoolMapper**

Extract the `MapToDto` method from the old `SchoolService` into a shared static mapper class used by all handlers.

```csharp
// Infrastructure/Schools/Shared/SchoolMapper.cs
using Kcow.Application.Schools.Shared;
using Kcow.Domain.Entities;

namespace Kcow.Infrastructure.Schools.Shared;

/// <summary>
/// Maps School entities to DTOs. Shared across all school handlers.
/// </summary>
public static class SchoolMapper
{
    public static SchoolDto ToDto(School school)
    {
        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            ShortName = school.ShortName,
            SchoolDescription = school.SchoolDescription,
            TruckId = school.TruckId,
            Price = school.Price,
            FeeDescription = school.FeeDescription,
            Formula = school.Formula,
            VisitDay = school.VisitDay,
            VisitSequence = school.VisitSequence,
            ContactPerson = school.ContactPerson,
            ContactCell = school.ContactCell,
            Phone = school.Phone,
            Telephone = school.Telephone,
            Fax = school.Fax,
            Email = school.Email,
            CircularsEmail = school.CircularsEmail,
            Address = school.Address,
            Address2 = school.Address2,
            Headmaster = school.Headmaster,
            HeadmasterCell = school.HeadmasterCell,
            IsActive = school.IsActive,
            Language = school.Language,
            PrintInvoice = school.PrintInvoice,
            ImportFlag = school.ImportFlag,
            Afterschool1Name = school.Afterschool1Name,
            Afterschool1Contact = school.Afterschool1Contact,
            Afterschool2Name = school.Afterschool2Name,
            Afterschool2Contact = school.Afterschool2Contact,
            SchedulingNotes = school.SchedulingNotes,
            MoneyMessage = school.MoneyMessage,
            SafeNotes = school.SafeNotes,
            WebPage = school.WebPage,
            Omsendbriewe = school.Omsendbriewe,
            KcowWebPageLink = school.KcowWebPageLink,
            LegacyId = school.LegacyId,
            CreatedAt = school.CreatedAt,
            UpdatedAt = school.UpdatedAt
        };
    }
}
```

**Step 2: Create GetSchoolsHandler**

```csharp
// Infrastructure/Schools/GetSchools/GetSchoolsHandler.cs
using Kcow.Application.Interfaces;
using Kcow.Application.Schools.GetSchools;
using Kcow.Application.Schools.Shared;
using Kcow.Infrastructure.Schools.Shared;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools.GetSchools;

public class GetSchoolsHandler : IGetSchoolsHandler
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<GetSchoolsHandler> _logger;

    public GetSchoolsHandler(ISchoolRepository schoolRepository, ILogger<GetSchoolsHandler> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    public async Task<List<SchoolDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schools = (await _schoolRepository.GetActiveAsync(cancellationToken))
                .OrderBy(s => s.Name)
                .ToList();

            _logger.LogInformation("Retrieved {Count} active schools", schools.Count);

            return schools.Select(SchoolMapper.ToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all schools");
            throw;
        }
    }
}
```

**Step 3: Create GetSchoolHandler**

```csharp
// Infrastructure/Schools/GetSchool/GetSchoolHandler.cs
using Kcow.Application.Interfaces;
using Kcow.Application.Schools.GetSchool;
using Kcow.Application.Schools.Shared;
using Kcow.Infrastructure.Schools.Shared;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools.GetSchool;

public class GetSchoolHandler : IGetSchoolHandler
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<GetSchoolHandler> _logger;

    public GetSchoolHandler(ISchoolRepository schoolRepository, ILogger<GetSchoolHandler> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    public async Task<SchoolDto?> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null || !school.IsActive)
        {
            _logger.LogWarning("School with ID {SchoolId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved school with ID {SchoolId}", id);
        return SchoolMapper.ToDto(school);
    }
}
```

**Step 4: Create CreateSchoolHandler**

```csharp
// Infrastructure/Schools/CreateSchool/CreateSchoolHandler.cs
using Kcow.Application.Interfaces;
using Kcow.Application.Schools.CreateSchool;
using Kcow.Application.Schools.Shared;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Schools.Shared;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools.CreateSchool;

public class CreateSchoolHandler : ICreateSchoolHandler
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<CreateSchoolHandler> _logger;

    public CreateSchoolHandler(ISchoolRepository schoolRepository, ILogger<CreateSchoolHandler> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    public async Task<SchoolDto> HandleAsync(CreateSchoolCommand command, CancellationToken cancellationToken = default)
    {
        var school = new School
        {
            Name = command.Name,
            ShortName = command.ShortName,
            SchoolDescription = command.SchoolDescription,
            TruckId = command.TruckId,
            Price = command.Price,
            FeeDescription = command.FeeDescription,
            Formula = command.Formula,
            VisitDay = command.VisitDay,
            VisitSequence = command.VisitSequence,
            ContactPerson = command.ContactPerson,
            ContactCell = command.ContactCell,
            Phone = command.Phone,
            Telephone = command.Telephone,
            Fax = command.Fax,
            Email = command.Email,
            CircularsEmail = command.CircularsEmail,
            Address = command.Address,
            Address2 = command.Address2,
            Headmaster = command.Headmaster,
            HeadmasterCell = command.HeadmasterCell,
            IsActive = true,
            Language = command.Language,
            PrintInvoice = command.PrintInvoice,
            ImportFlag = command.ImportFlag,
            Afterschool1Name = command.Afterschool1Name,
            Afterschool1Contact = command.Afterschool1Contact,
            Afterschool2Name = command.Afterschool2Name,
            Afterschool2Contact = command.Afterschool2Contact,
            SchedulingNotes = command.SchedulingNotes,
            MoneyMessage = command.MoneyMessage,
            SafeNotes = command.SafeNotes,
            WebPage = command.WebPage,
            Omsendbriewe = command.Omsendbriewe,
            KcowWebPageLink = command.KcowWebPageLink,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _schoolRepository.CreateAsync(school, cancellationToken);
        school.Id = id;

        _logger.LogInformation("Created school with ID {SchoolId} and name '{SchoolName}'",
            school.Id, school.Name);

        return SchoolMapper.ToDto(school);
    }
}
```

**Step 5: Create UpdateSchoolHandler**

```csharp
// Infrastructure/Schools/UpdateSchool/UpdateSchoolHandler.cs
using Kcow.Application.Interfaces;
using Kcow.Application.Schools.Shared;
using Kcow.Application.Schools.UpdateSchool;
using Kcow.Infrastructure.Schools.Shared;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools.UpdateSchool;

public class UpdateSchoolHandler : IUpdateSchoolHandler
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<UpdateSchoolHandler> _logger;

    public UpdateSchoolHandler(ISchoolRepository schoolRepository, ILogger<UpdateSchoolHandler> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    public async Task<SchoolDto?> HandleAsync(int id, UpdateSchoolCommand command, CancellationToken cancellationToken = default)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null || !school.IsActive)
        {
            _logger.LogWarning("Cannot update: School with ID {SchoolId} not found", id);
            return null;
        }

        school.Name = command.Name;
        school.ShortName = command.ShortName;
        school.SchoolDescription = command.SchoolDescription;
        school.TruckId = command.TruckId;
        school.Price = command.Price;
        school.FeeDescription = command.FeeDescription;
        school.Formula = command.Formula;
        school.VisitDay = command.VisitDay;
        school.VisitSequence = command.VisitSequence;
        school.ContactPerson = command.ContactPerson;
        school.ContactCell = command.ContactCell;
        school.Phone = command.Phone;
        school.Telephone = command.Telephone;
        school.Fax = command.Fax;
        school.Email = command.Email;
        school.CircularsEmail = command.CircularsEmail;
        school.Address = command.Address;
        school.Address2 = command.Address2;
        school.Headmaster = command.Headmaster;
        school.HeadmasterCell = command.HeadmasterCell;
        school.Language = command.Language;
        school.PrintInvoice = command.PrintInvoice;
        school.ImportFlag = command.ImportFlag;
        school.Afterschool1Name = command.Afterschool1Name;
        school.Afterschool1Contact = command.Afterschool1Contact;
        school.Afterschool2Name = command.Afterschool2Name;
        school.Afterschool2Contact = command.Afterschool2Contact;
        school.SchedulingNotes = command.SchedulingNotes;
        school.MoneyMessage = command.MoneyMessage;
        school.SafeNotes = command.SafeNotes;
        school.WebPage = command.WebPage;
        school.Omsendbriewe = command.Omsendbriewe;
        school.KcowWebPageLink = command.KcowWebPageLink;
        school.UpdatedAt = DateTime.UtcNow;

        await _schoolRepository.UpdateAsync(school, cancellationToken);

        _logger.LogInformation("Updated school with ID {SchoolId}", id);

        return SchoolMapper.ToDto(school);
    }
}
```

**Step 6: Create ArchiveSchoolHandler**

```csharp
// Infrastructure/Schools/ArchiveSchool/ArchiveSchoolHandler.cs
using Kcow.Application.Interfaces;
using Kcow.Application.Schools.ArchiveSchool;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools.ArchiveSchool;

public class ArchiveSchoolHandler : IArchiveSchoolHandler
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<ArchiveSchoolHandler> _logger;

    public ArchiveSchoolHandler(ISchoolRepository schoolRepository, ILogger<ArchiveSchoolHandler> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null || !school.IsActive)
        {
            _logger.LogWarning("Cannot archive: School with ID {SchoolId} not found", id);
            return false;
        }

        school.IsActive = false;
        school.UpdatedAt = DateTime.UtcNow;

        await _schoolRepository.UpdateAsync(school, cancellationToken);

        _logger.LogInformation("Archived school with ID {SchoolId}", id);
        return true;
    }
}
```

**Step 7: Delete old SchoolService.cs**

Delete `apps/backend/src/Infrastructure/Schools/SchoolService.cs`.

**Step 8: Update DI registration**

In `apps/backend/src/Infrastructure/DependencyInjection.cs`, replace the school service registration:

```csharp
// Remove:
using Kcow.Application.Schools;
// Add:
using Kcow.Application.Schools.ArchiveSchool;
using Kcow.Application.Schools.CreateSchool;
using Kcow.Application.Schools.GetSchool;
using Kcow.Application.Schools.GetSchools;
using Kcow.Application.Schools.UpdateSchool;

// Replace this line:
services.AddScoped<ISchoolService, SchoolService>();

// With:
services.AddScoped<IGetSchoolsHandler, Kcow.Infrastructure.Schools.GetSchools.GetSchoolsHandler>();
services.AddScoped<IGetSchoolHandler, Kcow.Infrastructure.Schools.GetSchool.GetSchoolHandler>();
services.AddScoped<ICreateSchoolHandler, Kcow.Infrastructure.Schools.CreateSchool.CreateSchoolHandler>();
services.AddScoped<IUpdateSchoolHandler, Kcow.Infrastructure.Schools.UpdateSchool.UpdateSchoolHandler>();
services.AddScoped<IArchiveSchoolHandler, Kcow.Infrastructure.Schools.ArchiveSchool.ArchiveSchoolHandler>();
```

**Step 9: Build Infrastructure to verify it compiles**

Run: `cd apps/backend/src/Infrastructure && dotnet build`
Expected: SUCCESS (or failures only in Api which references old types)

**Step 10: Commit**

```bash
git add apps/backend/src/Infrastructure/Schools/ apps/backend/src/Infrastructure/DependencyInjection.cs
git commit -m "refactor(schools): split Infrastructure layer into vertical slice handlers"
```

---

### Task 3: Update SchoolsController to use individual handlers

**Files:**
- Modify: `apps/backend/src/Api/Controllers/SchoolsController.cs`

**Step 1: Update controller to inject per-use-case handlers**

Replace the controller's constructor and fields. The controller now injects 5 individual handlers plus `ISchoolRepository` (still needed for archived-school checks in GetById, Update, Archive).

```csharp
// Api/Controllers/SchoolsController.cs
using Kcow.Application.Interfaces;
using Kcow.Application.Schools.ArchiveSchool;
using Kcow.Application.Schools.CreateSchool;
using Kcow.Application.Schools.GetSchool;
using Kcow.Application.Schools.GetSchools;
using Kcow.Application.Schools.Shared;
using Kcow.Application.Schools.UpdateSchool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

[ApiController]
[Route("api/schools")]
[Authorize]
public class SchoolsController : ControllerBase
{
    private readonly IGetSchoolsHandler _getSchoolsHandler;
    private readonly IGetSchoolHandler _getSchoolHandler;
    private readonly ICreateSchoolHandler _createSchoolHandler;
    private readonly IUpdateSchoolHandler _updateSchoolHandler;
    private readonly IArchiveSchoolHandler _archiveSchoolHandler;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<SchoolsController> _logger;

    public SchoolsController(
        IGetSchoolsHandler getSchoolsHandler,
        IGetSchoolHandler getSchoolHandler,
        ICreateSchoolHandler createSchoolHandler,
        IUpdateSchoolHandler updateSchoolHandler,
        IArchiveSchoolHandler archiveSchoolHandler,
        ISchoolRepository schoolRepository,
        ILogger<SchoolsController> logger)
    {
        _getSchoolsHandler = getSchoolsHandler;
        _getSchoolHandler = getSchoolHandler;
        _createSchoolHandler = createSchoolHandler;
        _updateSchoolHandler = updateSchoolHandler;
        _archiveSchoolHandler = archiveSchoolHandler;
        _schoolRepository = schoolRepository;
        _logger = logger;
    }
```

**Step 2: Update action methods to call handlers**

Each action method changes only the service call — all error handling, response codes, and ProblemDetails logic stays identical.

- `GetAll`: `_schoolService.GetAllAsync(ct)` → `_getSchoolsHandler.HandleAsync(ct)`
- `GetById`: `_schoolService.GetByIdAsync(id, ct)` → `_getSchoolHandler.HandleAsync(id, ct)`
- `Create`: `_schoolService.CreateAsync(request, ct)` → `_createSchoolHandler.HandleAsync(command, ct)` — note the parameter type changes from `CreateSchoolRequest` to `CreateSchoolCommand`
- `Update`: `_schoolService.UpdateAsync(id, request, ct)` → `_updateSchoolHandler.HandleAsync(id, command, ct)` — parameter type changes from `UpdateSchoolRequest` to `UpdateSchoolCommand`
- `Archive`: `_schoolService.ArchiveAsync(id, ct)` → `_archiveSchoolHandler.HandleAsync(id, ct)`

For Create and Update, the `[FromBody]` parameter type changes:
- `Create([FromBody] CreateSchoolRequest request, ...)` → `Create([FromBody] CreateSchoolCommand command, ...)`
- `Update(int id, [FromBody] UpdateSchoolRequest request, ...)` → `Update(int id, [FromBody] UpdateSchoolCommand command, ...)`

Keep ALL other controller code identical (ProblemDetails, try/catch, archived-school checks, status codes).

**Step 3: Build entire solution**

Run: `cd apps/backend && dotnet build Kcow.Backend.sln`
Expected: SUCCESS

**Step 4: Commit**

```bash
git add apps/backend/src/Api/Controllers/SchoolsController.cs
git commit -m "refactor(schools): update controller to use vertical slice handlers"
```

---

### Task 4: Update integration tests and verify

**Files:**
- Modify: `apps/backend/tests/Integration/Schools/SchoolsControllerTests.cs`

**Step 1: Update test imports if needed**

The tests use `CreateSchoolRequest` and `UpdateSchoolRequest` for HTTP request bodies. Since we renamed these to `CreateSchoolCommand` and `UpdateSchoolCommand`, update the test file:

- Replace `using Kcow.Application.Schools;` with:
  ```csharp
  using Kcow.Application.Schools.CreateSchool;
  using Kcow.Application.Schools.UpdateSchool;
  using Kcow.Application.Schools.Shared;
  ```
- Replace all occurrences of `CreateSchoolRequest` with `CreateSchoolCommand`
- Replace all occurrences of `UpdateSchoolRequest` with `UpdateSchoolCommand`

**Step 2: Run integration tests**

Run: `cd apps/backend && dotnet test tests/Integration --filter "FullyQualifiedName~Schools" -v normal`
Expected: All tests PASS

**Step 3: Run full backend test suite**

Run: `cd apps/backend && dotnet test -v normal`
Expected: All tests PASS

**Step 4: Commit**

```bash
git add apps/backend/tests/
git commit -m "refactor(schools): update integration tests for vertical slice types"
```

---

### Task 5: Check for remaining references to old types and clean up

**Step 1: Search for any remaining references to old types**

Run: `grep -r "ISchoolService" apps/backend/src/ --include="*.cs"`
Run: `grep -r "CreateSchoolRequest" apps/backend/ --include="*.cs"`
Run: `grep -r "UpdateSchoolRequest" apps/backend/ --include="*.cs"`
Run: `grep -r "Kcow.Application.Schools;" apps/backend/ --include="*.cs"` (old flat namespace)

Expected: No matches (or only in Import/ code that references school types — update those too).

**Step 2: Fix any remaining references found in Step 1**

Update using statements and type references in any files that still reference the old types.

**Step 3: Final full build and test**

Run: `cd apps/backend && dotnet build Kcow.Backend.sln && dotnet test -v normal`
Expected: Build SUCCESS, all tests PASS

**Step 4: Commit**

```bash
git add -A
git commit -m "refactor(schools): clean up remaining references to old school types"
```

---

## Phase 2: Remaining Features

Apply the exact same pattern from Phase 1 to each feature. Process one feature at a time, in this order:

### Feature Order

1. **Activities** — Simple CRUD like Schools (4 files in Application)
2. **Trucks** — Simple CRUD (4 files in Application)
3. **ClassGroups** — Has extra conflict-check use case (6 files in Application)
4. **Families** — Has link/unlink use cases (5 files in Application)
5. **Students** — Larger, has search + list DTOs (6 files in Application)
6. **Attendance** — Has batch operations (5 files in Application)
7. **Evaluations** — Has specialized DTOs (4 files in Application)
8. **Billing** — Multiple sub-entities: invoices, payments, summary (5 files in Application)
9. **Audit** — Read-only, simpler (2 files in Application)
10. **Auth** — Special case, may want to keep as-is (authentication is cross-cutting)
11. **Import** — Special case, CLI commands + complex orchestration, may want to keep as-is

### Per-Feature Checklist (repeat for each)

For each feature, create a new task following this checklist:

- [ ] Create use-case subfolders in `Application/<Feature>/`
- [ ] Create command/query classes in each subfolder
- [ ] Create handler interfaces in each subfolder
- [ ] Move DTOs to `Application/<Feature>/Shared/`
- [ ] Delete old service interface and request files
- [ ] Build Application project
- [ ] Create handler implementations in `Infrastructure/<Feature>/` subfolders
- [ ] Create shared mapper in `Infrastructure/<Feature>/Shared/` if needed
- [ ] Delete old service implementation
- [ ] Update DI registration in `Infrastructure/DependencyInjection.cs`
- [ ] Build Infrastructure project
- [ ] Update controller in `Api/Controllers/`
- [ ] Build full solution
- [ ] Update integration tests
- [ ] Run all tests
- [ ] Search for remaining old type references
- [ ] Commit

### Task 6-16: One task per remaining feature

Each task follows the identical structure as Tasks 1-5 but for the specific feature. Read the existing service interface to identify the use cases, then create the matching folder structure.

**Important notes for specific features:**

- **ClassGroups**: Has `CheckConflictsRequest` and `ScheduleConflictDto` — these become a `CheckConflicts/` use case
- **Billing**: Has sub-entities (Invoice, Payment) — organize as `Billing/CreateInvoice/`, `Billing/CreatePayment/`, `Billing/GetSummary/`, etc.
- **Students**: Has `StudentListDto` and `StudentSearchResultDto` — both go in `Students/Shared/`
- **Auth**: Consider keeping as-is since authentication is cross-cutting. If splitting, the use cases are Login, Register, RefreshToken, etc.
- **Import**: Complex orchestration with CLI commands — consider keeping as-is. The import pipeline is already fairly self-contained.

---

## Phase 3: Clean up Application/Interfaces

After all features are migrated:

### Task 17: Move repository interfaces into feature folders

Move each `IXxxRepository.cs` from `Application/Interfaces/` into the corresponding `Application/<Feature>/Shared/` folder. This completes the vertical slice by colocating the repository interface with the feature.

- `Application/Interfaces/ISchoolRepository.cs` → `Application/Schools/Shared/ISchoolRepository.cs`
- `Application/Interfaces/IStudentRepository.cs` → `Application/Students/Shared/IStudentRepository.cs`
- etc.

Keep cross-cutting interfaces (like `IDbConnectionFactory`) in `Application/Common/Interfaces/`.

Update all using statements across the solution.

Run: `cd apps/backend && dotnet build Kcow.Backend.sln && dotnet test -v normal`

Commit: `git commit -m "refactor: move repository interfaces into feature folders"`

---

## Verification Checklist

After all phases are complete:

- [ ] `dotnet build Kcow.Backend.sln` succeeds
- [ ] `dotnet test` — all tests pass
- [ ] No remaining references to old monolithic service interfaces (grep for `ISchoolService`, `ITruckService`, etc.)
- [ ] Each feature in Application has: use-case subfolders + Shared/ folder
- [ ] Each feature in Infrastructure mirrors the Application structure
- [ ] DI registration uses per-handler registrations
- [ ] Controllers inject individual handlers
- [ ] API routes and response shapes are unchanged (no breaking changes)
- [ ] Frontend still works end-to-end
