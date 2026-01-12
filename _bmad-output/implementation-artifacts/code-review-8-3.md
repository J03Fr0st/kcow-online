**🔥 CODE REVIEW FINDINGS, Joe!**

**Story:** `_bmad-output/implementation-artifacts/8-3-data-migration-activities.md`
**Git vs Story Discrepancies:** 0 found
**Issues Found:** 0 High, 2 Medium, 1 Low

## 🟡 MEDIUM ISSUES
- **Base64 OLE Header Risk**: The implementation assumes the `Icon` field in XML is raw base64 image data. In Access, "OLE Object" fields often contain OLE headers/wrappers before the actual image data (BMP/JPEG). If these headers aren't stripped, the base64 string will be invalid for direct use in `<img>` tags.
- **Preview Mode Logic Flaw**: The CLI `Program.cs` logic passes `null` for audit/summary paths in preview mode (`preview ? null : auditPath`). However, `LegacyActivityImportService.ImportAsync` executes `_context.Activities.Add(mapping.Activity)` and `_context.SaveChangesAsync` regardless of a "preview" flag (which isn't passed to the service method). The `LegacyActivityImportService.ImportAsync` method signature in `Program.cs` usage implies it doesn't take a preview boolean, meaning **preview mode will still commit data to the database**.

## 🟢 LOW ISSUES
- **Namespace Import Inconsistency**: `LegacyActivityImportService.cs` uses `using Kcow.Infrastructure.Data;` but `LegacyActivityXmlParser` is in `Kcow.Application.Import`. While functional, the layered architecture separation is slightly blurred by having infrastructure service depending directly on application layer parsers (usually fine, but worth noting for strict clean architecture).

## Recommendations
1.  **Fix Preview Mode**: Update `LegacyActivityImportService.ImportAsync` to accept a `bool preview` parameter and skip `SaveChangesAsync` (and maybe `Add`) if true.
2.  **Investigate OLE Headers**: Add a TODO or logic to check for/strip OLE headers from the base64 string if the `docs/legacy` data confirms they exist.
3.  **Refactor**: Ensure clean separation or explicit acceptance of the dependency direction.
