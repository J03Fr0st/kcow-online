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
