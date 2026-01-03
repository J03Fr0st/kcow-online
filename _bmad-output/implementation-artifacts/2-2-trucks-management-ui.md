# Story 2.2: Trucks Management UI

Status: ready-for-dev

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

- [ ] Task 1: Create TruckService for frontend (AC: #1-4)
  - [ ] Create TruckService in core/services
  - [ ] Implement getTrucks(): Observable<Truck[]>
  - [ ] Implement getTruck(id): Observable<Truck>
  - [ ] Implement createTruck(data): Observable<Truck>
  - [ ] Implement updateTruck(id, data): Observable<Truck>
  - [ ] Implement deleteTruck(id): Observable<void>
  - [ ] Add loading signal
- [ ] Task 2: Create Trucks list page (AC: #1)
  - [ ] Create trucks-list component in features/trucks
  - [ ] Display table with Name, Registration, Status columns
  - [ ] Add "Add Truck" button
  - [ ] Implement row click for editing
  - [ ] Add delete action per row
- [ ] Task 3: Create Truck form component (AC: #2, #3, #5)
  - [ ] Create truck-form component (reusable for create/edit)
  - [ ] Use Reactive Forms
  - [ ] Add form fields: Name, Registration, Status, Notes
  - [ ] Add validation rules
  - [ ] Display inline validation errors
- [ ] Task 4: Implement create flow (AC: #2)
  - [ ] Open form in drawer or inline
  - [ ] Submit to API
  - [ ] Refresh list on success
  - [ ] Show success notification
- [ ] Task 5: Implement edit flow (AC: #3)
  - [ ] Load truck data into form
  - [ ] Submit updates to API
  - [ ] Refresh list on success
  - [ ] Show success notification
- [ ] Task 6: Implement delete flow (AC: #4)
  - [ ] Show inline confirmation (no modal)
  - [ ] Call delete API
  - [ ] Remove from list on success
- [ ] Task 7: Add loading states (AC: #6)
  - [ ] Show loading spinner during list load
  - [ ] Show loading state on form submit
  - [ ] Disable form during submission
- [ ] Task 8: Configure routing
  - [ ] Add trucks feature routes
  - [ ] Lazy load trucks module

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

### Completion Notes List

### File List
