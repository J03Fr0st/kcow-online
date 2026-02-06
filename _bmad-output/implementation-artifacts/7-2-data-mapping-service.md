# Story 7.2: Data Mapping Service

Status: ready-for-dev

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

- [ ] Task 1: Create mapping interfaces (AC: #1)
  - [ ] Define IDataMapper interface
  - [ ] Create mapper for each entity type
- [ ] Task 2: Implement School mapper (AC: #1, #2)
  - [ ] Map LegacySchool -> School
  - [ ] Handle optional fields
- [ ] Task 3: Implement ClassGroup mapper (AC: #1, #2)
  - [ ] Map LegacyClassGroup -> ClassGroup
  - [ ] Convert day/time formats
- [ ] Task 4: Implement Activity mapper (AC: #1, #2)
  - [ ] Map LegacyActivity -> Activity
- [ ] Task 5: Implement Student mapper (AC: #1, #2)
  - [ ] Map LegacyChild -> Student
  - [ ] Convert date formats
  - [ ] Handle family relationships
- [ ] Task 6: Add validation and flagging (AC: #3)
  - [ ] Validate required fields
  - [ ] Flag missing or invalid values
  - [ ] Return mapping result with warnings
- [ ] Task 7: Document mapping rules (AC: #4)
  - [ ] Add XML comments to mappers
  - [ ] Create mapping documentation

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

