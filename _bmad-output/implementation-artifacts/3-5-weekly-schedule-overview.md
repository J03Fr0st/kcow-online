# Story 3.5: Weekly Schedule Overview

Status: ready-for-dev

## Story

As an **admin**,
I want **a visual weekly schedule view showing all class groups**,
so that **I can see truck utilization and school coverage at a glance**.

## Acceptance Criteria

1. **Given** I am on the Class Groups page
   **When** I switch to "Weekly View" tab
   **Then** I see a calendar-style grid with days as columns and time slots as rows
   **And** class groups are displayed as blocks on the grid

2. **Given** I click on a schedule block
   **When** the detail appears
   **Then** I see the class group name, school, and truck
   **And** I can navigate to edit the class group

3. **And** conflicts are visually highlighted with a warning indicator

## Tasks / Subtasks

- [ ] Task 1: Create WeeklyScheduleView component (AC: #1)
  - [ ] Create weekly-schedule component
  - [ ] Render grid with Monday-Friday columns
  - [ ] Render time slot rows (e.g., 7am-5pm in 30min or 1hr slots)
  - [ ] Position class groups as blocks based on time
- [ ] Task 2: Load and display class groups (AC: #1)
  - [ ] Fetch all class groups for the week
  - [ ] Group by day and calculate grid positions
  - [ ] Color-code by truck for visual distinction
- [ ] Task 3: Add block click interaction (AC: #2)
  - [ ] Show tooltip/popover with class group details
  - [ ] Add "Edit" link to navigate to form
- [ ] Task 4: Highlight conflicts (AC: #3)
  - [ ] Detect overlapping blocks for same truck
  - [ ] Add warning border/icon to conflicting blocks
- [ ] Task 5: Add tab navigation
  - [ ] Add tabs: List View / Weekly View
  - [ ] Persist selected view in route or state

## Dev Notes

### Weekly View Grid Layout

```
          | Monday   | Tuesday  | Wednesday | Thursday | Friday   |
----------|----------|----------|-----------|----------|----------|
 7:00 AM  |          |          |           |          |          |
 7:30 AM  | [Group A]|          |           | [Group C]|          |
 8:00 AM  | [Group A]| [Group B]|           | [Group C]|          |
 8:30 AM  |          | [Group B]|           |          |          |
 ...      |          |          |           |          |          |
```

### Grid Position Calculation

```typescript
function calculateBlockPosition(classGroup: ClassGroup) {
  const dayIndex = classGroup.dayOfWeek - 1; // Mon=0, Fri=4
  const startMinutes = parseTimeToMinutes(classGroup.startTime);
  const endMinutes = parseTimeToMinutes(classGroup.endTime);
  const gridStartTime = 7 * 60; // 7:00 AM in minutes
  
  return {
    column: dayIndex + 2, // CSS grid column (1 is time label)
    rowStart: Math.floor((startMinutes - gridStartTime) / 30) + 2,
    rowEnd: Math.floor((endMinutes - gridStartTime) / 30) + 2,
  };
}
```

### Class Group Block Styling

```css
.schedule-block {
  @apply rounded-md p-2 text-sm cursor-pointer;
  /* Color based on truck */
}

.schedule-block--conflict {
  @apply border-2 border-warning;
}
```

### File Structure

```
apps/frontend/src/app/features/class-groups/
├── weekly-schedule/
│   ├── weekly-schedule.component.ts
│   └── weekly-schedule.component.html
└── components/
    └── schedule-block/
        └── schedule-block.component.ts
```

### Previous Story Dependencies

- **Story 3.2** provides: Class groups list and data
- **Story 3.4** provides: Conflict detection logic to reuse

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 3.5]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
