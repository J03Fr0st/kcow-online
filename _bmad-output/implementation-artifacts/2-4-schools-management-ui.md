# Story 2.4: Schools Management UI

Status: ready-for-dev

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

- [ ] Task 1: Create SchoolService for frontend (AC: #1-3)
  - [ ] Create SchoolService in core/services
  - [ ] Implement CRUD operations
  - [ ] Add loading signal
- [ ] Task 2: Create Schools list page (AC: #1)
  - [ ] Create schools-list component
  - [ ] Display table with Name, Contact, Address columns
  - [ ] Add "Add School" button
  - [ ] Implement row selection for editing
- [ ] Task 3: Create School form component (AC: #2, #3, #4, #5)
  - [ ] Create school-form component
  - [ ] Add sections: Basic Info, Contact, Billing Settings
  - [ ] Use Reactive Forms with validation
  - [ ] Display inline validation errors
- [ ] Task 4: Implement CRUD flows (AC: #2, #3)
  - [ ] Create school flow
  - [ ] Edit school flow
  - [ ] Delete with inline confirmation
- [ ] Task 5: Configure routing
  - [ ] Add schools feature routes
  - [ ] Lazy load schools module

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
  address?: string;
  contactName?: string;
  contactPhone?: string;
  contactEmail?: string;
  billingSettings?: BillingSettings;
  notes?: string;
}

export interface BillingSettings {
  defaultSessionRate: number;
  billingCycle: 'Monthly' | 'Termly';
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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
