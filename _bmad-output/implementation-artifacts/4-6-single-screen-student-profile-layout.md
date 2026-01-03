# Story 4.6: Single-Screen Student Profile Layout

Status: ready-for-dev

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

- [ ] Task 1: Create StudentProfile component (AC: #1)
  - [ ] Create student-profile component
  - [ ] Fetch student with full details on init
  - [ ] Handle loading and error states
- [ ] Task 2: Create 3-column header layout (AC: #1)
  - [ ] Column 1: Photo placeholder, name, DOB, grade
  - [ ] Column 2: School, class group, seat number
  - [ ] Column 3: Status indicators (placeholders for now)
- [ ] Task 3: Create tabbed navigation (AC: #2)
  - [ ] Add DaisyUI tabs component
  - [ ] Create tab: Child Info (active by default)
  - [ ] Create tab placeholders: Financial, Attendance, Evaluation
- [ ] Task 4: Optimize for performance (AC: #3)
  - [ ] Single API call for student + related data
  - [ ] OnPush change detection
  - [ ] Lazy load tab content if needed
- [ ] Task 5: Configure routing
  - [ ] Add route `/students/:id`

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
