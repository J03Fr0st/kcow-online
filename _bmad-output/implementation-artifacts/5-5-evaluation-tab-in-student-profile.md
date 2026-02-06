# Story 5.5: Evaluation Tab in Student Profile

Status: ready-for-dev

## Story

As an **admin**,
I want **to view and record evaluations in the student profile**,
so that **I can track progress without leaving the profile**.

## Acceptance Criteria

1. **Given** I am on the student profile
   **When** I select the "Evaluation" tab
   **Then** I see a list of evaluations with Activity, Date, Score, Speed, Accuracy columns
   **And** scores are shown with visual indicators (status chips)

2. **Given** I click "Add Evaluation"
   **When** the inline form appears
   **Then** I can select an activity, enter date, scores, and notes
   **And** the new evaluation appears in the list

3. **Given** I click on an evaluation row
   **When** inline edit mode activates
   **Then** I can update scores and notes

## Tasks / Subtasks

- [ ] Task 1: Create EvaluationTab component (AC: #1)
  - [ ] Replace placeholder in student profile
  - [ ] Fetch evaluation history using Angular service calling Evaluation API
  - [ ] Display table with Activity, Date, Score, Speed, Accuracy
  - [ ] Use Angular Signals for component state management
- [ ] Task 2: Add score indicators (AC: #1)
  - [ ] Visual representation of scores using status chips
  - [ ] Color-coded or progress bar indicators
  - [ ] Use computed Signals for derived display values
- [ ] Task 3: Implement add flow (AC: #2)
  - [ ] Activity dropdown (load activities from API)
  - [ ] Date picker
  - [ ] Score, Speed, Accuracy inputs with validation
  - [ ] Save via Evaluation API and refresh list using Signal updates
- [ ] Task 4: Implement inline edit (AC: #3)
  - [ ] Click row to edit with Signal-driven edit state
  - [ ] Update and save via Evaluation API

## Dev Notes

### Evaluation Tab Layout

```
+-------------------------------------------------------------+
| Evaluations                                [Add Evaluation]  |
+-------------------------------------------------------------+
| Activity    | Date       | Score | Speed | Accuracy | Notes  |
| Reading L1  | 2026-01-02 | 85    | 45wpm | 92%      | Good   |
| Math Basics | 2026-01-01 | 78    | -     | 88%      |        |
+-------------------------------------------------------------+
```

### Frontend Architecture (Angular 21)

- **State management**: Use Angular Signals for local component state
- **API calls**: Use RxJS-based services with `toSignal()` for template binding
- **Component pattern**: Standalone component with OnPush change detection
- **Forms**: Reactive forms with signal-based validation state

### Previous Story Dependencies

- **Story 4.6** provides: Profile tabs infrastructure
- **Story 5.4** provides: Evaluation API (Dapper-based backend with `IEvaluationRepository`)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.5]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
