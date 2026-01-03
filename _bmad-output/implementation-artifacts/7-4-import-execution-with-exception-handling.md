# Story 7.4: Import Execution with Exception Handling

Status: ready-for-dev

## Story

As a **developer**,
I want **to run the full import with proper error handling**,
so that **valid records are imported and failures are tracked**.

## Acceptance Criteria

1. **Given** I run the import command without `--preview`
   **When** the import executes
   **Then** valid records are inserted into the database
   **And** failed records are logged to an exceptions file (JSON or CSV)
   **And** each exception includes: record ID, field, error reason, original value

2. **Given** some records fail validation
   **When** the import completes
   **Then** a summary shows: X imported, Y failed, Z skipped
   **And** the exceptions file is saved for review

## Tasks / Subtasks

- [ ] Task 1: Implement import execution (AC: #1)
  - [ ] Run parser and mapper
  - [ ] Begin database transaction
  - [ ] Insert valid records
  - [ ] Commit transaction
- [ ] Task 2: Handle record failures (AC: #1)
  - [ ] Catch validation/insert errors per record
  - [ ] Continue processing other records
  - [ ] Track failed records
- [ ] Task 3: Create exceptions file (AC: #1, #2)
  - [ ] Define ImportException model
  - [ ] Write exceptions to JSON file
  - [ ] Include: recordId, field, reason, originalValue
- [ ] Task 4: Generate summary (AC: #2)
  - [ ] Count imported, failed, skipped
  - [ ] Print summary to console
  - [ ] Report exceptions file location

## Dev Notes

### Exception File Format

```json
{
  "importRun": {
    "timestamp": "2026-01-03T19:00:00Z",
    "inputPath": "docs/legacy"
  },
  "exceptions": [
    {
      "entityType": "Student",
      "legacyId": "123",
      "field": "FirstName",
      "reason": "Required field is empty",
      "originalValue": ""
    },
    {
      "entityType": "ClassGroup",
      "legacyId": "5",
      "field": "SchoolId",
      "reason": "Referenced school not found",
      "originalValue": "999"
    }
  ]
}
```

### Import Summary Output

```
=== IMPORT COMPLETE ===

✅ Imported:
  - Schools: 15
  - Class Groups: 40
  - Activities: 8
  - Students: 347

❌ Failed:
  - Students: 3

📄 Exceptions saved to: import-exceptions-2026-01-03.json

Total: 410 processed, 3 failed (99.3% success rate)
```

### Transaction Strategy

- Use transaction per entity type
- Or atomic per batch (100 records)
- Rollback batch on critical error, log and continue

### Previous Story Dependencies

- **Story 7.3** provides: Preview mode logic to reuse

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.4]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
