# KCOW Online

Administration for mobile computer-literacy classes: students, families, schools, trucks, scheduling, attendance, evaluations and billing. Angular 21 SPA, .NET 10 API, SQLite, Dapper and DbUp.

## Start locally

Install .NET 10 SDK, Node.js 22 and Python 3.13 or newer. From the repository root:

```powershell
npm ci --prefix apps/frontend
dotnet restore apps/backend/Kcow.Backend.sln
dotnet run --project apps/backend/src/Api --launch-profile http
```

In a second terminal run `npm run dev --prefix apps/frontend`. Open `http://localhost:4200`; the API listens on `http://localhost:5039`. Development startup completes migrations and seeds the local admin (`admin@kcow.local` / `Admin123!`) before accepting requests. These are development fixtures. Set `ConnectionStrings__DefaultConnection` to an absolute SQLite path when choosing another database.

## Verify changes

```powershell
python scripts/check_boundaries.py
dotnet test apps/backend/Kcow.Backend.sln
python scripts/check_cli.py
python scripts/check_operations.py
python scripts/check_legacy_tools.py
npm run typecheck --prefix apps/frontend
npm test --prefix apps/frontend -- --runInBand
npm run build --prefix apps/frontend
```

These checks run in `.github/workflows/ci.yml`. Backend integration, CLI and operational checks use temporary databases. Jest covers Angular unit/HTTP contracts. Playwright under `apps/frontend/e2e` is separate: it requires both servers, an isolated E2E database and installed browsers. It is not included in Jest or the current CI job.

## Navigate the code

| Area | Location |
| --- | --- |
| HTTP/CLI entry points | `apps/backend/src/Api` |
| Use cases, DTOs and ports | `apps/backend/src/Application` |
| Entities | `apps/backend/src/Domain` |
| SQL, migrations, authentication adapters, import execution | `apps/backend/src/Infrastructure` |
| Pages, models and data services | `apps/frontend/src/app/features` |
| Session, transport and shared application services | `apps/frontend/src/app/core` |

Read [architecture](docs/architecture.md) for contracts, [operations](docs/operations.md) before migration/deployment/import, and [project context](docs/project_context.md) before code changes. The [remediation record](docs/plans/architecture-remediation.md) maps the September 2026 review to implementation and evidence.
