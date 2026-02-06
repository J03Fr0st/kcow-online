# Story 7.1: Legacy Schema Parser

Status: ready-for-dev

## Story

As a **developer**,
I want **a parser that reads legacy XML/XSD files**,
so that **I can extract data from the Access export format**.

## Acceptance Criteria

1. **Given** the legacy XML and XSD files in `docs/legacy/`
   **When** the parser runs
   **Then** it reads and validates XML against the XSD schema
   **And** extracts School, ClassGroup (Class Group), Activity, and Children (Student) data
   **And** handles encoding and format variations in the legacy data

2. **And** parser errors are logged with file/line information

3. **And** the parser can be run from CLI (`dotnet run import parse`)

## Tasks / Subtasks

- [ ] Task 1: Analyze legacy schemas (AC: #1)
  - [ ] Review docs/legacy/1_School/School.xsd
  - [ ] Review docs/legacy/2_Class_Group/Class Group.xsd
  - [ ] Review docs/legacy/3_Activity/Activity.xsd
  - [ ] Review docs/legacy/4_Children/Children.xsd
  - [ ] Document field mappings
- [ ] Task 2: Create parser project/module (AC: #3)
  - [ ] Create Import feature in Application layer
  - [ ] Add CLI command support in Api
- [ ] Task 3: Implement XML parsing (AC: #1)
  - [ ] Parse School.xml
  - [ ] Parse Class Group.xml
  - [ ] Parse Activity.xml
  - [ ] Parse Children.xml
- [ ] Task 4: Implement XSD validation (AC: #1)
  - [ ] Validate XML against XSD
  - [ ] Handle validation errors gracefully
- [ ] Task 5: Handle encoding issues (AC: #1)
  - [ ] Detect encoding variations
  - [ ] Handle special characters
  - [ ] Normalize data formats
- [ ] Task 6: Add error logging (AC: #2)
  - [ ] Log parser errors with file and line info
  - [ ] Create error report

## Dev Notes

### Architecture Note

This story is purely about XML parsing and validation. The parser outputs parsed legacy models (`LegacySchool`, `LegacyClassGroup`, etc.) but does NOT write to the database. No repository or database interaction is needed for this story.

### Legacy File Locations

| Entity | XSD Schema | XML Data |
|--------|------------|----------|
| School | docs/legacy/1_School/School.xsd | docs/legacy/1_School/School.xml |
| Class Group | docs/legacy/2_Class_Group/Class Group.xsd | docs/legacy/2_Class_Group/Class Group.xml |
| Activity | docs/legacy/3_Activity/Activity.xsd | docs/legacy/3_Activity/Activity.xml |
| Children | docs/legacy/4_Children/Children.xsd | docs/legacy/4_Children/Children.xml |

### Parser Interface

```csharp
public interface ILegacyParser
{
    ParseResult<LegacySchool> ParseSchools(string xmlPath, string xsdPath);
    ParseResult<LegacyClassGroup> ParseClassGroups(string xmlPath, string xsdPath);
    ParseResult<LegacyActivity> ParseActivities(string xmlPath, string xsdPath);
    ParseResult<LegacyChild> ParseChildren(string xmlPath, string xsdPath);
}

public class ParseResult<T>
{
    public List<T> Records { get; set; } = new();
    public List<ParseError> Errors { get; set; } = new();
    public bool HasErrors => Errors.Any();
}

public class ParseError
{
    public string File { get; set; } = string.Empty;
    public int? Line { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

### CLI Command

```bash
dotnet run --project apps/backend/src/Api import parse --input docs/legacy
```

### File Structure

```
apps/backend/src/
├── Application/
│   └── Import/
│       ├── ILegacyParser.cs
│       ├── LegacyModels/
│       │   ├── LegacySchool.cs
│       │   ├── LegacyClassGroup.cs
│       │   ├── LegacyActivity.cs
│       │   └── LegacyChild.cs
│       └── ParseResult.cs
└── Infrastructure/
    └── Import/
        └── XmlLegacyParser.cs
```

### Existing Import Infrastructure

The `Application/Import/` folder may already contain parsers and mappers from earlier stories (2-6, 3-6, 4-9, 8-3). Reuse existing patterns where applicable.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.1]
- [Source: _bmad-output/planning-artifacts/architecture.md#Legacy Import]
- [Source: docs/legacy/*] - All legacy schema files

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

