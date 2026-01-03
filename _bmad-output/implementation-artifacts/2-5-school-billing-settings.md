# Story 2.5: School Billing Settings

Status: ready-for-dev

## Story

As an **admin**,
I want **to configure billing settings per school**,
so that **student billing calculations use the correct rates**.

## Acceptance Criteria

1. **Given** I am editing a school
   **When** I view the billing settings section
   **Then** I can configure:
   - Default rate per session
   - Billing cycle (monthly/termly)
   - Any school-specific billing notes

2. **Given** I save billing settings
   **When** the form submits
   **Then** the settings are persisted with the school record
   **And** I see a success confirmation

3. **And** this fulfills FR3 (manage school billing settings)

## Tasks / Subtasks

- [ ] Task 1: Enhance school form with billing section (AC: #1)
  - [ ] Add billing settings fields to school-form
  - [ ] Create dropdown for billing cycle
  - [ ] Add validation for rate (numeric, >= 0)
- [ ] Task 2: Update school service for billing (AC: #2)
  - [ ] Ensure billing settings serialize correctly
  - [ ] Handle partial updates
- [ ] Task 3: Add success feedback (AC: #2)
  - [ ] Show success notification on save
- [ ] Task 4: Document billing usage
  - [ ] Add notes about how billing settings will be used

## Dev Notes

### Billing Settings Fields

| Field | Type | Validation |
|-------|------|------------|
| defaultSessionRate | number | >= 0, required |
| billingCycle | select | 'Monthly' or 'Termly' |
| billingNotes | textarea | optional, max 1000 chars |

### Form Structure

```html
<!-- Billing Settings Section in school-form -->
<div class="divider">Billing Settings</div>

<app-form-field label="Default Session Rate">
  <input type="number" formControlName="defaultSessionRate" />
</app-form-field>

<app-form-field label="Billing Cycle">
  <select formControlName="billingCycle">
    <option value="Monthly">Monthly</option>
    <option value="Termly">Termly</option>
  </select>
</app-form-field>

<app-form-field label="Billing Notes">
  <textarea formControlName="billingNotes"></textarea>
</app-form-field>
```

### Previous Story Dependencies

- **Story 2.4** provides: Schools management UI and form

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 2.5]
- [Source: _bmad-output/planning-artifacts/prd.md#FR3]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
