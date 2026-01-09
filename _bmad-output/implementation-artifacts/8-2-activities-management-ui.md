# Story 8.2: Activities Management UI

Status: ready-for-dev

## Story

As an admin,
I want an activities management page to view and manage the program catalog,
so that I can maintain the educational activities offered.

## Acceptance Criteria

1. **Given** I am authenticated and click "Activities" in sidebar, **When** the Activities page loads, **Then** I see a table listing activities with Icon thumbnail, Code, Name, Grade Level, Folder, Status columns
2. **And** I see "Add Activity" button
3. **Given** I click "Add Activity", **When** the create form appears, **Then** I can enter activity details including:
   - Code, Name, Description, Folder, Grade Level
   - Icon upload (image file converted to base64 on save)
4. **And** the new activity appears in the list with icon thumbnail
5. **Given** I click an activity row, **When** the edit form appears, **Then** I can update activity details and save
6. **And** I see a success confirmation
7. **Given** I click delete on an activity, **When** I confirm the action, **Then** the activity is archived (soft-deleted) and removed from the active list
8. **And** validation errors display inline
9. **And** loading states are shown during API calls

## Tasks / Subtasks

- [ ] Task 1: Create Activity Feature Module Structure (AC: #1)
  - [ ] Create `features/activities/` directory
  - [ ] Create `activities.routes.ts` with lazy loading
  - [ ] Add route to main app routes

- [ ] Task 2: Create Activity Models (AC: #1, #3)
  - [ ] Create `models/activity.model.ts`
  - [ ] Define Activity interface matching backend DTO
  - [ ] Define CreateActivityRequest interface
  - [ ] Define UpdateActivityRequest interface

- [ ] Task 3: Create Activity Service (AC: #1, #3, #5, #7)
  - [ ] Create `core/services/activity.service.ts`
  - [ ] Implement CRUD operations using Angular Signals
  - [ ] Follow TruckService pattern exactly
  - [ ] Add loading state signal
  - [ ] Add activities signal for reactive list

- [ ] Task 4: Create Activities List Component (AC: #1, #2, #7, #9)
  - [ ] Create `activities-list/activities-list.component.ts`
  - [ ] Create HTML template with DaisyUI table
  - [ ] Display Icon thumbnail, Code, Name, GradeLevel, Folder columns
  - [ ] Add "Add Activity" button
  - [ ] Add edit/delete actions per row
  - [ ] Implement delete confirmation (inline, no modal)
  - [ ] Use OnPush change detection
  - [ ] Show loading spinner during API calls

- [ ] Task 5: Create Activity Form Component (AC: #3, #4, #5, #6, #8)
  - [ ] Create `activity-form/activity-form.component.ts`
  - [ ] Use Reactive Forms
  - [ ] Create form fields: Code, Name, Description, Folder, GradeLevel
  - [ ] Add Icon file upload with base64 conversion
  - [ ] Show icon preview when uploaded
  - [ ] Add validation (max lengths per XSD)
  - [ ] Display inline validation errors
  - [ ] Support create and edit modes
  - [ ] Emit success events for notifications

- [ ] Task 6: Add Activities to Sidebar Navigation (AC: #1)
  - [ ] Add "Activities" link to sidebar
  - [ ] Add appropriate icon (e.g., academic cap or puzzle)
  - [ ] Place after Students, before Import

- [ ] Task 7: Style Icon Thumbnails (AC: #1, #4)
  - [ ] Display base64 icon as small thumbnail in table
  - [ ] Handle missing icons gracefully (placeholder)
  - [ ] Size: 32x32 or 40x40 pixels

- [ ] Task 8: Test UI Functionality (AC: all)
  - [ ] Test create flow
  - [ ] Test edit flow
  - [ ] Test delete flow
  - [ ] Test validation errors
  - [ ] Test icon upload and display

## Dev Notes

### Architecture Compliance

- **Follow Trucks feature module pattern exactly**
- **OnPush change detection is MANDATORY for all components**
- **Angular Signals for UI state, RxJS for async flows only**
- **Components are presentation-only, services own data/state**
- **Reactive Forms only, no template-driven forms**
- **No `any` type - use proper TypeScript types**

### File Locations

| Component | Path |
|-----------|------|
| Routes | `apps/frontend/src/app/features/activities/activities.routes.ts` |
| Models | `apps/frontend/src/app/features/activities/models/activity.model.ts` |
| Service | `apps/frontend/src/app/core/services/activity.service.ts` |
| List Component | `apps/frontend/src/app/features/activities/activities-list/` |
| Form Component | `apps/frontend/src/app/features/activities/activity-form/` |

### Model Definitions (activity.model.ts)

```typescript
export interface Activity {
  id: number;
  code: string | null;
  name: string | null;
  description: string | null;
  folder: string | null;
  gradeLevel: string | null;
  icon: string | null;  // base64 encoded
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateActivityRequest {
  code?: string;
  name?: string;
  description?: string;
  folder?: string;
  gradeLevel?: string;
  icon?: string;  // base64 encoded
}

export interface UpdateActivityRequest {
  code?: string;
  name?: string;
  description?: string;
  folder?: string;
  gradeLevel?: string;
  icon?: string;
}
```

### Service Pattern (Follow TruckService)

```typescript
@Injectable({ providedIn: 'root' })
export class ActivityService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl + '/activities';

  // Signals for reactive state
  activities = signal<Activity[]>([]);
  loading = signal<boolean>(false);

  loadActivities(): void { ... }
  getActivity(id: number): Observable<Activity> { ... }
  createActivity(request: CreateActivityRequest): Observable<Activity> { ... }
  updateActivity(id: number, request: UpdateActivityRequest): Observable<Activity> { ... }
  deleteActivity(id: number): Observable<void> { ... }
}
```

### Icon Upload Implementation

```typescript
// In activity-form.component.ts
onIconSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files[0]) {
    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = () => {
      // reader.result is base64 data URL
      const base64 = (reader.result as string).split(',')[1];
      this.form.patchValue({ icon: base64 });
      this.iconPreview.set(reader.result as string);
    };
    reader.readAsDataURL(file);
  }
}
```

### Routing Configuration

```typescript
// activities.routes.ts
export const ACTIVITIES_ROUTES: Routes = [
  {
    path: '',
    component: ActivitiesListComponent,
  },
];

// app.routes.ts - add lazy loading
{
  path: 'activities',
  loadChildren: () => import('./features/activities/activities.routes')
    .then(m => m.ACTIVITIES_ROUTES),
},
```

### UI Components (DaisyUI)

- Table: `table table-zebra`
- Icon thumbnail: `avatar` with `w-10 h-10 rounded`
- Buttons: `btn btn-primary`, `btn btn-ghost`, `btn btn-error`
- Form inputs: `input input-bordered`
- Textarea: `textarea textarea-bordered`
- File input: `file-input file-input-bordered`
- Loading: `loading loading-spinner`

### Validation Rules (from XSD)

| Field | Max Length | Required |
|-------|------------|----------|
| Code | 255 | No |
| Name | 255 | No |
| Folder | 255 | No |
| GradeLevel | 255 | No |
| Description | Unlimited (memo) | No |
| Icon | Large (base64) | No |

### Project Structure Notes

- Feature lazy-loaded under `/activities` route
- Accessible from sidebar after authentication
- Uses DaisyUI dark theme components
- Desktop-first responsive design

### References

- [Source: apps/frontend/src/app/features/trucks/] - Complete reference implementation
- [Source: apps/frontend/src/app/core/services/truck.service.ts] - Service pattern
- [Source: _bmad-output/planning-artifacts/architecture.md#Frontend-Architecture] - Angular patterns
- [Source: docs/project_context.md#Framework-Specific-Rules] - Angular rules

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
