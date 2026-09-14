# Operating KCOW

## Release ownership and readiness

The deployment operator owns backups, schema migration and promotion. The API does not run migrations or seed authentication in Production. CI verifies code and disposable databases; it does not deploy this repository. Keep one release operator/migrator at a time for the SQLite file.

Publish with `dotnet publish apps/backend/src/Api -c Release -o artifacts/api` and build the frontend with the README command. Retain the bundled `Migrations/Scripts` directory with the API. Configure `ASPNETCORE_ENVIRONMENT=Production`, an absolute `ConnectionStrings__DefaultConnection`, `Jwt__Key`, issuer/audience, listening URLs and the deployment's HTTPS/CORS settings. Store secrets outside source control. The migration command does not create an admin account; initial production account provisioning remains an operator responsibility.

1. Stop writes and take the API out of service. Confirm the actual database path and retain the previous application artifact.
2. Create a new verified backup using the command below. Keep its timestamp and application version with the release record.
3. From the published API directory, with the intended connection string set, run `dotnet Kcow.Api.dll database migrate`. Require exit code zero before proceeding. DbUp records applied scripts; rerunning applies only missing scripts. Do not edit previously applied SQL.
4. Start the API in Production. Require HTTP 200 from `/health/ready` before routing users to it. `/health` and `/api/health` prove only liveness. A missing database, pending migration or unusable schema must produce readiness 503.
5. Check an authenticated read and a controlled business workflow, then restore traffic. Record migration version, duration and readiness outcome. A failure stops promotion; do not use Development startup to bypass it.

Migration `019_CreateBillingCommands.sql` adds only operational command receipts. It does not alter legacy XSD entities. Old application binaries do not need that table, but they also do not provide the new replay/transaction guarantees. Prefer forward repair; if returning to an older artifact, keep writes stopped until its behavior and database compatibility have been assessed.

## Backup and restore

From the repository root, substitute the verified absolute paths:

```powershell
python scripts/backup_sqlite.py 'D:/KCOW/data/kcow.db' 'D:/KCOW/backups/kcow-20260912-before-release.db'
```

The script opens the source read-only, uses SQLite's backup API for a consistent snapshot, runs `PRAGMA integrity_check`, and refuses to overwrite an existing destination. It closes connections before returning. Keep backups access-controlled with the same care as the live database; they contain student, authentication and billing data. Retention and off-host storage depend on the deployment's recovery requirements.

To restore, stop the API and all writers. Preserve the current database for reconciliation. Restore into a **new** file using the same backup script with the saved backup as source, then point `ConnectionStrings__DefaultConnection` at the restored file. This avoids replacing a database that has live WAL/SHM sidecars. Apply migrations from the chosen application artifact if needed, start the API, and gate on readiness and an authenticated smoke check before traffic resumes.

Restoring rolls back business data and idempotency receipts together. Payments created after the backup may exist in external records while being absent locally. Reconcile those records before resubmission; an old key cannot deduplicate a receipt absent from the restored database. Never delete `billing_commands` as routine cleanup: keys have no expiry in this version.

`python scripts/check_operations.py` exercises a Production host with a missing database (live 200, ready 503, no file creation), runs migration twice, verifies no auth seed, verifies ready 200, backs up/restores to another file, and verifies restored readiness. All paths are temporary. This is a local executable release-contract check, not proof that a particular production host has been configured correctly.

## Import execution and reconciliation

Build the API, then run from the repository root:

```powershell
dotnet apps/backend/src/Api/bin/Debug/net10.0/Kcow.Api.dll import run --help
dotnet apps/backend/src/Api/bin/Debug/net10.0/Kcow.Api.dll import run --preview --input docs/legacy
```

Preview and help are database-free. A real run omits `--preview`; set an explicit absolute connection string and back up the target first. Use the command's help for conflict/report options. Capture console output and reports with the run ID, source fingerprint and per-entity commit outcomes. Ctrl+C requests cancellation and exits with code 130; completed entity outcomes have already been logged. Preview validates mapped input, while execution evaluates live conflicts/references against the database. Execution uses the plan snapshot, so later file edits do not change that run.

Accepted rows commit per entity type. The default conflict mode rejects conflicting rows and still commits accepted rows. Skipping/updating are explicit alternatives. After cancellation or a fatal failure, the current transaction rolls back but earlier entity commits may remain. Inspect the target and captured counts before rerunning. Legacy compatibility tools keep their existing mapper semantics; compare fixtures and caller expectations before replacing a tool with `import run`.

## Monitoring

Monitor readiness failures separately from liveness, DbUp failures/applied versions and migration duration, import accepted/rejected counts, and billing commit/replay logs. A lost billing response is an unknown outcome: retry with the same persisted key and payload or reconcile through the student's payment/invoice list. A changed payload with the same key is a 409 conflict. The current SPA keeps pending keys only for its service lifetime; after reload, reconcile before creating a new command. Do not log full financial request bodies to investigate retries.

Retained tools under `apps/backend/tools` support `--help`, `--count` and `--sample N`; the activity tool also supports `--preview`. All honor `ConnectionStrings__DefaultConnection`. Use an explicit absolute path: historical default paths differ between tools. Their read commands require an existing schema and do not migrate/seed. `python scripts/check_legacy_tools.py` builds all four tools and verifies help/read commands plus an activity preview/import/rerun against disposable data.
