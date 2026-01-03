# Story 2.2: Trucks Management UI

Status: review

## Story

As an **admin**,
I want **a trucks management page to view and manage fleet records**,
so that **I can maintain accurate truck information**.

## Acceptance Criteria

1. **Given** I am authenticated and click "Trucks" in the sidebar
   **When** the Trucks page loads
   **Then** I see a table listing all trucks with Name, Registration, Status columns
   **And** I see "Add Truck" button

2. **Given** I click "Add Truck"
   **When** the create form appears
   **Then** I can enter truck details and save
   **And** the new truck appears in the list

3. **Given** I click a truck row
   **When** the edit form appears
   **Then** I can update truck details and save
   **And** I see a success confirmation

4. **Given** I click delete on a truck
   **When** I confirm the action
   **Then** the truck is archived (soft-deleted) and removed from the active list

5. **And** validation errors display inline

6. **And** loading states are shown during API calls

## Tasks / Subtasks

- [x] Task 1: Create TruckService for frontend (AC: #1-4)
  - [x] Create TruckService in core/services
  - [x] Implement getTrucks(): Observable<Truck[]>
  - [x] Implement getTruck(id): Observable<Truck>
  - [x] Implement createTruck(data): Observable<Truck>
  - [x] Implement updateTruck(id, data): Observable<Truck>
  - [x] Implement deleteTruck(id): Observable<void>
  - [x] Add loading signal
- [x] Task 2: Create Trucks list page (AC: #1)
  - [x] Create trucks-list component in features/trucks
  - [x] Display table with Name, Registration, Status columns
  - [x] Add "Add Truck" button
  - [x] Implement row click for editing
  - [x] Add delete action per row
- [x] Task 3: Create Truck form component (AC: #2, #3, #5)
  - [x] Create truck-form component (reusable for create/edit)
  - [x] Use Reactive Forms
  - [x] Add form fields: Name, Registration, Status, Notes
  - [x] Add validation rules
  - [x] Display inline validation errors
- [x] Task 4: Implement create flow (AC: #2)
  - [x] Open form in drawer or inline
  - [x] Submit to API
  - [x] Refresh list on success
  - [x] Show success notification
- [x] Task 5: Implement edit flow (AC: #3)
  - [x] Load truck data into form
  - [x] Submit updates to API
  - [x] Refresh list on success
  - [x] Show success notification
- [x] Task 6: Implement delete flow (AC: #4)
  - [x] Show inline confirmation (no modal)
  - [x] Call delete API
  - [x] Remove from list on success
- [x] Task 7: Add loading states (AC: #6)
  - [x] Show loading spinner during list load
  - [x] Show loading state on form submit
  - [x] Disable form during submission
- [x] Task 8: Configure routing
  - [x] Add trucks feature routes
  - [x] Lazy load trucks module

## Dev Notes

### Architecture Compliance

- **TruckService in `core/services/`** - Uses Angular Signals for loading state
- **Components in `features/trucks/`** - Standalone, OnPush change detection
- **No global loading overlays** - Use local spinners only
- **No modals for confirmations** - Use inline confirmation rows

### Technology Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| Angular | 21 | Standalone components |
| Reactive Forms | - | Required |
| DaisyUI | Latest | Table, button, form components |

### Critical Rules

- **OnPush change detection** for all components
- **Reactive Forms only** - no template-driven
- **No `any` type** - use typed models
- **Services own state** - components are presentation-only
- **No modals** - Use inline confirmations or drawers

### TruckService Pattern

```typescript
@Injectable({ providedIn: 'root' })
export class TruckService {
  private readonly http = inject(HttpClient);
  
  readonly trucks = signal<Truck[]>([]);
  readonly loading = signal(false);
  
  loadTrucks(): void {
    this.loading.set(true);
    this.http.get<Truck[]>('/api/trucks').subscribe({
      next: (trucks) => this.trucks.set(trucks),
      complete: () => this.loading.set(false)
    });
  }
  
  createTruck(data: CreateTruckRequest): Observable<Truck> {
    return this.http.post<Truck>('/api/trucks', data);
  }
  
  // ... other methods
}
```

### Truck Model (frontend)

```typescript
export interface Truck {
  id: number;
  name: string;
  registrationNumber: string;
  status: 'Active' | 'Maintenance' | 'Retired';
  notes?: string;
}

export interface CreateTruckRequest {
  name: string;
  registrationNumber: string;
  status: string;
  notes?: string;
}
```

### File Structure

```
apps/frontend/src/app/
├── core/
│   └── services/
│       └── truck.service.ts
├── features/
│   └── trucks/
│       ├── routes.ts
│       ├── trucks-list/
│       │   ├── trucks-list.component.ts
│       │   └── trucks-list.component.html
│       ├── truck-form/
│       │   ├── truck-form.component.ts
│       │   └── truck-form.component.html
│       └── models/
│           └── truck.model.ts
└── app.routes.ts (add trucks lazy load)
```

### Previous Story Dependencies

- **Story 1.4** provides: AdminLayout with sidebar navigation
- **Story 2.1** provides: Backend API endpoints for trucks

### Testing Requirements

- Component test: Trucks list displays trucks from service
- Component test: Form validation works
- Component test: Create flow calls service and refreshes
- Component test: Delete with inline confirmation

### UI/UX Requirements

- DaisyUI dark theme
- Table with hover states
- Loading spinner during data fetch
- Inline form for create/edit (drawer or slide-in panel)
- Inline confirmation for delete (no modal)
- Success notifications (toast or inline)

### References

- [Source: _bmad-output/planning-artifacts/architecture.md#Frontend Architecture]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#No Modals]
- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.2]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

- `npm run dev` - Start frontend and backend concurrently for testing

### Completion Notes List

- Created TruckService in core/services with Angular Signals for state management
- Implemented full CRUD operations (getTrucks, getTruck, createTruck, updateTruck, deleteTruck)
- Built trucks-list component with table view, inline delete confirmation, and drawer form
- Created reusable truck-form component with Reactive Forms and validation
- Implemented create and edit flows with drawer panel UI
- Added inline delete confirmation (no modal per UX requirements)
- Implemented loading states using Angular Signals (loading spinner + form disable)
- Configured routing in app.routes.ts with lazy loading
- All components use OnPush change detection
- All forms are Reactive Forms (no template-driven forms)
- No `any` types used - fully typed interfaces
- Services own all state - components are presentation-only

### File List

**Services:**
- apps/frontend/src/app/core/services/truck.service.ts

**Components:**
- apps/frontend/src/app/features/trucks/trucks-list/trucks-list.component.ts
- apps/frontend/src/app/features/trucks/trucks-list/trucks-list.component.html
- apps/frontend/src/app/features/trucks/truck-form/truck-form.component.ts
- apps/frontend/src/app/features/trucks/truck-form/truck-form.component.html

**Models:**
- apps/frontend/src/app/features/trucks/models/truck.model.ts

**Routing:**
- apps/frontend/src/app/app.routes.ts (added trucks route)

**Menu:**
- apps/frontend/src/app/models/menu-item.model.ts (Trucks link already existed in sidebar)

### Change Log

- 2026-01-04: Implemented complete Trucks Management UI with create, edit, delete, and list views
