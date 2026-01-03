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
  - [ ] Fetch evaluation history
  - [ ] Display table with Activity, Date, Score, Speed, Accuracy
- [ ] Task 2: Add score indicators (AC: #1)
  - [ ] Visual representation of scores
  - [ ] Color-coded or progress bar
- [ ] Task 3: Implement add flow (AC: #2)
  - [ ] Activity dropdown (load activities)
  - [ ] Date picker
  - [ ] Score, Speed, Accuracy inputs
  - [ ] Save and refresh
- [ ] Task 4: Implement inline edit (AC: #3)
  - [ ] Click row to edit
  - [ ] Update and save

## Dev Notes

### Evaluation Tab Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Evaluations                                [Add Evaluation]  │
├─────────────────────────────────────────────────────────────┤
│ Activity    │ Date       │ Score │ Speed │ Accuracy │ Notes │
│ Reading L1  │ 2026-01-02 │ 85    │ 45wpm │ 92%      │ Good  │
│ Math Basics │ 2026-01-01 │ 78    │ -     │ 88%      │       │
└─────────────────────────────────────────────────────────────┘
```

### Previous Story Dependencies

- **Story 4.6** provides: Profile tabs
- **Story 5.4** provides: Evaluation API

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.5]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
