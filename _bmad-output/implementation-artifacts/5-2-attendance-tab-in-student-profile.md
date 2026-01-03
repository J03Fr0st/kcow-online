# Story 5.2: Attendance Tab in Student Profile

Status: ready-for-dev

## Story

As an **admin**,
I want **to view and manage attendance in the student profile**,
so that **I can track and update attendance without leaving the profile (FR10)**.

## Acceptance Criteria

1. **Given** I am on the student profile
   **When** I select the "Attendance" tab
   **Then** I see a list of recent attendance records with Date, Class Group, Status, Notes columns
   **And** status is shown with color-coded chips (Present=green, Absent=red, Late=yellow)

2. **Given** I click on an attendance row
   **When** inline edit mode activates
   **Then** I can change the status or add notes
   **And** save the change immediately

3. **Given** I click "Add Attendance"
   **When** the inline form appears
   **Then** I can select date, class group, and status
   **And** the new record appears in the list

## Tasks / Subtasks

- [ ] Task 1: Create AttendanceTab component (AC: #1)
  - [ ] Replace placeholder in student profile
  - [ ] Fetch attendance history for student
  - [ ] Display in table with Date, Class Group, Status, Notes
- [ ] Task 2: Add status chips (AC: #1)
  - [ ] Create StatusChip component
  - [ ] Color-code: Present=green, Absent=red, Late=yellow
- [ ] Task 3: Implement inline edit (AC: #2)
  - [ ] Click row to enter edit mode
  - [ ] Editable status dropdown and notes
  - [ ] Save on blur or enter
- [ ] Task 4: Implement add flow (AC: #3)
  - [ ] "Add Attendance" button
  - [ ] Inline form with date, class group, status
  - [ ] Save and append to list

## Dev Notes

### Attendance Tab Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Attendance History                        [Add Attendance]  │
├─────────────────────────────────────────────────────────────┤
│ Date       │ Class Group │ Status      │ Notes             │
│ 2026-01-02 │ Class 5A    │ [Present ✓] │ On time           │
│ 2026-01-01 │ Class 5A    │ [Late ⚠]    │ Arrived 10min late│
│ 2025-12-31 │ Class 5A    │ [Absent ✗]  │ Sick              │
└─────────────────────────────────────────────────────────────┘
```

### Status Chip Component

```typescript
@Component({
  selector: 'app-status-chip',
  template: `
    <span class="badge" [ngClass]="statusClass">
      {{ status }}
    </span>
  `
})
export class StatusChipComponent {
  @Input() status!: AttendanceStatus;
  
  get statusClass() {
    return {
      'badge-success': this.status === 'Present',
      'badge-error': this.status === 'Absent',
      'badge-warning': this.status === 'Late',
    };
  }
}
```

### Previous Story Dependencies

- **Story 4.6** provides: Profile tabs
- **Story 5.1** provides: Attendance API

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.2]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Status Chips]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
