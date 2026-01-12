**🔥 CODE REVIEW FINDINGS, Joe!**

**Story:** `_bmad-output/implementation-artifacts/8-2-activities-management-ui.md`
**Git vs Story Discrepancies:** 0 found
**Issues Found:** 0 High, 2 Medium, 1 Low

## 🟡 MEDIUM ISSUES
- **MIME Type Handling Bug**: The `ActivityFormComponent` accepts any image (`image/*`) and saves raw base64. The `ActivitiesListComponent` hardcodes the display prefix to `data:image/png;base64,`. Uploading a JPEG/GIF will result in an incorrect MIME type prefix, which is malformed data (though some browsers may tolerate it).
- **UI Logic Inconsistency**: The Activities table has a "Status" column that handles "Inactive" items (`badge-ghost`), implying the list can show archived activities. However, the `deleteActivity` action optimistically *removes* the item from the list entirely, rendering the "Inactive" badge logic unreachable for the current session.

## 🟢 LOW ISSUES
- **Upload Size Risk**: The 5MB client-side validation limit for base64 images is risky. A 5MB image becomes ~6.7MB in base64, which likely exceeds the default ASP.NET Core request body limit (often ~30MB for IIS but lower for Kestrel defaults or proxies).

## Recommendations
1.  **Fix MIME Type**: Restrict upload to PNG only OR store the full Data URI.
2.  **Fix UI Logic**: Update `deleteActivity` to set `isActive = false` locally instead of removing the item, so the "Inactive" badge confirms the action.
3.  **Safety**: Lower upload limit to 2MB.
