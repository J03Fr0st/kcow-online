# Story 7.2: Data Mapping Service

Status: done

## Story

As a **developer**,
I want **a mapping service that transforms legacy data to domain entities**,
so that **parsed data can be loaded into the new database**.

## Acceptance Criteria

1. **Given** parsed legacy data
   **When** the mapping service runs
   **Then** legacy fields are mapped to new entity properties:
   - Children -> Student entity
   - Class Group -> ClassGroup entity
   - School -> School entity
   - Activity -> Activity entity

2. **And** data transformations are applied (date formats, enum conversions, etc.)

3. **And** missing or invalid values are flagged for review

4. **And** mapping rules are documented in code

## Tasks / Subtasks

- [x] Task 1: Create mapping interfaces (AC: #1)
  - [x] Define IDataMapper interface
  - [x] Create mapper for each entity type
- [x] Task 2: Implement School mapper (AC: #1, #2)
  - [x] Map LegacySchool -> School
  - [x] Handle optional fields
- [x] Task 3: Implement ClassGroup mapper (AC: #1, #2)
  - [x] Map LegacyClassGroup -> ClassGroup
  - [x] Convert day/time formats
- [x] Task 4: Implement Activity mapper (AC: #1, #2)
  - [x] Map LegacyActivity -> Activity
- [x] Task 5: Implement Student mapper (AC: #1, #2)
  - [x] Map LegacyChild -> Student
  - [x] Convert date formats
  - [x] Handle family relationships
- [x] Task 6: Add validation and flagging (AC: #3)
  - [x] Validate required fields
  - [x] Flag missing or invalid values
  - [x] Return mapping result with warnings
- [x] Task 7: Document mapping rules (AC: #4)
  - [x] Add XML comments to mappers
  - [x] Create mapping documentation

## Dev Notes

### Architecture Note

This story is purely about data transformation. Mappers convert parsed legacy models (from Story 7.1) into domain entities. No direct database access is needed -- mappers produce domain entity instances that will be persisted by Story 7.4 using existing Dapper repositories.

### Mapping Interface

```csharp
public interface IDataMapper<TSource, TTarget>
{
    MappingResult<TTarget> Map(TSource source);
    MappingResult<List<TTarget>> MapMany(IEnumerable<TSource> sources);
}

public class MappingResult<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public List<MappingWarning> Warnings { get; set; } = new();
    public List<MappingError> Errors { get; set; } = new();
}

public class MappingWarning
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }
    public string? MappedValue { get; set; }
}
```

### Field Mapping Examples

| Legacy Field | Domain Field | Transformation |
|--------------|--------------|----------------|
| Child.FirstName | Student.FirstName | Direct copy |
| Child.DateOfBirth | Student.DateOfBirth | Parse date format |
| Child.Gender | Student.Gender | Normalize "M"/"F" to enum |
| ClassGroup.DayOfWeek | ClassGroup.DayOfWeek | Convert string to int |

### File Structure

```
apps/backend/src/
├── Application/
│   └── Import/
│       ├── Mappers/
│       │   ├── IDataMapper.cs
│       │   ├── SchoolMapper.cs
│       │   ├── ClassGroupMapper.cs
│       │   ├── ActivityMapper.cs
│       │   └── StudentMapper.cs
│       └── MappingResult.cs
```

### Existing Import Infrastructure

The `Application/Import/` folder may already contain parsers and mappers from earlier stories (2-6, 3-6, 4-9, 8-3). Reuse existing patterns and interfaces where applicable.

### Previous Story Dependencies

- **Story 7.1** provides: Parsed legacy data (LegacySchool, LegacyChild, etc.)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.2]
- [Source: docs/domain-models.md]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- ✅ Task 1: Created `IDataMapper<TSource, TTarget>` generic interface with `Map` and `MapMany` methods. Created `MappingResult<T>`, `MappingWarning`, and `MappingError` types with factory methods (`Ok`, `Fail`, `Skipped`). 8 unit tests.
- ✅ Task 2: Implemented `SchoolDataMapper` mapping `LegacySchoolRecord` → `School`. Handles whitespace trimming, TruckId FK validation, Price double→decimal conversion. 7 unit tests.
- ✅ Task 3: Implemented `ClassGroupDataMapper` mapping `LegacyClassGroupRecord` → `ClassGroup`. Handles DayId→DayOfWeek conversion, time parsing, Import flag skip, SchoolId/TruckId validation. 12 unit tests.
- ✅ Task 4: Implemented `ActivityDataMapper` mapping `LegacyActivityRecord` → `Activity`. Handles field truncation (255 char limit), large icon warnings, OLE wrapper detection. 6 unit tests.
- ✅ Task 5: Implemented `StudentDataMapper` mapping `LegacyChildRecord` → `StudentMappingData` (Student + FamilyInfo). Handles date parsing (4 formats + fallback), school/classgroup ID lookup, decimal charge parsing, family info extraction. 11 unit tests.
- ✅ Task 6: All mappers validate required fields and flag missing/invalid values via `MappingWarning` (non-fatal) and `MappingError` (fatal). Warnings include original values for review. 8 dedicated validation tests.
- ✅ Task 7: All mapper classes have XML summary documentation describing mapping rules, field transformations, and special handling. `MappingResult`, `MappingWarning`, `MappingError` classes documented.
- Note: Existing mappers in `Application/Import/` (from earlier epics) are preserved and unchanged. New mappers in `Application/Import/Mappers/` implement the unified `IDataMapper` interface.
- Pre-existing test failures: 13 tests in `FamilyServiceTests` fail due to Dapper async mock issues (unrelated to this story).

### File List

- apps/backend/src/Application/Import/Mappers/IDataMapper.cs (new)
- apps/backend/src/Application/Import/Mappers/MappingResult.cs (new)
- apps/backend/src/Application/Import/Mappers/SchoolDataMapper.cs (new)
- apps/backend/src/Application/Import/Mappers/ClassGroupDataMapper.cs (new)
- apps/backend/src/Application/Import/Mappers/ActivityDataMapper.cs (new)
- apps/backend/src/Application/Import/Mappers/StudentDataMapper.cs (new)
- apps/backend/tests/Unit/Import/DataMapperInterfaceTests.cs (new)
- apps/backend/tests/Unit/Import/SchoolDataMapperTests.cs (new)
- apps/backend/tests/Unit/Import/ClassGroupDataMapperTests.cs (new)
- apps/backend/tests/Unit/Import/ActivityDataMapperTests.cs (new)
- apps/backend/tests/Unit/Import/StudentDataMapperTests.cs (new)
- apps/backend/tests/Unit/Import/MappingValidationTests.cs (new)

### Change Log

- 2026-02-13: Story 7.2 implemented - Created unified IDataMapper interface and 4 entity mappers with 52 unit tests covering all ACs.

## Senior Developer Review (AI)

**Reviewer:** Joe (AI-assisted) on 2026-02-13
**Outcome:** Approved with fixes applied

**Issues Found and Fixed (Epic-wide review):**
- [CRITICAL] Missing DI registrations for ILegacyParser and IImportExecutionService in DependencyInjection.cs — Fixed
- [CRITICAL] 37+ Student fields missing from INSERT/UPDATE SQL in ImportExecutionService — Fixed (all XSD fields now mapped)
- [CRITICAL] ImportAuditLogController returned entities directly — Fixed (created ImportAuditLogDto, added count validation)
- [CRITICAL] ClassGroupDataMapper.MapMany always reported Success=true — Fixed (now checks allErrors.Count == 0)
- [CRITICAL] LegacyParser directly instantiated concrete parsers — Fixed (added constructor injection)
- [HIGH] SQL injection risk via table name interpolation in FindByLegacyIdAsync — Fixed (added whitelist validation)
- [HIGH] ImportAuditLogRepository used blocking Create() and ignored CancellationToken — Fixed (uses CreateAsync + CommandDefinition)
- [HIGH] Missing UNIQUE constraint on legacy_id indexes — Fixed (partial unique indexes)
- [HIGH] ImportExceptionWriter had no null checks or path validation — Fixed (added ArgumentNullException, path canonicalization, CancellationToken)
- [HIGH] School INSERT/UPDATE missing school_description, phone, scheduling_notes, omsendbriewe columns — Fixed
- [MEDIUM] ImportHistoryCommand had no error handling for repository calls — Fixed (try-catch added)
- [MEDIUM] ImportHistoryCommand accepted negative count values — Fixed (validation added)

**Test Results:** 86 unit tests PASS, 126 integration tests PASS (2 pre-existing failures unrelated to Epic 7)
