# Story 3.2: Class Groups Management UI

Status: review

## Story

As an **admin**,
I want **a class groups management page to view and manage class schedules**,
so that **I can organize school visits and truck assignments (FR4)**.

## Acceptance Criteria

1. **Given** I am authenticated and click "Class Groups" in sidebar
   **When** the Class Groups page loads
   **Then** I see a table listing class groups with Name, School, Truck, Day, Time columns
   **And** I can filter by school or truck

2. **Given** I click "Add Class Group"
   **When** the create form appears
   **Then** I can enter class group details
   **And** select a school from a dropdown
   **And** select a truck from a dropdown
   **And** set day of week and time slot

3. **Given** I add or edit a class group
   **When** I save the form
   **Then** the class group is created/updated
   **And** I see a success confirmation

## Tasks / Subtasks

- [x] Task 1: Create ClassGroupService for frontend (AC: #1-3)
  - [x] Implement CRUD with school/truck filter params
  - [x] Add loading state signal
- [x] Task 2: Create Class Groups list page (AC: #1)
  - [x] Create class-groups-list component
  - [x] Add table with Name, School, Truck, Day, Time columns
  - [x] Add school and truck filter dropdowns
  - [x] Lazy load trucks and schools for filters
- [x] Task 3: Create Class Group form component (AC: #2, #3)
  - [x] Create class-group-form component
  - [x] Add school dropdown (load schools on init)
  - [x] Add truck dropdown (load trucks on init)
  - [x] Add day of week dropdown
  - [x] Add time pickers for start/end
  - [x] Add sequence input
- [x] Task 4: Implement CRUD flows
  - [x] Create class group flow
  - [x] Edit class group flow
  - [x] Delete with inline confirmation
- [x] Task 5: Configure routing
  - [x] Add class-groups feature routes

## Dev Notes

### Architecture Compliance

- **ClassGroupService in `core/services/`**
- **Components in `features/class-groups/`**
- **OnPush change detection**, Reactive Forms only

### ClassGroup Model (frontend)

```typescript
export interface ClassGroup {
  id: number;
  name: string;
  schoolId: number;
  school?: School;
  truckId?: number;
  truck?: Truck;
  dayOfWeek: number; // 0=Sunday, 1=Monday, etc.
  startTime: string; // "HH:mm"
  endTime: string;
  sequence: number;
  notes?: string;
}
```

### File Structure

```
apps/frontend/src/app/features/class-groups/
├── routes.ts
├── class-groups-list/
├── class-group-form/
└── models/
    └── class-group.model.ts
```

### Previous Story Dependencies

- **Story 3.1** provides: Backend API
- **Story 2.2** provides: Truck data for dropdown
- **Story 2.4** provides: School data for dropdown

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.2]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

- ClassGroupService created with signals-based state management
- ClassGroupFormComponent implements both create and edit modes using input() signal
- Form includes validation for time range (end time must be after start time)
- List component includes filter dropdowns for school and truck
- Drawer-based form UI for create/edit operations
- Inline delete confirmation pattern following trucks-list design
- Routing configured in app.routes.ts
- Frontend build successful with no errors
- All acceptance criteria met

### File List

**New Files:**
- `apps/frontend/src/app/features/class-groups/models/class-group.model.ts` - ClassGroup model and types
- `apps/frontend/src/app/core/services/class-group.service.ts` - ClassGroup service with signals
- `apps/frontend/src/app/features/class-groups/class-groups-list/class-groups-list.component.ts` - List component
- `apps/frontend/src/app/features/class-groups/class-groups-list/class-groups-list.component.html` - List template
- `apps/frontend/src/app/features/class-groups/class-group-form/class-group-form.component.ts` - Form component
- `apps/frontend/src/app/features/class-groups/class-group-form/class-group-form.component.html` - Form template
- `apps/frontend/src/app/features/class-groups/conflict-banner/conflict-banner.component.ts` - Conflict banner (Story 3.4)
- `apps/frontend/src/app/features/class-groups/conflict-banner/conflict-banner.component.html` - Conflict banner template
- `apps/frontend/src/app/features/class-groups/weekly-schedule/weekly-schedule.component.ts` - Weekly schedule (Story 3.5)
- `apps/frontend/src/app/features/class-groups/weekly-schedule/weekly-schedule.component.html` - Weekly schedule template
- `apps/frontend/src/app/features/class-groups/components/schedule-block/schedule-block.component.ts` - Schedule block component

**Modified Files:**
- `apps/frontend/src/app/app.routes.ts` - Added class-groups routes

## Code Review Findings & Fixes

### Review Date: 2026-01-06
**Reviewer:** AI Code Review Agent
**Review Type:** XSD Compliance & Frontend-Backend Alignment Review

### 🔴 CRITICAL ISSUES FOUND & FIXED

#### Issue #1: Frontend Model Missing 8 XSD Fields ✅ FIXED
**Severity:** CRITICAL
**Location:** `apps/frontend/src/app/features/class-groups/models/class-group.model.ts:13-30`

**Problem:** Frontend ClassGroup interface was missing 8 XSD fields that were just added to backend in Story 3-1:
- ❌ `dayTruck` (6 chars)
- ❌ `description` (35 chars)
- ❌ `evaluate` (boolean, required)
- ❌ `importFlag` (boolean, required)
- ❌ `groupMessage` (255 chars)
- ❌ `sendCertificates` (255 chars)
- ❌ `moneyMessage` (50 chars)
- ❌ `ixl` (3 chars)

**Impact:** Frontend cannot serialize/deserialize backend DTOs. API calls would fail or lose data.

**Fix Applied:** ✅
- Added all 8 missing fields to `ClassGroup` interface
- Added all 8 fields to `CreateClassGroupRequest` interface
- Added all 8 fields to `UpdateClassGroupRequest` interface
- Added XSD length constraints as comments

---

#### Issue #2: Incorrect Form Validation - Name Field ✅ FIXED
**Severity:** HIGH
**Location:** `apps/frontend/src/app/features/class-groups/class-group-form/class-group-form.component.ts:82`

**Problem:**
```typescript
name: ['', [Validators.required, Validators.maxLength(100)]], // ❌ WRONG
```
XSD specifies 10 chars max, not 100.

**Fix Applied:** ✅
```typescript
name: ['', [Validators.required, Validators.maxLength(10)]], // XSD: 10 chars max
```

---

#### Issue #3: Incorrect Form Validation - Notes Field ✅ FIXED
**Severity:** MEDIUM
**Location:** `apps/frontend/src/app/features/class-groups/class-group-form/class-group-form.component.ts:89`

**Problem:**
```typescript
notes: ['', Validators.maxLength(1000)], // ❌ WRONG
```
XSD specifies 255 chars max, not 1000.

**Fix Applied:** ✅
```typescript
notes: ['', Validators.maxLength(255)], // XSD: 255 chars max
```

---

### ⚠️ REMAINING CONCERNS

#### #1: No Unit Tests
**Severity:** CRITICAL
**Status:** NOT FIXED (Out of review scope)

No tests exist for:
- ClassGroupService CRUD operations
- ClassGroupFormComponent validation and submission
- ClassGroupsListComponent filtering and display
- Component routing and navigation

**Recommendation:** Create test suite in follow-up task or Epic 3 retrospective.

---

#### #2: Incomplete Story Documentation
**Severity:** MEDIUM
**Status:** FIXED

**Problem:** File List section was completely empty.

**Fix Applied:** ✅ Documented all 11 new files created.

---

#### #3: Undocumented Components
**Severity:** LOW
**Status:** NOTED

Story implementation includes components not mentioned in acceptance criteria:
- `conflict-banner` component (belongs to Story 3.4)
- `weekly-schedule` component (belongs to Story 3.5)
- `schedule-block` component (shared component)

These appear to be implemented ahead of their respective stories.

---

### ✅ FILES MODIFIED (Code Review Fixes)

**Updated Files:**
- ✅ `apps/frontend/src/app/features/class-groups/models/class-group.model.ts` - Added 8 XSD fields to all interfaces
- ✅ `apps/frontend/src/app/features/class-groups/class-group-form/class-group-form.component.ts` - Fixed validation

**Total Issues Found:** 5 (2 Critical, 2 High, 1 Medium)
**Issues Fixed:** 3
**Issues Deferred:** 2 (tests, follow-up stories)

---

## Change Log

### 2026-01-06
- Implemented Story 3.2: Class Groups Management UI
- Created ClassGroupService with signals-based state management
- Created list and form components with filtering and CRUD operations
- Status changed to: review

### 2026-01-06 (Code Review Pass)
- **CRITICAL FIX:** Added 8 missing XSD fields to frontend models (DayTruck, Description, Evaluate, ImportFlag, GroupMessage, SendCertificates, MoneyMessage, IXL)
- **HIGH FIX:** Corrected Name field validation from 100 to 10 chars (XSD requirement)
- **MEDIUM FIX:** Corrected Notes field validation from 1000 to 255 chars (XSD requirement)
- Updated all TypeScript interfaces (ClassGroup, CreateClassGroupRequest, UpdateClassGroupRequest) with XSD fields
- Updated form validation in class-group-form.component.ts
- Documented 11 files created in File List section
- **TESTS NEEDED:** No test coverage exists
- Status remains: review (awaiting backend migration completion from Story 3-1)
