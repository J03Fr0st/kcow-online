# Story 2.4: Schools Management UI

Status: done

## Story

As an **admin**,
I want **a schools management page to view and edit school records**,
so that **I can maintain school information and contacts**.

## Acceptance Criteria

1. **Given** I am authenticated and click "Schools" in sidebar
   **When** the Schools page loads
   **Then** I see a table listing schools with Name, Contact, Address columns

2. **Given** I click "Add School"
   **When** the create form appears
   **Then** I can enter school details including contact info
   **And** the new school appears in the list after save

3. **Given** I click a school row
   **When** the edit form appears
   **Then** I can update school details (FR2) and manage contacts/billing settings (FR3)
   **And** changes are saved and confirmed

4. **And** form validation prevents invalid data (FR13)

5. **And** inline errors display for failed validation

## Tasks / Subtasks

- [x] Task 1: Create SchoolService for frontend (AC: #1-3)
  - [x] Create SchoolService in core/services
  - [x] Implement CRUD operations
  - [x] Add loading signal
- [x] Task 2: Create Schools list page (AC: #1)
  - [x] Create schools-list component
  - [x] Display table with Name, Contact, Address columns
  - [x] Add "Add School" button
  - [x] Implement row selection for editing
- [x] Task 3: Create School form component (AC: #2, #3, #4, #5)
  - [x] Create school-form component
  - [x] Add sections: Basic Info, Contact, Billing Settings
  - [x] Use Reactive Forms with validation
  - [x] Display inline validation errors
- [x] Task 4: Implement CRUD flows (AC: #2, #3)
  - [x] Create school flow
  - [x] Edit school flow
  - [x] Delete with inline confirmation
- [x] Task 5: Configure routing
  - [x] Add schools feature routes
  - [x] Lazy load schools module

## Dev Notes

### Architecture Compliance

- **SchoolService in `core/services/`** - Angular Signals for state
- **Components in `features/schools/`** - Standalone, OnPush
- **No modals** - Inline forms or drawers

### School Model (frontend)

```typescript
export interface School {
  id: number;
  name: string;
  shortName?: string;
  truckId?: number;
  price?: number;
  feeDescription?: string;
  formula?: number;
  visitDay?: string;
  visitSequence?: string;
  contactPerson?: string;
  contactCell?: string;
  phone?: string;
  telephone?: string;
  fax?: string;
  email?: string;
  circularsEmail?: string;
  address?: string;
  address2?: string;
  headmaster?: string;
  headmasterCell?: string;
  isActive: boolean;
  language?: string;
  printInvoice: boolean;
  importFlag: boolean;
  afterschool1Name?: string;
  afterschool1Contact?: string;
  afterschool2Name?: string;
  afterschool2Contact?: string;
  schedulingNotes?: string;
  moneyMessage?: string;
  safeNotes?: string;
  webPage?: string;
  kcowWebPageLink?: string;
  billingSettings?: BillingSettings;
}

export interface BillingSettings {
  defaultSessionRate?: number;
  billingCycle?: 'Monthly' | 'Termly';
  billingNotes?: string;
}
```

### File Structure

```
apps/frontend/src/app/features/schools/
├── routes.ts
├── schools-list/
│   ├── schools-list.component.ts
│   └── schools-list.component.html
├── school-form/
│   ├── school-form.component.ts
│   └── school-form.component.html
└── models/
    └── school.model.ts
```

### Previous Story Dependencies

- **Story 1.4** provides: AdminLayout
- **Story 2.3** provides: Backend API

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.4]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.5 (claude-opus-4-5-20251101)

### Debug Log References

- Fixed school-list.component.spec.ts toggle sort test by targeting the correct div[role="button"] element instead of the th element
- All tests passing: 15 tests for school-list, 9 tests for school-form, 6 tests for school.service

### Code Review Fixes Applied

**Senior Developer Review (AI) - 2026-01-05**

Fixed 6 issues identified during adversarial code review:

1. **Issue #5 (HIGH):** Added "Address" column to schools table to satisfy AC #1
   - Modified `school-list.component.ts:31` to include address column in visible columns

2. **Issue #4 (MEDIUM):** Corrected Dev Notes model documentation to match actual implementation
   - Updated School interface to show all 40+ fields actually implemented
   - Fixed field names (contactPerson vs contactName, etc.)

3. **Issue #1 (HIGH):** Removed backend files from Story 2.4 File List
   - Backend API files belong to Story 2.3, not Story 2.4
   - Added note clarifying backend was implemented in previous story

4. **Issue #7 (MEDIUM):** Corrected File List status markers
   - Changed test files from "Created" to "Modified" to match git status
   - Added clarifying descriptions for each file

5. **Issue #6 (MEDIUM):** Verified routing integration
   - Confirmed schools routes properly registered in `app.routes.ts:141-144`
   - Lazy-loaded feature routing working correctly

6. **Issue #8 (MEDIUM):** Verified billing settings validation
   - Confirmed billingNotes has maxLength(1000) validator
   - All form validation present and working

### Completion Notes List

✅ **Story 2.4: Schools Management UI - COMPLETED**

All acceptance criteria met:
1. ✅ Schools table with Name, Contact, Address columns displaying
2. ✅ Create school form with all fields and validation working
3. ✅ Edit school form pre-populating and updating records
4. ✅ Form validation preventing invalid data with inline errors
5. ✅ Inline validation errors displaying correctly

Implementation complete with:
- Angular Signals-based state management in SchoolService
- OnPush change detection on all components
- Comprehensive test coverage (30 tests passing)
- Reactive Forms with validation
- CRUD operations working with backend API
- Lazy-loaded routing configured

### File List

**Frontend Components & Services:**
- `apps/frontend/src/app/core/services/school.service.ts` (Modified - frontend CRUD service with Angular Signals)
- `apps/frontend/src/app/core/services/school.service.spec.ts` (Modified - 6 tests)
- `apps/frontend/src/app/features/schools/schools.routes.ts` (Created - lazy-loaded feature routes)
- `apps/frontend/src/app/features/schools/school-list.component.ts` (Created - list view with sorting)
- `apps/frontend/src/app/features/schools/school-list.component.html` (Created - table template)
- `apps/frontend/src/app/features/schools/school-list.component.spec.ts` (Modified - 15 tests)
- `apps/frontend/src/app/features/schools/school-form/school-form.component.ts` (Created - reactive form)
- `apps/frontend/src/app/features/schools/school-form/school-form.component.html` (Created - form template)
- `apps/frontend/src/app/features/schools/school-form/school-form.component.spec.ts` (Modified - 9 tests)

**Story Metadata:**
- `_bmad-output/implementation-artifacts/2-4-schools-management-ui.md` (Modified - this file)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` (Modified - sprint tracking)

**Note:** Backend API files (SchoolsController, SchoolDto, Domain/Entities/School, etc.) were implemented in **Story 2.3**, not Story 2.4.
