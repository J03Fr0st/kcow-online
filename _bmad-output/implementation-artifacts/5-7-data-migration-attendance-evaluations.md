# Story 5.7: Data Migration - Attendance & Evaluations

Status: ready-for-dev

## Story

As a developer,
I want legacy attendance and evaluation data parsed, mapped, and imported into the database,
So that tracking flows and audit trails operate on real historical records.

## Acceptance Criteria

1. **Given** legacy XML/XSD data for Activities (`docs/legacy/3_Activity/`)
   **When** the migration import executes for Attendance and Evaluations
   **Then** all valid attendance records are inserted with correct student and class group links

2. **Given** the migration imports evaluation data
   **Then** evaluation/assessment records are imported with proper date and score mappings

3. **Given** the migration processes historical data
   **Then** historical data maintains accurate timestamps for audit trail purposes

4. **Given** validation errors occur during import
   **Then** errors are captured and logged to the migration audit log

5. **Given** the import completes
   **Then** a summary report shows imported count, skipped count, and error count

6. **Given** the migration is complete
   **Then** the imported data appears correctly in the attendance tab and evaluation tab

## Tasks / Subtasks

- [ ] Task 1: Create Legacy Schema Parser for Activities (AC: #1, #2)
  - [ ] Read and validate Activity.xml against Activity.xsd schema
  - [ ] Extract all 7 fields from Activity XSD
  - [ ] Parse attendance and evaluation records
  - [ ] Handle date/time format variations

- [ ] Task 2: Implement Attendance Record Import (AC: #1)
  - [ ] Map legacy attendance data to Attendance entity
  - [ ] Link StudentId and ClassGroupId from imported data
  - [ ] Map status values (Present, Absent, Late)
  - [ ] Handle orphaned attendance (student/class not found)

- [ ] Task 3: Implement Evaluation Record Import (AC: #2)
  - [ ] Map legacy evaluation/assessment data to Evaluation entity
  - [ ] Link StudentId and ActivityId
  - [ ] Map score, speed, and accuracy metrics
  - [ ] Translate field names from Afrikaans to English

- [ ] Task 4: Preserve Historical Timestamps (AC: #3)
  - [ ] Map original CreatedAt timestamps from legacy data
  - [ ] Preserve ModifiedAt dates for audit accuracy
  - [ ] Ensure audit trail reflects actual historical dates

- [ ] Task 5: Implement Validation and Error Logging (AC: #4)
  - [ ] Validate imported records against XSD constraints
  - [ ] Create audit log entries for validation errors
  - [ ] Include file/line information in error logs

- [ ] Task 6: Create Import Summary Report (AC: #5)
  - [ ] Track imported, skipped, and error counts
  - [ ] Separate counts for Attendance and Evaluations
  - [ ] Include association errors in report

- [ ] Task 7: Verify Tab UI Display (AC: #6)
  - [ ] Test that imported Attendance appears in student profile Attendance tab
  - [ ] Test that imported Evaluations appear in Evaluation tab
  - [ ] Test status chips display correctly
  - [ ] Test audit trail shows historical data

## Dev Notes

### Architecture Requirements
- **Legacy XSD Location**: `docs/legacy/3_Activity/Activity.xsd` (7 fields)
- **Entity Locations**: 
  - `apps/backend/src/Domain/Entities/Attendance.cs`
  - `apps/backend/src/Domain/Entities/Evaluation.cs`
  - `apps/backend/src/Domain/Entities/Activity.cs`
- **Import Service Location**: `apps/backend/src/Application/Import/`
- **CLI Command**: `dotnet run import attendance-evaluations`

### Field Mapping Reference
See `docs/domain-models.md` for complete Afrikaans → English translations:
- Activity entity: 7 fields from Activity.xsd
- Attendance record extraction from activity data
- Evaluation/score mapping

### Dependency Requirements
- Story 4-9 (Students migration) must be completed first
- Story 3-6 (Class Groups migration) must be completed first
- Students and Class Groups must exist before importing attendance

### Import Order
1. Schools (Story 2-6)
2. Class Groups (Story 3-6)
3. Students & Families (Story 4-9)
4. Activities, Attendance & Evaluations (this story)

### Timestamp Preservation
Critical for audit compliance (FR14):
- Original dates must be preserved
- Do not overwrite with import timestamps
- Audit trail must reflect actual historical activity

### Previous Story Context
- Story 5-6 completed Bulk Attendance Entry
- Attendance and Evaluation tabs are functional
- Audit Trail Panel is implemented

### Testing Standards
- Integration tests in `apps/backend/tests/Integration/Import/`
- Use sample XML files from `docs/legacy/3_Activity/`
- Test timestamp preservation
- Test audit trail accuracy

### Project Structure Notes
- Reuse import infrastructure from previous migration stories
- Extend ImportAuditLog for attendance/evaluation imports

### References
- [Source: docs/legacy/3_Activity/Activity.xsd]
- [Source: docs/domain-models.md]
- [Source: _bmad-output/planning-artifacts/architecture.md#Data-Architecture]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
