**🔥 CODE REVIEW FINDINGS, Joe!**

**Story:** `_bmad-output/implementation-artifacts/2-7-e2e-tests-trucks-schools-crud.md`
**Git vs Story Discrepancies:** 0 found (Files verified in commit `a52ad5b`)
**Issues Found:** 0 High (Wait...), 1 Critical, 1 High, 2 Medium, 1 Low.

## 🔴 CRITICAL ISSUES
- **Conditional Test Logic (Soft Assertions):**
  - Files: `trucks-crud.spec.ts`, `schools-crud.spec.ts`, `data-integrity.spec.ts`
  - Problem: Extensive use of `if (await locator.count() > 0) { ... }` blocks.
  - Impact: If a field or button is renamed or missing, the test **skips** the interaction and passes silently, instead of failing. This defeats the purpose of E2E testing.
  - Fix: Replace `if` checks with `await expect(locator).toBeVisible()` followed by the action.

## 🔴 HIGH ISSUES
- **Performance/Flakiness (Hard Waits):**
  - Files: All spec files.
  - Problem: ~40 occurrences of `await page.waitForTimeout(500)` or `1000`.
  - Impact: Slows down test suite artificially and masks race conditions.
  - Fix: Use `await expect().toBeVisible()` or specific state waits.

## 🟡 MEDIUM ISSUES
- **Test Isolation (State Leakage):**
  - Problem: Tests create entities ("E2E Test Truck ...") but don't clean them up in `afterEach`.
  - Impact: Subsequent tests run against a polluted database, potentially causing false failures or passes.
  - Fix: Implement API-based cleanup in `afterEach` or use a fresh DB per test worker (complex). At minimum, use unique prefixes and clean up via UI or API.
- **Maintainability (No Page Objects):**
  - Problem: Raw locators and duplication across 3 files.
  - Impact: UI changes require updating 3+ files.

## 🟢 LOW ISSUES
- **Brittle Selectors:**
  - Problem: `text=/add|new|create/i` is imprecise.
  - Fix: Use exact text or `data-testid`.
