# Story 3.3: Schedule Time Slot Configuration

Status: review

## Story

As an **admin**,
I want **to set class group schedules with day, time, and sequence**,
so that **I can organize the weekly school visit calendar (FR5)**.

## Acceptance Criteria

1. **Given** I am creating or editing a class group
   **When** I configure the schedule
   **Then** I can select:
   - Day of week (Monday-Friday dropdown)
   - Start time and end time
   - Sequence number (order within the day)

2. **Given** I assign a truck to the class group
   **When** I save
   **Then** the truck assignment is persisted
   **And** the schedule appears in a weekly view or list

3. **And** this fulfills FR5 (set class group schedules and assign trucks)

## Tasks / Subtasks

- [x] Task 1: Enhance class group form (AC: #1)
  - [x] Add day of week dropdown (Monday-Friday)
  - [x] Add time picker components for start/end time
  - [x] Add sequence number input with validation
  - [x] Add time validation (end > start)
- [x] Task 2: Handle truck assignment (AC: #2)
  - [x] Truck dropdown already present from 3.2
  - [x] Ensure truck assignment persists
- [x] Task 3: Display schedule in list view (AC: #2)
  - [x] Format day/time nicely in table
  - [x] Sort by day then sequence

## Dev Notes

### Schedule Fields

| Field | Component | Validation |
|-------|-----------|------------|
| dayOfWeek | Select dropdown | Required, 1-5 (Mon-Fri) |
| startTime | Time picker | Required, HH:mm format |
| endTime | Time picker | Required, must be > startTime |
| sequence | Number input | Required, >= 1 |

### Time Picker Pattern

```typescript
// Using DaisyUI input with type="time"
<input type="time" formControlName="startTime" class="input input-bordered" />
```

### Day of Week Display

```typescript
const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
// Only show Monday-Friday (1-5) in dropdown
```

### Previous Story Dependencies

- **Story 3.2** provides: Class group form with basic fields

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.3]
- [Source: _bmad-output/planning-artifacts/prd.md#FR5]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

- Day of week dropdown restricted to Monday-Friday (school days only)
- DAY_OF_WEEK_OPTIONS updated to only include weekdays
- Day mapping functions updated to match .NET DayOfWeek enum (Monday=1, etc.)
- List component now sorts by dayOfWeek then sequence
- Time range validation already implemented (end time > start time)
- Sequence input has min(1) validation
- Truck assignment persisted through existing form functionality
- Schedule displayed in table with formatted day name and time range
- All acceptance criteria met

### File List
