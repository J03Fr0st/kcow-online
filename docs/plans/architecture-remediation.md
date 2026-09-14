# Architecture remediation

Base: `4035031cefc904b3d4f18fb0e7775e5062d70e4a`; branch `main`; clean working tree at implementation start.
Authorized scope: all recommendations in the 12 September 2026 architecture review. No commit, deployment or live-data migration.

| Slice | Outcome / acceptance | Status |
| --- | --- | --- |
| HTTP | No automatic write retry; original transport error preserved; session reset once through AuthService; real chain tests | Implemented and verified |
| Billing | Atomic payment/receipt/invoice/audit, ownership check, persisted request deduplication, real SQLite rollback/replay tests | Implemented and verified |
| Imports | Main CLI preview/help have no database effects; immutable plan feeds preview/execution; explicit partial commits/cancellation; compatibility tools build | Implemented and verified |
| Ownership | Application owns business policy; Infrastructure owns SQL; frontend business data services and auth models have explicit owners | Implemented and verified |
| Reads | Class-group and family lists use bounded queries, including missing optional relationships | Implemented and verified |
| Operations | Separate liveness/readiness; explicit migrations command; executable backup/restore verification and runbook | Implemented and verified locally |
| Enforcement | Current architecture and project context, setup README, boundary checks and CI configuration | Implemented and verified locally |
| Integration | Backend/frontend suites, builds, real CLI/operational checks, final diff review | Passed; deployment exclusions below |

Decisions: retain the four-project monolith; retain existing HTTP payload shapes and optional unallocated payments. Idempotency uses a header and additive operational storage, not changes to legacy XSD entities. Retain accepted-row/per-entity import commits but name and test that contract. Production migration/backup commands are documented and tested only against disposable data.

## Review findings and implementation

| Finding | Change | Evidence |
| --- | --- | --- |
| F1 unsafe retries | GET/HEAD allowlist; persisted actor/operation/key command receipts; SPA retains unresolved keys for manual retry and guards double submissions | Real interceptor chain, failed-response retry, SQLite replay and eight concurrent same-key requests |
| F2 nonatomic billing | `IBillingWriter` owns one transaction for command, final receipt, invoice status, audit and deduplication result | Fault injection at receipt, audit, invoice-status and command-receipt writes; every failure rolls back, subsequent retry succeeds |
| F3 cross-student links | Application-owned billing rule checks invoice ownership and rejects cancelled invoices | Real SQLite and HTTP tests; missing invoice/student and unallocated/partial/full payments covered |
| F4 preview writes | Lazy acquisition of write dependencies after arguments/preview; no auth seed for imports | Real executable help/preview leaves the configured database absent |
| F5 error/session contracts | Preserve `HttpErrorResponse`; AuthService owns clearing user/token state; auth interceptor owns 401 redirect | Login followed by 401 clears real service signals and navigates once; ProblemDetails status/body survives |
| F6 policy ownership | Move ten business services to Application, add narrow writer/read ports, move frontend feature data services and core auth models | Four-project graph, full tests/type check, forbidden-import rejection check |
| F7 import ownership | One immutable plan, source fingerprint, shared main orchestration, per-entity commit flags/logs, warning diagnostics, Ctrl+C cancellation, shared legacy tool database bootstrap | Snapshot mutation/source-change tests, rerun/mixed conflicts, cancellation after an entity commit; four legacy tool builds and real activity preview/import/rerun |
| F8 operations | Read-only readiness probe; explicit production migration command; backup script and deployment/restore runbook | Production missing DB: live 200 / ready 503 / no file; migrations twice; no auth seed; ready 200; verified backup, overwrite rejection and restored readiness |
| F9 read growth | One joined class-group query; family list plus one batch relationship read; JSON parameter for many family IDs | SQLite fixtures at 10/100/1,000 rows: one/two database operations; filtering, ordering, inactive rows and null related projections |
| F10 enforcement | Replace stale architecture/context; useful README; local CI workflow; real persistence/HTTP/tool tests | Full suites pass; dependency probe deliberately rejects a forbidden Application import; all documented smoke checks pass |

The old list loops implied up to `1 + 2N` class-group reads and `1 + N` family reads. The new fixtures verify constant one/two operations. The old counts are source-derived, not a timed baseline; no production latency improvement is claimed.

## Validation-driven repairs

Enabling the real checks exposed additional contract drift. Repairs include class-group signal consumption, selector refresh after async results, a valid family template alias, DestroyRef use, modal input binding, student-list error/retry/empty states, and correct null/date handling. Attendance batches now reach the repository's existing update logic instead of rejecting existing session rows. Legacy school imports preserve their supplied IDs; legacy billing reads typed decimals. Three legacy tools that referenced removed EF Core types now build against the existing repositories.

Test changes preserve observable behavior: Angular inputs use `setInput`, Jest mocks return actual Observables, fixtures satisfy required fields, invalid-ID tests avoid IDs allocated by other cases, and DOM assertions follow actual controls. Existing permissive legacy fallbacks were characterized rather than replaced with stricter rejection; missing generated identities/names now emit warnings. Database-backed billing tests replaced mock-only write tests after the replacement coverage passed.

## Final local verification — 12 September 2026

- Backend: **449 passed** (220 unit, 229 HTTP/integration), no skipped tests.
- Frontend: **732 passed** across 54 Jest suites; type checking and production bundle passed.
- Main CLI help/preview: passed with no database creation.
- Production readiness/migration/backup/restore smoke: passed on disposable databases.
- All four legacy tool projects build; help/read-only commands and activity preview/import/rerun smoke passed.
- Boundary check and deliberate forbidden-import rejection passed. `git diff --check` passed.
- CI configuration runs these commands, but the remote GitHub Actions job has not been executed or branch protection configured in this session.

## Deliberate compatibility choices and remaining limits

- Legacy tools/mappers are retained because their identity and normalization contracts differ. Shared setup and main plan orchestration were consolidated; silently replacing all legacy mapping/persistence paths or retiring callers would violate the report's compatibility gate.
- Server idempotency receipts are retained indefinitely. The SPA pending-key map survives manual retries only while its service instance lives; after reload or database restore, reconcile unknown outcomes before creating a new command. See [operations](../operations.md).
- Production deployment and migration `019_CreateBillingCommands.sql` were **not** applied to live data. Follow the migration/backup/readiness gate in the runbook before release.
- Existing NuGet warning `NU1903` remains for `SQLitePCLRaw.lib.e_sqlite3` 2.1.10. The [reviewed advisory](https://github.com/advisories/GHSA-2m69-gcr7-jv3q) lists versions through 2.1.11 as affected and no patched version under that package name. Changing the native distribution/provider needs separate compatibility verification; this work does not claim to resolve that advisory or constitute a dependency-security audit.
- Existing bundle-size/sourcemap and nullable-test warnings remain. Playwright/browser E2E and production load testing were not run. Unit/integration success does not prove every historical browser workflow or production configuration.
