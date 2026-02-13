# Story 7.1: Legacy Schema Parser

Status: done

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

- [x] Task 1: Analyze legacy schemas (AC: #1)
  - [x] Review docs/legacy/1_School/School.xsd
  - [x] Review docs/legacy/2_Class_Group/Class Group.xsd
  - [x] Review docs/legacy/3_Activity/Activity.xsd
  - [x] Review docs/legacy/4_Children/Children.xsd
  - [x] Document field mappings
- [x] Task 2: Create parser project/module (AC: #3)
  - [x] Create Import feature in Application layer
  - [x] Add CLI command support in Api
- [x] Task 3: Implement XML parsing (AC: #1)
  - [x] Parse School.xml
  - [x] Parse Class Group.xml
  - [x] Parse Activity.xml
  - [x] Parse Children.xml
- [x] Task 4: Implement XSD validation (AC: #1)
  - [x] Validate XML against XSD
  - [x] Handle validation errors gracefully
- [x] Task 5: Handle encoding issues (AC: #1)
  - [x] Detect encoding variations
  - [x] Handle special characters
  - [x] Normalize data formats
- [x] Task 6: Add error logging (AC: #2)
  - [x] Log parser errors with file and line info
  - [x] Create error report

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

GLM-5

### Debug Log References

N/A

### Completion Notes List

**Implementation Summary:**

This story leveraged existing XML parsers (`LegacySchoolXmlParser`, `LegacyClassGroupXmlParser`, `LegacyActivityXmlParser`, `LegacyChildXmlParser`) that were already implemented in previous stories. The following new components were added to fulfill the remaining requirements:

1. **Unified Parser Interface** (`ILegacyParser.cs`):
   - Created `ILegacyParser` interface with methods for parsing all entity types
   - Created generic `ParseResult<T>` class with Records, Errors, and HasErrors properties
   - Created `ParseError` class with File, Line, and Message properties for error tracking

2. **Unified Parser Implementation** (`LegacyParser.cs`):
   - Implements `ILegacyParser` interface
   - Wraps existing entity-specific parsers
   - Converts validation errors to `ParseError` format with file/line information

3. **CLI Command Support** (`ImportParseCommand.cs`):
   - Added `import parse` command handler
   - Supports `--input`/`-i` option for specifying legacy directory
   - Supports `--output`/`-o` option for JSON report output
   - Supports `--verbose`/`-v` for detailed output
   - Generates summary report with record counts and errors

4. **Program.cs Integration**:
   - Added CLI command detection before web host startup
   - Routes to `ImportParseCommand` when `import parse` arguments detected

5. **Tests**:
   - `LegacyParserTests.cs` - 9 unit tests for unified parser
   - `ImportParseCommandTests.cs` - 11 integration tests for CLI handler

**All ACs satisfied:**
- AC#1: XML/XSD parsing with validation for all 4 entity types ✓
- AC#2: Errors logged with file/line information via `ParseError` ✓
- AC#3: CLI command `dotnet run import parse --input docs/legacy` ✓

### File List

**New Files:**
- apps/backend/src/Application/Import/ILegacyParser.cs
- apps/backend/src/Application/Import/LegacyParser.cs
- apps/backend/src/Api/CliCommands/ImportParseCommand.cs
- apps/backend/tests/Unit/Import/LegacyParserTests.cs
- apps/backend/tests/Integration/Import/ImportParseCommandTests.cs

**Modified Files:**
- apps/backend/src/Api/Program.cs

**Existing Files (Verified Working):**
- apps/backend/src/Application/Import/LegacySchoolXmlParser.cs
- apps/backend/src/Application/Import/LegacyClassGroupXmlParser.cs
- apps/backend/src/Application/Import/LegacyActivityXmlParser.cs
- apps/backend/src/Application/Import/LegacyChildXmlParser.cs

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
