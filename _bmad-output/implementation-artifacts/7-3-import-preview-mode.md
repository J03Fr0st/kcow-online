# Story 7.3: Import Preview Mode

Status: ready-for-dev

## Story

As a **developer**,
I want **to preview import results before committing**,
so that **I can verify the data before it goes live (FR12)**.

## Acceptance Criteria

1. **Given** I run the import command with `--preview` flag
   **When** the import runs
   **Then** it shows:
   - Total records to import by entity type
   - Sample of mapped records
   - Validation warnings and errors
   - Records that will be skipped

2. **And** NO data is written to the database

3. **And** I can review the preview output before running full import

## Tasks / Subtasks

- [ ] Task 1: Add --preview flag to CLI (AC: #1)
  - [ ] Parse --preview command line argument
  - [ ] Branch logic based on flag
- [ ] Task 2: Implement preview logic (AC: #1, #2)
  - [ ] Run parser and mapper
  - [ ] Collect statistics
  - [ ] Skip database writes when preview=true
- [ ] Task 3: Generate preview report (AC: #1)
  - [ ] Count total records per entity type
  - [ ] Show sample of first 5 records per type
  - [ ] List all warnings and errors
  - [ ] List records to be skipped with reasons
- [ ] Task 4: Output preview to console/file (AC: #3)
  - [ ] Pretty-print preview to console
  - [ ] Optionally save to JSON file

## Dev Notes

### CLI Command

```bash
# Preview mode (no database writes)
dotnet run --project apps/backend/src/Api import run --preview

# Full import (writes to database)
dotnet run --project apps/backend/src/Api import run
```

### Preview Output Format

```
=== IMPORT PREVIEW ===

📊 Record Counts:
  - Schools: 15
  - Class Groups: 42
  - Activities: 8
  - Students: 350

📝 Sample Records (Schools):
  1. Greenwood Primary - 123 Main St
  2. Hillside Academy - 456 Oak Ave
  ...

⚠️ Warnings (12 total):
  - Student #45: DateOfBirth missing, using null
  - Student #89: Invalid gender value "X", using null
  
❌ Errors (3 records will be skipped):
  - Student #123: FirstName is required (skipped)
  - ClassGroup #5: Invalid SchoolId reference (skipped)

📋 Summary:
  - Valid: 410 records
  - Warnings: 12 records
  - Skipped: 3 records
  
⚡ Run without --preview to import data.
```

### Previous Story Dependencies

- **Story 7.1** provides: Parser
- **Story 7.2** provides: Mapper

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.3]
- [Source: _bmad-output/planning-artifacts/prd.md#FR12]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
