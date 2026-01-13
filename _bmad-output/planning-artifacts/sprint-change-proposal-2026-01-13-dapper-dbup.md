# Sprint Change Proposal: EF Core to Dapper + DbUp Migration

**Date:** 2026-01-13
**Author:** Joe (via Correct Course Workflow)
**Status:** Approved ✅
**Change Scope:** Minor (Documentation Updates Only)

---

## Section 1: Issue Summary

### Problem Statement

The current architecture specifies Entity Framework Core as the ORM for data access. EF Core's complexity, abstraction overhead, and "magic" behaviors make it difficult to work with effectively. For a single-admin application with straightforward CRUD operations, a lighter-weight approach using Dapper (micro-ORM) and DbUp (migration runner) provides explicit SQL control, simpler debugging, and reduced friction.

### Context

- **Discovery:** Proactive architectural decision during pre-implementation review
- **Timing:** Backend not yet implemented - optimal time for change
- **Driver:** Developer preference for explicit SQL over abstracted ORM

### Evidence

| EF Core Pain Point | Dapper/DbUp Solution |
|-------------------|---------------------|
| Complex change tracking | Explicit SQL - you see exactly what runs |
| Migration generation "magic" | Hand-written SQL scripts with full control |
| Debugging generated SQL | SQL is visible in code |
| Heavy abstraction layer | Thin wrapper over ADO.NET |
| Configuration ceremony | Simple repository pattern |

---

## Section 2: Impact Analysis

### Epic Impact

| Epic | Impact Level | Details |
|------|--------------|---------|
| Epic 1: Foundation & Auth | Medium | Story 1.1 backend scaffold updates |
| Epic 2: Trucks & Schools | Medium | Stories 2.1, 2.3 entity/API updates |
| Epic 3: Class Groups | Medium | Story 3.1 entity/API updates |
| Epic 4: Students & Families | Medium | Stories 4.1, 4.2 entity/API updates |
| Epic 5: Attendance | Medium | Stories 5.1, 5.4 entity/API updates |
| Epic 6: Billing | Medium | Story 6.1 entity/API updates |
| Epic 7: Legacy Migration | Low | Import logic unchanged |
| Epic 8: Activities | Medium | Story 8.1 entity/API updates |

**Summary:** All 8 epics remain valid. Changes affect implementation approach, not delivered features.

### Story Impact

**12 stories require acceptance criteria updates:**
- Story 1.1: Backend Project Scaffold
- Story 2.1: Truck Entity & API
- Story 2.3: School Entity & API
- Story 3.1: Class Group Entity & API
- Story 4.1: Student Entity & API
- Story 4.2: Family Entity & API
- Story 5.1: Attendance Entity & API
- Story 5.4: Evaluation Entity & API
- Story 6.1: Billing Entity & API
- Story 8.1: Activity Entity & API
- Plus architecture reference sections

### Artifact Conflicts

| Artifact | Impact | Changes Required |
|----------|--------|------------------|
| Architecture.md | **High** | Data access rewrite, project structure, patterns |
| PRD.md | Low | Minor technology references |
| Epics.md | **High** | 12 story acceptance criteria updates |
| docs/index.md | Low | Backend stack reference |
| UX Design | None | No UI/UX impact |

### Technical Impact

| Area | Current | Proposed |
|------|---------|----------|
| ORM | EF Core | Dapper |
| Migrations | EF Core Migrations | DbUp SQL Scripts |
| Data Layer | DbContext + Configs | Repositories + SQL |
| Connection | EF managed | IDbConnectionFactory |

---

## Section 3: Recommended Approach

### Selected Path: Direct Adjustment

**Rationale:**

| Factor | Assessment |
|--------|------------|
| Implementation effort | **Low** - Documentation updates only |
| Code changes required | **None** - Backend not yet built |
| Timeline impact | **Minimal** - Single session |
| Technical risk | **Low** - Mature technologies |
| Developer experience | **Positive** - Preferred tooling |

### Alternatives Considered

| Option | Decision | Reason |
|--------|----------|--------|
| Keep EF Core | Rejected | Developer friction, unnecessary complexity |
| Hybrid approach | Rejected | Two patterns adds confusion |
| **Dapper + DbUp** | **Selected** | Explicit, simple, preferred |

### Risk Assessment

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Learning curve | Low | Dapper is simpler than EF Core |
| Missing EF features | Low | Features not needed for this app |
| Migration complexity | Low | DbUp is straightforward |

---

## Section 4: Detailed Change Proposals

### 4.1 Architecture Document Changes

#### Change A: Data Architecture Section

**File:** `_bmad-output/planning-artifacts/architecture.md`

**OLD:**
```markdown
### Data Architecture

- **Database:** SQLite (v1), migration path to PostgreSQL.
- **ORM:** EF Core; entities in Domain, configs in Infrastructure/Data.
- **Migration:** EF Core migrations in `apps/backend/Migrations`.
```

**NEW:**
```markdown
### Data Architecture

- **Database:** SQLite (v1), migration path to PostgreSQL.
- **Data Access:** Dapper (micro-ORM) with repository pattern; entities in Domain, repositories in Infrastructure/Repositories.
- **Migrations:** DbUp with versioned SQL scripts in `apps/backend/Migrations/Scripts`.
- **SQL Patterns:** Explicit parameterized SQL; no dynamic query building. Store complex queries in `/Infrastructure/Sql/` as embedded resources or constants.
```

#### Change B: Decision Priority Analysis

**OLD:**
```markdown
**Critical Decisions (Block Implementation):**
- SQLite for v1 with EF Core and migration path.
```

**NEW:**
```markdown
**Critical Decisions (Block Implementation):**
- SQLite for v1 with Dapper + DbUp and migration path to PostgreSQL.

**Important Decisions (Shape Architecture):**
- Repository pattern for data access with explicit SQL.
```

#### Change C: Project Structure

**OLD:**
```markdown
│   └── Infrastructure/
│       ├── Data/               # EF Core configs, DbContext
│       └── Migrations/
```

**NEW:**
```markdown
│   └── Infrastructure/
│       ├── Repositories/       # Dapper repositories per entity
│       ├── Database/           # Connection factory, DbUp bootstrapper
│       ├── Sql/                # Complex SQL queries as constants/resources
│       └── Migrations/
│           └── Scripts/        # Versioned SQL scripts (001_InitialSchema.sql, etc.)
```

#### Change D: New Data Access Patterns Section

**ADD after Loading State Patterns:**
```markdown
### Data Access Patterns

**Repository Pattern:**
- One repository interface per aggregate root in `Application/`
- Repository implementations in `Infrastructure/Repositories/`
- Repositories receive `IDbConnection` via constructor injection

**SQL Patterns:**
- Use parameterized queries exclusively (never string concatenation)
- Simple queries inline in repository methods
- Complex queries as constants in `Infrastructure/Sql/{Entity}Queries.cs`
- Use Dapper's `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`

**Connection Management:**
- `IDbConnectionFactory` creates connections per request
- Connections are disposed after each operation
- Transaction support via `IDbTransaction` passed to repository methods

**Migration Patterns (DbUp):**
- Scripts in `Migrations/Scripts/` with naming: `YYYYMMDD_NN_Description.sql`
- Scripts are idempotent where possible
- DbUp runs automatically on application startup in Development
- Production migrations run via CLI command
```

---

### 4.2 PRD Changes

**File:** `_bmad-output/planning-artifacts/prd.md`

**OLD:**
```markdown
1. **Backend Base Setup**: Create ASP.NET Core Web API project (net10.0) in `apps/backend/` with:
   - Clean Architecture folder structure (Api, Application, Domain, Infrastructure)
   - EF Core + SQLite database configuration
```

**NEW:**
```markdown
1. **Backend Base Setup**: Create ASP.NET Core Web API project (net10.0) in `apps/backend/` with:
   - Clean Architecture folder structure (Api, Application, Domain, Infrastructure)
   - Dapper + SQLite database configuration with repository pattern
   - DbUp migration runner with versioned SQL scripts
```

---

### 4.3 Epics Document Changes

**File:** `_bmad-output/planning-artifacts/epics.md`

| Story | Section | Old Text | New Text |
|-------|---------|----------|----------|
| 1.1 | AC | "EF Core with SQLite is configured with a DbContext" | "Dapper with SQLite is configured with IDbConnectionFactory" + "DbUp migration runner is configured" |
| 2.1 | AC | "EF Core configuration exists in Infrastructure/Data" | "TruckRepository exists in Infrastructure/Repositories" |
| 2.3 | AC | "EF Core configuration and migration create the schools table" | "SchoolRepository and DbUp migration script create the schools table" |
| 3.1 | AC | "EF Core configuration with foreign key relationships" | "ClassGroupRepository with methods handling relationships" |
| 4.1 | AC | "EF Core configuration with foreign key relationships" | "StudentRepository with methods handling relationships" |
| 4.2 | AC | (join table reference) | "DbUp migration creates student_family join table" + "FamilyRepository handles queries" |
| 5.1 | AC | "EF Core configuration with foreign key relationships" | "AttendanceRepository with methods handling relationships" |
| 5.4 | AC | "EF Core configuration and migrations" | "EvaluationRepository and DbUp migration scripts" |
| 6.1 | AC | "EF Core configuration and migrations create billing tables" | "BillingRepository and DbUp migration scripts create billing tables" |
| 8.1 | AC | "EF Core configuration and migration create activities table" | "ActivityRepository and DbUp migration script create activities table" |

**Architecture Reference Section:**

**OLD:**
```markdown
- **ORM**: EF Core with entities in Domain layer, configurations in Infrastructure/Data.
```

**NEW:**
```markdown
- **Data Access**: Dapper with repository pattern; entities in Domain layer, repositories in Infrastructure/Repositories.
- **Migrations**: DbUp with versioned SQL scripts in Infrastructure/Migrations/Scripts.
```

---

## Section 5: Implementation Handoff

### Change Scope Classification

**Classification:** Minor

**Rationale:** All changes are documentation updates. No code has been written yet, so this is purely updating specifications before implementation begins.

### Handoff Recipients

| Role | Responsibility |
|------|----------------|
| Developer (Joe) | Update documentation per this proposal |
| Developer (Joe) | Implement backend using Dapper + DbUp per updated specs |

### Implementation Steps

1. **Update Architecture.md** with all 4 changes (A, B, C, D)
2. **Update PRD.md** with technology reference change
3. **Update Epics.md** with all 12 story acceptance criteria changes
4. **Optionally update docs/index.md** backend stack reference

### Success Criteria

- [ ] All documentation updated to reference Dapper + DbUp
- [ ] No remaining references to EF Core in planning artifacts
- [ ] Backend implementation follows new patterns
- [ ] First repository (Story 1.1) validates the pattern works

### Dependencies

None - this is a documentation-only change with no code dependencies.

---

## Approval

**Proposal Status:** Approved ✅

**Approver:** Joe

**Decision:** [x] Approved / [ ] Rejected / [ ] Revise

**Approval Date:** 2026-01-13

**Notes:** All changes have been applied to planning artifacts (Architecture, Epics).

---

*Generated by Correct Course Workflow on 2026-01-13*
