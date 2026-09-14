# Project context for code changes

Updated 12 September 2026. Read [architecture](architecture.md) when changing modules, billing, imports or HTTP behavior. Read [operations](operations.md) before changing startup, migrations, deployment or backup. Historical plans are not evidence of current runtime behavior.

## Constraints

- Preserve legacy fields and limits in `docs/legacy/*/*.xsd`; inspect the matching schema before changing DTOs/mappers. Do not add legacy-domain fields without user authorization. Operational metadata such as `billing_commands` is separate.
- Keep Application independent of Infrastructure, Dapper and SQLite. Policy belongs to Application features; database/technology implementations belong to Infrastructure. API composes both. Add narrow ports for concrete use cases, not a universal repository/mediator or a microservice split.
- Keep frontend feature data/models under `features`; core must not import feature-owned code. Auth models live in `core/auth/models`. Preserve `HttpErrorResponse` through core interceptors. Set Angular input signals with component inputs in tests.
- Billing state, audit and command receipts must commit together. Preserve invoice ownership and key/payload replay contracts. Do not automatically retry writes. A retention change requires an explicit reconciliation plan.
- Import preview/help must remain database-free. Execute the immutable plan, propagate cancellation, and preserve/document accepted-row per-entity commits. Do not retire legacy tools without fixture and caller equivalence.
- Add numbered DbUp scripts; do not rewrite applied migrations. Production startup must not migrate or seed authentication. Use isolated absolute database paths for tests and migration experiments.

## Validation

Run the root [README](../README.md) commands. `dotnet test` builds the API used by `scripts/check_cli.py` and `scripts/check_operations.py`; both exercise temporary databases. Run Angular Jest in `apps/frontend` or use explicit `--prefix`. Root `npm test` has different workspace behavior. Keep Playwright separate from Jest discovery; component tests do not prove browser E2E behavior.

Verify observable contracts at real boundaries and run relevant existing checks. Keep source changes reviewable. Do not commit, deploy or migrate live data unless requested. Report failed checks and environmental limitations accurately.
