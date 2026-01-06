# Story 3.5: Weekly Schedule Overview

Status: review

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

- [x] Task 1: Create WeeklyScheduleView component (AC: #1)
  - [x] Create weekly-schedule component
  - [x] Render grid with Monday-Friday columns
  - [x] Render time slot rows (7am-5pm in 30min slots)
  - [x] Position class groups as blocks based on time
- [x] Task 2: Load and display class groups (AC: #1)
  - [x] Fetch all class groups for the week
  - [x] Group by day and calculate grid positions
  - [x] Color-code by truck for visual distinction
- [x] Task 3: Add block click interaction (AC: #2)
  - [x] Show tooltip with class group details
  - [x] Navigate to edit on click
- [x] Task 4: Highlight conflicts (AC: #3)
  - [x] Detect overlapping blocks for same truck
  - [x] Add warning border to conflicting blocks
- [x] Task 5: Add tab navigation
  - [x] Add tabs: List View / Weekly View
  - [x] View mode state tracked in component

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

Claude Opus 4.5

### Debug Log References

### Completion Notes List

Frontend:
- Created ScheduleBlockComponent with dynamic truck-based coloring
- Created WeeklyScheduleComponent with CSS Grid layout
- Grid configuration: 7am-5pm in 30-minute slots, Monday-Friday columns
- Block position calculation based on time and day
- Conflict detection for overlapping same-truck assignments
- Added tab navigation to ClassGroupsListComponent (List View / Weekly View)
- Filters shown only in List View
- Weekly view shows all class groups as blocks on the schedule
- Color coding by truck using HSL color generation
- Conflict blocks highlighted with warning border
- Click on block navigates to edit page
- Tooltip shows class group details on hover
- All acceptance criteria met

### File List
