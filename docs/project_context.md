---
project_name: 'kcow-online'
user_name: 'Joe'
date: '2026-01-05'
sections_completed:
  ['technology_stack', 'language_rules', 'framework_rules', 'testing_rules', 'quality_rules', 'workflow_rules', 'anti_patterns', 'xsd_alignment']
status: 'complete'
rule_count: 36
optimized_for_llm: true
updated_from: 'architecture-workflow'
---

# Project Context for AI Agents

_Critical rules and patterns AI agents must follow. Keep consistent with architecture and patterns already defined._

---

## ⚠️ CRITICAL: Strict XSD Alignment

**The legacy XSD schemas are the authoritative source of truth for all domain models.** All entity implementations, database schemas, and API contracts MUST strictly align with the XSD definitions in `docs/legacy/`.

- **School**: `docs/legacy/1_School/School.xsd` (30 fields)
- **Class Group**: `docs/legacy/2_Class_Group/Class Group.xsd` (15 fields)
- **Activity**: `docs/legacy/3_Activity/Activity.xsd` (7 fields)
- **Children**: `docs/legacy/4_Children/Children.xsd` (92 fields)

**XSD Alignment Rules:**
1. Implement ALL fields from XSD—no omissions
2. Use exact XSD field names (with case transformations per naming conventions)
3. **Use English field names** where XSD uses Afrikaans (e.g., `Trok` → `Truck`, `Taal` → `Language`). See `docs/domain-models.md` for translations.
4. Match XSD data types exactly (see `docs/domain-models.md` for type mappings)
5. Enforce XSD max lengths in database and validation
6. Honor `minOccurs` for required/optional designation
7. Do NOT add fields not present in XSD without explicit approval

---

## Technology Stack & Versions
- Frontend: Angular 21.0.2 (CLI scaffold), TypeScript, Tailwind + daisyUI, Reactive Forms, Angular HttpClient + interceptors.
- Backend: ASP.NET Core Web API (net10.0), Clean Architecture (Api/Application/Domain/Infrastructure).
- Data: EF Core with SQLite provider (migration path to Postgres later), EF migrations.
- Auth: Better Auth (TypeScript authentication framework), email/password with Admin role, HTTPS required.
- Logging/Errors: Serilog (API), ProblemDetails error responses; Angular interceptors for error handling.

## Critical Implementation Rules

### Language-Specific Rules
- TypeScript: strict; **`any` type is strictly forbidden**—use proper types, `unknown`, or generics; prefer typed RxJS streams; avoid default exports; use tsconfig paths/import aliases; async/await for HTTP; shared interceptors for error handling (avoid per-call duplication).
- C#: nullable enabled; async all the way with `CancellationToken`; DTOs immutable where practical; avoid throwing from controllers—map to ProblemDetails/result patterns.

### Framework-Specific Rules
- Angular: feature modules per domain; reactive forms only; validators/helpers in `shared/`; HTTP via interceptors (auth/error); components dumb, services own data/state.
- Angular State: **Angular Signals for UI state; RxJS for async flows only**. Services own data/state; components remain presentation-only.
- Angular Performance: **OnPush change detection is mandatory for all components**. Lazy-load feature routes under AdminLayout shell.
- Angular Routing: Features lazy-loaded via `loadChildren`; all admin routes under `/admin` prefix with AdminLayout wrapper.
- ASP.NET Core: Controllers in Api call Application services; no EF in controllers; DI everywhere; ProblemDetails middleware/filter; Better Auth integration; enforce HTTPS.
- EF Core: Entities in Domain; configs in Infrastructure/Data; no lazy loading; explicit includes; migrations in `apps/backend/Migrations`.

### Testing Rules
- Frontend: co-locate feature tests under `apps/frontend/tests/<feature>`; interaction tests over snapshot.
- Backend: unit test Application services/validators; integration tests for controllers with in-memory SQLite; use real EF in integration tests (no DbContext mocking there).

### Code Quality & Style Rules
- Naming: DB snake_case; API paths plural kebab; JSON camelCase; C# PascalCase types / camelCase locals; Angular files kebab-case, classes PascalCase.
- Lint/format: keep Angular ESLint/Prettier defaults; dotnet-format for C#; remove unused code/vars.
- Structure: follow documented layout (`apps/frontend` core/shared/features; `apps/backend` Api/Application/Domain/Infrastructure).
- API Response: Direct resource payload (no wrapper object); errors via ProblemDetails. Dates as ISO 8601 strings; booleans as true/false; explicit nulls (never omit required fields).
- Loading States: Service-level loading state with local component spinners. **Never use global "page lock" for partial updates**.

### Development Workflow Rules
- Scaffold only via CLI: Angular CLI for frontend; `dotnet new webapi -f net10.0` for backend.
- Migrations: create via `dotnet ef migrations add ...`, check in; apply automatically in dev, controlled in prod.
- HTTPS required; no HTTP-only endpoints.
- No schema drift: update models + migrations together.
- **Startup**: Use `npm run dev` from root to start both Frontend (Angular) and Backend (.NET watch) concurrently.
- **Debugging**: Use VS Code "Full Stack Debug (F5)" to debug both C# and TypeScript simultaneously.

### Critical Don't-Miss Rules / Anti-Patterns
- **Do not use `any` type in TypeScript**—use proper types, `unknown`, or generics. This is enforced by TypeScript compiler strict mode and must be followed in all code.
- Do not return EF entities from controllers—DTOs only.
- Do not bypass interceptors with ad-hoc HTTP error handling; rely on shared interceptor.
- Do not store secrets in source; use env/appsettings/user-secrets.
- Do not add a global event bus/state lib unless explicitly decided.
- Do not add rate limiting unless the decision is made (currently deferred).
- **Do not use default change detection**—OnPush is required for all components.
- Do not manage state in components—services own all data; components are presentation-only.
- Do not omit required JSON fields—use explicit null values.
- Do not create global loading overlays—use service-level loading with local spinners.
- Do not skip lazy-loading for feature modules—all features must lazy-load under AdminLayout.
- **Do not deviate from XSD schemas**—all entity fields, types, and constraints must match the legacy XSD definitions exactly.
- **Do not omit XSD fields**—every field defined in the XSD must be implemented in the domain model.
- **Do not add fields not in XSD**—new fields require explicit approval and documentation update.

---

## Legacy System Reference

The new application replaces a Microsoft Access database system. Consult legacy artifacts when implementing features:

| Resource | Location | Purpose |
|----------|----------|---------|
| **Student UI Screenshots** | `docs/legacy/4_Children/*.png` | Reference layouts for student management tabs |
| **Domain Schemas** | `docs/legacy/*/` | XSD files with complete field definitions |
| **UX Design Spec** | `docs/ux-design-specification.md` | Detailed legacy UI analysis and modernization guidance |
| **Domain Models** | `docs/domain-models.md` | TypeScript interface proposals based on legacy schemas |

### Legacy Workflow Types

| Type | Entities | Implementation Priority |
|------|----------|------------------------|
| **Maintenance** | School, Class Group, Activity | CRUD screens with simple forms |
| **Core** | Children (Students) | Full-featured module with 6 tabs |

When implementing student features, replicate the dense, information-rich layout from the legacy system. See `docs/ux-design-specification.md` for detailed tab layouts.

---

## Usage Guidelines

**For AI Agents:**
- Read this file before implementing any code.
- Follow ALL rules exactly as documented.
- When in doubt, prefer the more restrictive option.
- Update this file if new patterns emerge.
- Consult legacy UI screenshots when implementing student management features.

**For Humans:**
- Keep this file lean and focused on agent needs.
- Update when technology stack changes.
- Review quarterly for outdated rules.
- Remove rules that become obvious over time.

Last Updated: 2026-01-05
