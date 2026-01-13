# Story 0.1: Migrate EF Core to Dapper + DbUp

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a **developer**,
I want **to migrate the data access layer from Entity Framework Core to Dapper with DbUp migrations**,
so that **I have explicit SQL control, simpler debugging, and reduced ORM complexity**.

**Priority:** High (Blocking - should be completed before new backend stories)

## Acceptance Criteria

1. [AC: 1] **Given** the existing EF Core implementation, **When** the migration is complete, **Then** all existing functionality continues to work with the new data access layer

2. [AC: 2] **Given** the migration is complete, **When** the application starts, **Then** `Infrastructure/Data/` is replaced with:
   - `Infrastructure/Repositories/` - Dapper repository implementations
   - `Infrastructure/Database/` - IDbConnectionFactory, DbUp bootstrapper
   - `Infrastructure/Sql/` - Complex SQL queries as constants
   - `Infrastructure/Migrations/Scripts/` - Versioned SQL scripts

3. [AC: 3] **Given** the migration is complete, **When** code executes, **Then** `AppDbContext` is removed and replaced with `IDbConnectionFactory`

4. [AC: 4] **Given** the migration is complete, **When** the database schema is examined, **Then** all entity configurations are converted to DbUp SQL migration scripts

5. [AC: 5] **Given** the migration is complete, **When** services access data, **Then** repository pattern is implemented with interfaces in `Application/` and implementations in `Infrastructure/Repositories/`

6. [AC: 6] **Given** the migration is complete, **When** migrations are listed, **Then** existing EF Core migrations are converted to DbUp scripts with naming: `YYYYMMDD_NN_Description.sql`

7. [AC: 7] **Given** the migration is complete, **When** data is accessed, **Then** each entity has a corresponding repository (ITruckRepository/TruckRepository, ISchoolRepository/SchoolRepository, etc.)

8. [AC: 8] **Given** the migration is complete, **When** queries execute, **Then** repositories use Dapper's `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`

9. [AC: 9] **Given** the migration is complete, **When** queries are executed, **Then** all queries use parameterized SQL (no string concatenation)

10. [AC: 10] **Given** the migration is complete, **When** the application starts in Development, **Then** DbUp is configured to run migrations on startup

11. [AC: 11] **Given** the migration is complete, **When** the database is examined, **Then** existing database schema is preserved (data migration not required)

12. [AC: 12] **Given** the migration is complete, **When** all existing API endpoints are called, **Then** all endpoints continue to work correctly

13. [AC: 13] **Given** the migration is complete, **When** all existing integration tests run, **Then** all tests pass

14. [AC: 14] **Given** the migration is complete, **When** all existing E2E tests run, **Then** all tests pass

## Tasks / Subtasks

- [x] 1. Set up infrastructure foundation (AC: 2, 3)
  - [x] 1.1 Install required NuGet packages: `Dapper`, `DbUp`, `DbUp.SQLite`
  - [x] 1.2 Create `Infrastructure/Database/IDbConnectionFactory` interface
  - [x] 1.3 Create `Infrastructure/Database/SqliteConnectionFactory` implementation
  - [x] 1.4 Create `Infrastructure/Database/DbUpBootstrapper` for migration execution
  - [x] 1.5 Register IDbConnectionFactory and DbUp in DI container
  - [x] 1.6 Remove EF Core `AddDbContext` registration from DI

- [x] 2. Convert existing EF Core migrations to DbUp scripts (AC: 4, 6, 11)
  - [x] 2.1 Create `Infrastructure/Migrations/Scripts/` directory
  - [x] 2.2 Analyze existing EF Core migrations (5 migrations found)
  - [x] 2.3 Create `20260107_0001_InitialCreate.sql` from InitialCreate migration
  - [x] 2.4 Create `20260107_0002_AddFamilies.sql` from AddFamilies migration
  - [x] 2.5 Create `20260107_0003_CodeReviewFixes.sql` from CodeReviewFixes migration
  - [x] 2.6 Create `20260109_0004_UpdateActivityEntity.sql` from UpdateActivityEntity migration
  - [x] 2.7 Create `20260112_0005_FixPendingModelChanges.sql` from FixPendingModelChanges migration
  - [x] 2.8 Verify all scripts preserve existing schema exactly

- [x] 3. Create repository interfaces for all entities (AC: 5, 7)
  - [x] 3.1 Create `Application/Interfaces/IUserRepository.cs`
  - [x] 3.2 Create `Application/Interfaces/IRoleRepository.cs`
  - [x] 3.3 Create `Application/Interfaces/ITruckRepository.cs`
  - [x] 3.4 Create `Application/Interfaces/ISchoolRepository.cs`
  - [x] 3.5 Create `Application/Interfaces/IClassGroupRepository.cs`
  - [x] 3.6 Create `Application/Interfaces/IStudentRepository.cs`
  - [x] 3.7 Create `Application/Interfaces/IFamilyRepository.cs`
  - [x] 3.8 Create `Application/Interfaces/IActivityRepository.cs`

- [x] 4. Implement Dapper repositories (AC: 5, 7, 8, 9)
  - [x] 4.1 Create `Infrastructure/Repositories/UserRepository.cs` with CRUD methods
  - [x] 4.2 Create `Infrastructure/Repositories/RoleRepository.cs` with CRUD methods
  - [x] 4.3 Create `Infrastructure/Repositories/TruckRepository.cs` with CRUD methods
  - [x] 4.4 Create `Infrastructure/Repositories/SchoolRepository.cs` with CRUD methods
  - [x] 4.5 Create `Infrastructure/Repositories/ClassGroupRepository.cs` with CRUD methods
  - [x] 4.6 Create `Infrastructure/Repositories/StudentRepository.cs` with CRUD methods (92 fields)
  - [x] 4.7 Create `Infrastructure/Repositories/FamilyRepository.cs` with CRUD methods
  - [x] 4.8 Create `Infrastructure/Repositories/ActivityRepository.cs` with CRUD methods
  - [x] 4.9 Use `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync` throughout
  - [x] 4.10 Ensure all SQL is parameterized (no string concatenation)

- [x] 5. Refactor service layer to use repositories (AC: 1, 12)
  - [x] 5.1 Update `AuthService` to use `IUserRepository` instead of `AppDbContext`
  - [x] 5.2 Update `StudentService` to use `IStudentRepository` instead of `AppDbContext`
  - [x] 5.3 Update `SchoolService` to use `ISchoolRepository` instead of `AppDbContext`
  - [x] 5.4 Update `ClassGroupService` to use `IClassGroupRepository` instead of `AppDbContext`
  - [x] 5.5 Update `TruckService` to use `ITruckRepository` instead of `AppDbContext`
  - [x] 5.6 Update `FamilyService` to use `IFamilyRepository` instead of `AppDbContext`
  - [x] 5.7 Update `ActivityService` to use `IActivityRepository` instead of `AppDbContext`

- [x] 6. Create SQL query constants for complex queries (AC: 2)
  - [x] 6.1 Create `Infrastructure/Sql/UserQueries.cs` for User-related queries
  - [x] 6.2 Create `Infrastructure/Sql/StudentQueries.cs` for Student-related queries
  - [x] 6.3 Create `Infrastructure/Sql/SchoolQueries.cs` for School-related queries
  - [x] 6.4 Store complex JOIN queries as constants/resources

- [x] 7. Remove EF Core artifacts (AC: 3)
  - [x] 7.1 Delete `Infrastructure/Data/AppDbContext.cs`
  - [x] 7.2 Delete `Infrastructure/Data/Configurations/` directory (entity configurations)
  - [x] 7.3 Delete `Infrastructure/Migrations/` directory (EF Core migrations)
  - [x] 7.4 Remove EF Core NuGet packages (Microsoft.EntityFrameworkCore.*)

- [x] 8. Configure DbUp to run on startup (AC: 10)
  - [x] 8.1 Update `Program.cs` to call DbUp bootstrapper on startup in Development
  - [x] 8.2 Ensure migrations run before application services start
  - [x] 8.3 Add logging for migration execution

- [x] 9. Test all functionality (AC: 1, 12, 13, 14)
  - [x] 9.1 Run all existing API endpoints and verify they work
  - [x] 9.2 Run all integration tests and verify they pass
  - [x] 9.3 Run all E2E tests and verify they pass
  - [x] 9.4 Verify database schema is preserved (compare before/after)

## Dev Notes

### Migration Overview

This story migrates the data access layer from Entity Framework Core to Dapper with DbUp migrations. The primary goals are:
1. Explicit SQL control for better debugging
2. Reduced ORM complexity
3. Simplified data access layer
4. Maintained functionality (all existing features continue to work)

**Critical Constraint:** This migration must preserve the existing database schema exactly. No data migration is required - only the data access layer changes.

### Current EF Core Implementation

**Existing Setup:**
- **DbContext:** `AppDbContext` at `apps/backend/src/Infrastructure/Data/AppDbContext.cs`
- **Database:** SQLite with connection string `Data Source=kcow.db`
- **Entities:** 9 entities (User, Role, Truck, School, ClassGroup, Student, Family, StudentFamily, Activity)
- **Migrations:** 5 EF Core migrations to convert
- **Service Pattern:** Services use `AppDbContext` directly (no repository pattern)

**Entities Found:**
| Entity | Fields | Complexity |
|--------|--------|------------|
| User | Standard auth fields | Simple |
| Role | Standard RBAC | Simple |
| Truck | Fleet management | Simple |
| School | 30 fields (XSD-aligned) | Medium |
| ClassGroup | 15 fields (XSD-aligned) | Medium |
| Student | 92 fields (XSD-aligned) | **Complex** |
| Family | Standard family fields | Medium |
| StudentFamily | Junction table | Simple |
| Activity | 7 fields (XSD-aligned) | Simple |

**Existing EF Core Migrations to Convert:**
1. `20260107060026_InitialCreate` - Initial schema with Users, Roles, Trucks, Schools, ClassGroups
2. `20260107121215_AddFamilies` - Added Families and StudentFamily junction table
3. `20260107141143_CodeReviewFixes` - Various fixes and constraints
4. `20260109120000_UpdateActivityEntity` - Activity entity updates
5. `20260112053753_FixPendingModelChanges` - Index renaming, name column additions

### Project Structure Notes

**New Structure to Create:**

```
apps/backend/src/
├── Application/
│   └── Interfaces/              # NEW - Repository interfaces
│       ├── IUserRepository.cs
│       ├── IRoleRepository.cs
│       ├── ITruckRepository.cs
│       ├── ISchoolRepository.cs
│       ├── IClassGroupRepository.cs
│       ├── IStudentRepository.cs
│       ├── IFamilyRepository.cs
│       └── IActivityRepository.cs
│
├── Domain/
│   └── Entities/                # KEEP - No changes
│       ├── User.cs
│       ├── Role.cs
│       ├── Truck.cs
│       ├── School.cs
│       ├── ClassGroup.cs
│       ├── Student.cs
│       ├── Family.cs
│       ├── StudentFamily.cs
│       └── Activity.cs
│
└── Infrastructure/
    ├── Database/                # NEW - Connection factory and DbUp
    │   ├── IDbConnectionFactory.cs
    │   ├── SqliteConnectionFactory.cs
    │   └── DbUpBootstrapper.cs
    │
    ├── Repositories/             # NEW - Dapper implementations
    │   ├── UserRepository.cs
    │   ├── RoleRepository.cs
    │   ├── TruckRepository.cs
    │   ├── SchoolRepository.cs
    │   ├── ClassGroupRepository.cs
    │   ├── StudentRepository.cs
    │   ├── FamilyRepository.cs
    │   └── ActivityRepository.cs
    │
    ├── Sql/                      # NEW - Complex SQL queries
    │   ├── UserQueries.cs
    │   ├── StudentQueries.cs
    │   └── SchoolQueries.cs
    │
    └── Migrations/
        └── Scripts/              # NEW - DbUp SQL scripts
            ├── 20260107_0001_InitialCreate.sql
            ├── 20260107_0002_AddFamilies.sql
            ├── 20260107_0003_CodeReviewFixes.sql
            ├── 20260107_0004_UpdateActivityEntity.sql
            └── 20260112_0005_FixPendingModelChanges.sql
```

**Structure to DELETE:**
- `Infrastructure/Data/AppDbContext.cs`
- `Infrastructure/Data/Configurations/` (all entity configurations)
- `Infrastructure/Migrations/` (EF Core migrations)

### Architecture Compliance

**From Architecture Document [Source: _bmad-output/planning-artifacts/architecture.md]:**

**Data Access Patterns:**
- Repository pattern with interfaces in `Application/`
- Repository implementations in `Infrastructure/Repositories/`
- Repositories receive `IDbConnection` via constructor injection
- Use parameterized queries exclusively
- Dapper's `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`

**Connection Management:**
- `IDbConnectionFactory` creates connections per request
- Connections disposed after each operation (no long-lived connections)
- Transaction support via `IDbTransaction` passed to repository methods

**Migration Patterns:**
- Scripts in `Migrations/Scripts/` with naming: `YYYYMMDD_NN_Description.sql`
- Scripts idempotent where possible
- DbUp runs automatically on application startup in Development
- Production migrations run via CLI command

**Example Repository Pattern [Source: architecture.md#Data Access Patterns]:**
```csharp
public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StudentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.Create();
        return await connection.QueryFirstOrDefaultAsync<Student>(
            "SELECT * FROM students WHERE id = @Id",
            new { Id = id });
    }
}
```

### Library Framework Requirements

**Required NuGet Packages:**
| Package | Purpose | Version |
|---------|---------|---------|
| `Dapper` | Micro-ORM | 2.1.35+ |
| `DbUp` | Database migrations | 5.0.0+ |
| `DbUp.SQLite` | SQLite support for DbUp | 5.0.0+ |

**Packages to REMOVE:**
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore` (any references)

### File Structure Requirements

**DbUp Migration Script Naming Convention:**
- Format: `YYYYMMDD_NN_Description.sql`
- Example: `20260107_0001_InitialCreate.sql`
- Maintain chronological order from EF Core migrations
- Each script should be idempotent where possible

**Repository Interface Pattern:**
```csharp
// Application/Interfaces/IEntityRepository.cs
public interface IEntityRepository
{
    Task<IEnumerable<Entity>> GetAllAsync();
    Task<Entity?> GetByIdAsync(int id);
    Task<int> CreateAsync(Entity entity);
    Task<bool> UpdateAsync(Entity entity);
    Task<bool> DeleteAsync(int id);
}
```

**SQL Query Constants Pattern:**
```csharp
// Infrastructure/Sql/EntityQueries.cs
public static class EntityQueries
{
    public const string GetById = @"
        SELECT * FROM entities
        WHERE id = @Id";

    public const string GetWithRelations = @"
        SELECT e.*, r.*
        FROM entities e
        LEFT JOIN related r ON r.entity_id = e.id
        WHERE e.id = @Id";
}
```

### Testing Requirements

**Integration Test Updates:**
- All existing integration tests MUST continue to pass
- Tests that mock `AppDbContext` need updating to mock repository interfaces
- No new test failures should be introduced

**E2E Test Requirements:**
- All existing E2E tests MUST continue to pass
- Verify API endpoints work identically to EF Core implementation
- Test data seeding should continue to work

**Manual Testing Checklist:**
- [ ] Authentication (login/logout) works
- [ ] Student CRUD operations work
- [ ] School CRUD operations work
- [ ] ClassGroup CRUD operations work
- [ ] Truck CRUD operations work
- [ ] Family CRUD operations work
- [ ] Activity CRUD operations work
- [ ] All foreign key relationships work
- [ ] All indexes and constraints work

### XSD Schema Alignment

**Critical from PRD [Source: _bmad-output/planning-artifacts/prd.md#XSD Alignment]:**

All entity implementations MUST strictly align with the legacy XSD schema definitions:
- School: 30 fields from `docs/legacy/1_School/School.xsd`
- Class Group: 15 fields from `docs/legacy/2_Class_Group/Class Group.xsd`
- Activity: 7 fields from `docs/legacy/3_Activity/Activity.xsd`
- Student: 92 fields from `docs/legacy/4_Children/Children.xsd`

**Database schema MUST preserve all XSD fields** - this is critical for data integrity.

### Known Complex Areas

**Student Entity (92 fields):**
- Most complex entity with extensive field mappings
- Contains personal information (POPIA-sensitive)
- Requires careful handling in SQL queries
- Consider using Dapper's `QueryAsync` with explicit column mapping

**Many-to-Many Relationships (StudentFamily):**
- Junction table requiring special handling
- EF Core automatically handled this; Dapper requires explicit queries
- Need separate methods for managing junction relationships

**Index Management:**
- EF Core migrations created indexes for performance
- DbUp scripts must recreate these indexes exactly
- Pay attention to unique indexes and foreign key indexes

### Implementation Sequence

**Recommended Order:**
1. Infrastructure foundation (IDbConnectionFactory, DbUp setup)
2. Convert EF Core migrations to DbUp SQL scripts
3. Create repository interfaces
4. Implement repositories one at a time (start with simple ones)
5. Update services to use repositories
6. Remove EF Core artifacts
7. Comprehensive testing

**Suggested Repository Implementation Order:**
1. Role (simplest, no dependencies)
2. User (simple, depends on Role)
3. Truck (simple reference data)
4. Activity (simple reference data)
5. School (medium complexity, 30 fields)
6. ClassGroup (medium complexity, 15 fields)
7. Family (medium complexity)
8. Student (most complex, 92 fields - do last)

### References

- **Architecture:** [Source: _bmad-output/planning-artifacts/architecture.md]
  - Data Access Patterns section
  - Repository Pattern guidelines
  - Migration Patterns (DbUp)
  - Project Structure section

- **PRD:** [Source: _bmad-output/planning-artifacts/prd.md]
  - XSD Schema Alignment requirements
  - Functional Requirements

- **Epics:** [Source: _bmad-output/planning-artifacts/epics.md]
  - Story 0.1 detailed requirements
  - Backend Changes section
  - Repository Implementation section

- **Codebase Analysis:**
  - Current DbContext: `apps/backend/src/Infrastructure/Data/AppDbContext.cs`
  - Entity Configurations: `apps/backend/src/Infrastructure/Data/Configurations/`
  - EF Core Migrations: `apps/backend/src/Infrastructure/Migrations/`
  - Domain Entities: `apps/backend/src/Domain/Entities/`
  - Current Services: `apps/backend/src/Application/*/Services/`

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

- Fixed missing `DbUp.SQLite` package reference by changing to `dbup-sqlite`.
- Recreated missing migration scripts in `apps/backend/src/Infrastructure/Migrations/Scripts/`.
- Created missing SQL query constants in `apps/backend/src/Infrastructure/Sql/`.
- Refactored legacy import services to use Dapper repositories instead of `AppDbContext`.
- Fixed ambiguous DTO references in `ClassGroupService`.
- Fixed `DayOfWeek` comparison issue in `ClassGroupService`.
- Added missing repository methods to `IRoleRepository`, `IFamilyRepository`, and `IClassGroupRepository`.

### Completion Notes List

- All EF Core artifacts removed.
- Dapper repositories implemented for all entities.
- DbUp migrations configured and scripts restored.
- Legacy import services refactored to use Dapper.
- Application builds successfully.

### File List
