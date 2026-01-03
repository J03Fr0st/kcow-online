# Story 3.4: Scheduling Conflict Detection

Status: ready-for-dev

## Story

As an **admin**,
I want **the system to detect scheduling conflicts before I save**,
so that **I don't accidentally double-book a truck (FR6)**.

## Acceptance Criteria

1. **Given** I am creating or editing a class group
   **When** I select a truck, day, and time that overlaps with an existing class group
   **Then** a Schedule Conflict Banner appears warning me of the conflict
   **And** the conflicting class group details are shown

2. **Given** a conflict is detected
   **When** I try to save
   **Then** I am blocked from saving until the conflict is resolved
   **And** I can adjust the time or truck to resolve

3. **Given** I resolve the conflict
   **When** I save
   **Then** the class group is saved successfully
   **And** no conflict warning appears

4. **And** this fulfills FR6 (detect and resolve scheduling conflicts)

## Tasks / Subtasks

- [ ] Task 1: Create conflict detection API (AC: #1)
  - [ ] Add endpoint POST `/api/class-groups/check-conflicts`
  - [ ] Accept truckId, dayOfWeek, startTime, endTime, excludeId (for edits)
  - [ ] Return list of conflicting class groups
- [ ] Task 2: Create ConflictBanner component (AC: #1)
  - [ ] Display warning with conflicting group details
  - [ ] Show school name, time, and day
  - [ ] Style as prominent warning banner
- [ ] Task 3: Wire conflict check to form (AC: #1, #2)
  - [ ] Call conflict check on truck/day/time change
  - [ ] Debounce API calls
  - [ ] Display ConflictBanner when conflicts found
- [ ] Task 4: Block save on conflict (AC: #2)
  - [ ] Disable save button when conflicts exist
  - [ ] Show message explaining how to resolve
- [ ] Task 5: Handle conflict resolution (AC: #3)
  - [ ] Re-check conflicts when fields change
  - [ ] Clear banner when no conflicts
  - [ ] Enable save button

## Dev Notes

### Conflict Detection Logic

A conflict exists when:
- Same truck is assigned
- Same day of week
- Time slots overlap (startA < endB && startB < endA)
- Excluding the current class group being edited (by id)

### Conflict Check API

```typescript
// Request
interface ConflictCheckRequest {
  truckId: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  excludeId?: number; // Exclude self when editing
}

// Response
interface ConflictCheckResponse {
  hasConflicts: boolean;
  conflicts: {
    id: number;
    name: string;
    schoolName: string;
    startTime: string;
    endTime: string;
  }[];
}
```

### Conflict Banner Component

```html
<div class="alert alert-warning" *ngIf="conflicts.length > 0">
  <span>⚠️ Schedule Conflict Detected</span>
  <div>
    This truck is already assigned to:
    <ul>
      <li *ngFor="let conflict of conflicts">
        {{ conflict.name }} at {{ conflict.schoolName }} 
        ({{ conflict.startTime }} - {{ conflict.endTime }})
      </li>
    </ul>
    <p>Please adjust the time or choose a different truck.</p>
  </div>
</div>
```

### Previous Story Dependencies

- **Story 3.3** provides: Schedule time slot fields in form

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.4]
- [Source: _bmad-output/planning-artifacts/prd.md#FR6]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Schedule Conflict Banner]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
