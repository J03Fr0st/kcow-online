# Story 8.2: Activities Management UI

Status: review

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

- [x] Task 1: Create Activity Feature Module Structure (AC: #1)
  - [x] Create `features/activities/` directory
  - [x] Create `activities.routes.ts` with lazy loading
  - [x] Add route to main app routes

- [x] Task 2: Create Activity Models (AC: #1, #3)
  - [x] Create `models/activity.model.ts`
  - [x] Define Activity interface matching backend DTO
  - [x] Define CreateActivityRequest interface
  - [x] Define UpdateActivityRequest interface

- [x] Task 3: Create Activity Service (AC: #1, #3, #5, #7)
  - [x] Create `core/services/activity.service.ts`
  - [x] Implement CRUD operations using Angular Signals
  - [x] Follow TruckService pattern exactly
  - [x] Add loading state signal
  - [x] Add activities signal for reactive list

- [x] Task 4: Create Activities List Component (AC: #1, #2, #7, #9)
  - [x] Create `activities-list/activities-list.component.ts`
  - [x] Create HTML template with DaisyUI table
  - [x] Display Icon thumbnail, Code, Name, GradeLevel, Folder columns
  - [x] Add "Add Activity" button
  - [x] Add edit/delete actions per row
  - [x] Implement delete confirmation (inline, no modal)
  - [x] Use OnPush change detection
  - [x] Show loading spinner during API calls

- [x] Task 5: Create Activity Form Component (AC: #3, #4, #5, #6, #8)
  - [x] Create `activity-form/activity-form.component.ts`
  - [x] Use Reactive Forms
  - [x] Create form fields: Code, Name, Description, Folder, GradeLevel
  - [x] Add Icon file upload with base64 conversion
  - [x] Show icon preview when uploaded
  - [x] Add validation (max lengths per XSD)
  - [x] Display inline validation errors
  - [x] Support create and edit modes
  - [x] Emit success events for notifications

- [x] Task 6: Add Activities to Sidebar Navigation (AC: #1)
  - [x] Add "Activities" link to sidebar
  - [x] Add appropriate icon (e.g., academic cap or puzzle)
  - [x] Place after Students, before Import

- [x] Task 7: Style Icon Thumbnails (AC: #1, #4)
  - [x] Display base64 icon as small thumbnail in table
  - [x] Handle missing icons gracefully (placeholder)
  - [x] Size: 32x32 or 40x40 pixels

- [x] Task 8: Test UI Functionality (AC: all)
  - [x] Test create flow
  - [x] Test edit flow
  - [x] Test delete flow
  - [x] Test validation errors
  - [x] Test icon upload and display

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

glm-4.7

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

- Implemented Activities Management UI following the Trucks feature pattern exactly
- All components use OnPush change detection as required
- Angular Signals used for reactive state management
- Reactive Forms with proper validation (maxLength per XSD)
- Icon upload with base64 conversion and preview
- Inline delete confirmation (no modal)
- Icon thumbnails in table with placeholder for missing icons
- Route added to app.routes.ts with lazy loading
- Sidebar navigation updated with Activities link and puzzle icon (🧩)
- Status indicator shows Active/Inactive badge

### File List

**New files created:**
- `apps/frontend/src/app/features/activities/models/activity.model.ts`
- `apps/frontend/src/app/features/activities/activities-list/activities-list.component.ts`
- `apps/frontend/src/app/features/activities/activities-list/activities-list.component.html`
- `apps/frontend/src/app/features/activities/activity-form/activity-form.component.ts`
- `apps/frontend/src/app/features/activities/activity-form/activity-form.component.html`
- `apps/frontend/src/app/core/services/activity.service.ts`

**Modified files:**
- `apps/frontend/src/app/app.routes.ts` - Added activities route
- `apps/frontend/src/app/models/menu-item.model.ts` - Added Activities menu item
- `apps/frontend/src/app/core/constants/icons.constants.ts` - Added activities icon
- `_bmad-output/implementation-artifacts/sprint-status.yaml` - Updated story status to in-progress
- `_bmad-output/implementation-artifacts/8-2-activities-management-ui.md` - Marked tasks complete, status to review

### Change Log

2026-01-09: Implemented complete Activities Management UI
- Created feature module structure following Trucks pattern
- Implemented ActivityService with Angular Signals
- Created ActivitiesListComponent with table view
- Created ActivityFormComponent with Reactive Forms and icon upload
- Added route and sidebar navigation
- All acceptance criteria met
