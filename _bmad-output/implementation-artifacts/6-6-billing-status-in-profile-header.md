# Story 6.6: Billing Status in Profile Header

Status: ready-for-dev

## Story

As an **admin**,
I want **to see quick billing status in the profile header**,
so that **I immediately know if a student has outstanding balance**.

## Acceptance Criteria

1. **Given** I view a student profile
   **When** the header loads
   **Then** the third column shows:
   - Current balance with color indicator (green if 0, red if outstanding)
   - "Up to date" or "Balance due" text

2. **Given** the student has overdue invoices
   **When** I view the header
   **Then** a warning indicator is shown

## Tasks / Subtasks

- [ ] Task 1: Fetch billing summary with profile (AC: #1)
  - [ ] Include billing summary in student profile API response
  - [ ] Or fetch as separate call on profile load
- [ ] Task 2: Display in header column 3 (AC: #1)
  - [ ] Show balance amount
  - [ ] Green text/badge if R 0.00
  - [ ] Red text/badge if balance > 0
  - [ ] "Up to date" or "Balance due: R X"
- [ ] Task 3: Add overdue indicator (AC: #2)
  - [ ] Check for overdue invoices
  - [ ] Show warning icon or badge

## Dev Notes

### Header Column 3 Content

```html
<div class="stat">
  <div class="stat-title">Billing Status</div>
  <div class="stat-value" [ngClass]="{'text-success': balance() === 0, 'text-error': balance() > 0}">
    {{ balance() === 0 ? 'Up to date' : 'R ' + balance().toFixed(2) }}
  </div>
  @if (hasOverdue()) {
    <div class="stat-desc text-warning">
      <span class="badge badge-warning">Overdue</span>
    </div>
  }
</div>
```

### Frontend Architecture

- Angular 21 with Signals + RxJS
- Balance and overdue status are signals derived from billing summary API call
- Billing summary API backed by Dapper repository (Story 6.1)

### Previous Story Dependencies

- **Story 4.6** provides: Profile header layout
- **Story 6.1** provides: Billing summary API (Dapper-based backend)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.6]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
