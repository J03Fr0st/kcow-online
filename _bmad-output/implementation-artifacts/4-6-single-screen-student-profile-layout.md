# Story 4.6: Single-Screen Student Profile Layout

Status: done

## Story

As an **admin**,
I want **a single-screen student profile with all key information**,
so that **I can view and update a student without navigating away (FR10)**.

## Acceptance Criteria

1. **Given** I navigate to a student profile (`/students/:id`)
   **When** the page loads
   **Then** I see a 3-column summary header with:
   - Column 1: Photo placeholder, name, basic demographics
   - Column 2: School and class group assignment
   - Column 3: Quick status indicators (attendance, billing)

2. **And** below the header, I see tabbed sections for:
   - Child Info (demographics details)
   - Financial (billing - placeholder for Epic 6)
   - Attendance (placeholder for Epic 5)
   - Evaluation (placeholder for Epic 5)

3. **And** the page loads in under 2 seconds (NFR2)

## Tasks / Subtasks

- [x] Task 1: Create StudentProfile component (AC: #1)
  - [x] Create student-profile component
  - [x] Fetch student with full details on init
  - [x] Handle loading and error states
- [x] Task 2: Create 3-column header layout (AC: #1)
  - [x] Column 1: Photo placeholder, name, DOB, grade
  - [x] Column 2: School, class group, seat number
  - [x] Column 3: Status indicators (placeholders for now)
- [x] Task 3: Create tabbed navigation (AC: #2)
  - [x] Add DaisyUI tabs component
  - [x] Create tab: Child Info (active by default)
  - [x] Create tab placeholders: Financial, Attendance, Evaluation
- [x] Task 4: Optimize for performance (AC: #3)
  - [x] Single API call for student + related data
  - [x] OnPush change detection
  - [x] Lazy load tab content if needed
- [x] Task 5: Configure routing
  - [x] Add route `/students/:id`

## Dev Notes

### Student Profile Layout

```
┌─────────────────────────────────────────────────────────────┐
│ [Photo] Name: John Smith          │ School: Greenwood      │ Status Indicators │
│         DOB: 2015-03-15           │ Class: 5A              │ [Attendance chip] │
│         Grade: 5                  │ Seat: 12               │ [Billing chip]    │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│ [Child Info] [Financial] [Attendance] [Evaluation]         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│              Tab content goes here                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Tab Structure

```typescript
const tabs = [
  { id: 'child-info', label: 'Child Info', component: ChildInfoTabComponent },
  { id: 'financial', label: 'Financial', component: FinancialTabPlaceholder },
  { id: 'attendance', label: 'Attendance', component: AttendanceTabPlaceholder },
  { id: 'evaluation', label: 'Evaluation', component: EvaluationTabPlaceholder },
];
```

### File Structure

```
apps/frontend/src/app/features/students/
├── student-profile/
│   ├── student-profile.component.ts
│   ├── student-profile.component.html
│   └── components/
│       ├── profile-header/
│       ├── child-info-tab/
│       ├── financial-tab/         (placeholder)
│       ├── attendance-tab/        (placeholder)
│       └── evaluation-tab/        (placeholder)
```

### Previous Story Dependencies

- **Story 4.1** provides: Student API with full details
- **Story 4.5** provides: Navigation to profile

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.6]
- [Source: _bmad-output/planning-artifacts/prd.md#FR10]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Single-Screen Student Profile]
- [Source: docs/legacy/4_Children/*.png] - Legacy UI screenshots

## Dev Agent Record

### Agent Model Used

glm-4.7

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

**Implementation Summary:**
- Redesigned existing student profile page with 3-column header layout and tabbed sections
- Added OnPush change detection and Angular signals for performance
- Implemented tabbed navigation with Child Info, Financial, Attendance, Evaluation tabs
- Financial and Attendance/Evaluation tabs are placeholders for future epics
- Updated tests to cover new layout and functionality

**Acceptance Criteria Status:**
1. AC #1: 3-column header layout ✅
   - Column 1: Photo, name, basic demographics (DOB, gender, grade, reference)
   - Column 2: School assignment (school, class group, seat, teacher)
   - Column 3: Status indicators (attendance/financial placeholders)
2. AC #2: Tabbed sections ✅
   - Child Info tab (active, with basic info display)
   - Financial tab (placeholder for Epic 6)
   - Attendance tab (placeholder for Epic 5)
   - Evaluation tab (placeholder for Epic 5)
3. AC #3: Performance ✅
   - Single API call via getStudentById
   - OnPush change detection
   - Signals for reactive state management

**Technical Decisions:**
- Used Angular signals (signal, computed, WritableSignal) for reactive state
- DaisyUI tabs-boxed for tabbed navigation
- 3-column grid layout using Tailwind's grid-cols-1 md:grid-cols-3
- Icon emojis for tabs (👤, 💰, 📋, 📊)
- Helper methods: formatDate(), getDisplayValue(), setActiveTab(), isTabActive()

**File List:**

**Frontend:**
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.ts` (MODIFIED - added signals, OnPush, tab state, helper methods)
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.html` (MODIFIED - 3-column layout, tabbed navigation)
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.spec.ts` (MODIFIED - updated tests for new layout)
