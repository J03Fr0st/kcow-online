# Story 7.6: Import Re-run Capability

Status: ready-for-dev

## Story

As a **developer**,
I want **to re-run the import to update or fix records**,
so that **I can iterate on data quality**.

## Acceptance Criteria

1. **Given** data has been previously imported
   **When** I run the import again
   **Then** existing records are matched by a unique key (e.g., legacy ID)
   **And** I can choose: skip existing, update existing, or fail on conflict

2. **Given** I run with `--update` flag
   **When** matching records exist
   **Then** they are updated with new values
   **And** changes are logged to the audit trail

## Tasks / Subtasks

- [ ] Task 1: Add legacy ID tracking (AC: #1)
  - [ ] Add LegacyId column to entities (nullable)
  - [ ] Apply migration
  - [ ] Store legacy ID during import
- [ ] Task 2: Implement conflict detection (AC: #1)
  - [ ] Check for existing record by LegacyId
  - [ ] Return conflict status
- [ ] Task 3: Add conflict resolution flags (AC: #1)
  - [ ] --skip-existing: Skip records that exist
  - [ ] --update: Update existing records
  - [ ] --fail-on-conflict: Error if exists (default)
- [ ] Task 4: Implement update logic (AC: #2)
  - [ ] Update existing records with new data
  - [ ] Track what changed
- [ ] Task 5: Log updates to audit trail (AC: #2)
  - [ ] Use existing audit service
  - [ ] Log old/new values for updated fields

## Dev Notes

### CLI Flags

```bash
# Skip existing records
dotnet run import run --skip-existing

# Update existing records
dotnet run import run --update

# Fail on conflict (default)
dotnet run import run --fail-on-conflict
```

### Legacy ID Tracking

Add `LegacyId` to each entity:
```csharp
public class Student
{
    // ... existing properties ...
    
    /// <summary>
    /// Original ID from legacy Access database import.
    /// Used for re-import matching and audit trail.
    /// </summary>
    public string? LegacyId { get; set; }
}
```

### Re-import Summary

```
=== RE-IMPORT COMPLETE ===

📊 Results:
  - New records: 5
  - Updated: 12
  - Skipped (unchanged): 393
  - Failed: 0

📝 Updates logged to audit trail.
```

### Previous Story Dependencies

- **Story 7.4** provides: Basic import execution
- **Story 7.5** provides: Audit logging
- **Story 5.3** provides: Audit trail service (reuse for update logging)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.6]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
