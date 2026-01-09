# Story 8.3: Data Migration - Activities

Status: ready-for-dev

## Story

As a developer,
I want legacy Activity data parsed, mapped, and imported into the database,
so that the activities catalog is populated with existing programs.

## Acceptance Criteria

1. **Given** legacy XML/XSD data for Activities (docs/legacy/3_Activity/), **When** the migration import executes for Activities, **Then** all valid Activity records are inserted into the database
2. **And** field mappings transform legacy data to new schema:
   - ActivityID → Id
   - Program → Code
   - ProgramName → Name
   - Educational_Focus → Description
   - Folder → Folder
   - Grade → GradeLevel
   - Icon (OleObject/base64Binary) → Icon (base64 string)
3. **And** the Icon OLE object data is extracted and stored as base64 string
4. **And** validation errors are captured and logged to the migration audit log
5. **And** a summary report shows imported count, skipped count, and error count
6. **And** the imported data is available in the UI and API

## Tasks / Subtasks

- [ ] Task 1: Analyze Legacy Activity XML Structure (AC: #1, #2)
  - [ ] Review `docs/legacy/3_Activity/Activity.xml` structure
  - [ ] Confirm field names match XSD (ActivityID, Program, ProgramName, etc.)
  - [ ] Understand Icon OLE object encoding

- [ ] Task 2: Create Activity Import Model (AC: #2)
  - [ ] Create `ActivityXmlRecord` model matching XML structure
  - [ ] Handle XML element names including `Educational_x0020_Focus` encoding

- [ ] Task 3: Create Activity Import Runner (AC: #1, #2, #3)
  - [ ] Create `ActivityImportRunner.cs` in `tools/DataMigration/`
  - [ ] Follow ClassGroupImportRunner pattern
  - [ ] Parse XML using XDocument or XmlSerializer
  - [ ] Map ActivityID to Id (auto-generate if needed for new records)
  - [ ] Map Program → Code
  - [ ] Map ProgramName → Name
  - [ ] Map Educational_Focus → Description
  - [ ] Map Folder → Folder
  - [ ] Map Grade → GradeLevel
  - [ ] Map Icon base64 data directly

- [ ] Task 4: Handle Icon OLE Object Data (AC: #3)
  - [ ] Parse base64Binary Icon data from XML
  - [ ] Store as base64 string in Icon field
  - [ ] Handle missing/null Icon gracefully
  - [ ] Log large Icon sizes for awareness

- [ ] Task 5: Implement Validation and Error Handling (AC: #4, #5)
  - [ ] Validate field lengths (255 char max for Code, Name, Folder, GradeLevel)
  - [ ] Capture validation errors per record
  - [ ] Log errors to migration audit log
  - [ ] Continue import on individual record errors

- [ ] Task 6: Implement Import Summary Report (AC: #5)
  - [ ] Track imported count
  - [ ] Track skipped count (validation failures)
  - [ ] Track error count
  - [ ] Output summary to console and log file

- [ ] Task 7: Create CLI Command for Activity Import (AC: #1)
  - [ ] Add `import activities` command to DataMigration tool
  - [ ] Support `--preview` flag for dry run
  - [ ] Support `--source` flag for XML file path

- [ ] Task 8: Test Import with Real Data (AC: #1, #6)
  - [ ] Run import against `docs/legacy/3_Activity/Activity.xml`
  - [ ] Verify all records imported correctly
  - [ ] Verify Icon data stored and retrievable
  - [ ] Verify data appears in UI and API

## Dev Notes

### Legacy XML Structure (docs/legacy/3_Activity/Activity.xml)

```xml
<dataroot>
  <Activity>
    <ActivityID>1</ActivityID>
    <Program>W01</Program>
    <ProgramName>Introduction to Computers</ProgramName>
    <Educational_x0020_Focus>Basic computer literacy...</Educational_x0020_Focus>
    <Folder>W01</Folder>
    <Grade>1</Grade>
    <Icon>base64data...</Icon>
  </Activity>
  ...
</dataroot>
```

**Note:** `Educational_x0020_Focus` is XML-encoded for "Educational Focus" with space.

### XSD Field Reference (docs/legacy/3_Activity/Activity.xsd)

| XSD Field | Type | Max Length | Required |
|-----------|------|------------|----------|
| ActivityID | int | - | Yes |
| Program | nvarchar | 255 | No |
| ProgramName | nvarchar | 255 | No |
| Educational_Focus | ntext/memo | ~536M | No |
| Folder | nvarchar | 255 | No |
| Grade | nvarchar | 255 | No |
| Icon | base64Binary/OLE | ~1.4GB | No |

### Field Mapping

| XML Field | Entity Field | Notes |
|-----------|--------------|-------|
| ActivityID | Id | Primary key |
| Program | Code | Activity code/identifier |
| ProgramName | Name | Display name |
| Educational_x0020_Focus | Description | Memo/long text |
| Folder | Folder | File system folder |
| Grade | GradeLevel | Target grade level |
| Icon | Icon | Base64 OLE object |

### Import Runner Pattern (Follow ClassGroupImportRunner)

```csharp
public class ActivityImportRunner
{
    public async Task<ImportResult> ImportAsync(string xmlPath, bool preview = false)
    {
        var result = new ImportResult();
        var xml = XDocument.Load(xmlPath);

        foreach (var element in xml.Descendants("Activity"))
        {
            try
            {
                var activity = MapFromXml(element);
                if (ValidateActivity(activity, out var errors))
                {
                    if (!preview)
                    {
                        await SaveActivityAsync(activity);
                    }
                    result.ImportedCount++;
                }
                else
                {
                    result.SkippedCount++;
                    result.Errors.AddRange(errors);
                }
            }
            catch (Exception ex)
            {
                result.ErrorCount++;
                result.Errors.Add($"Error processing activity: {ex.Message}");
            }
        }

        return result;
    }

    private Activity MapFromXml(XElement element)
    {
        return new Activity
        {
            Id = int.Parse(element.Element("ActivityID")?.Value ?? "0"),
            Code = element.Element("Program")?.Value,
            Name = element.Element("ProgramName")?.Value,
            Description = element.Element("Educational_x0020_Focus")?.Value,
            Folder = element.Element("Folder")?.Value,
            GradeLevel = element.Element("Grade")?.Value,
            Icon = element.Element("Icon")?.Value,  // Already base64
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Icon Handling Notes

- Icon field in XML is already base64 encoded (OLE object)
- Store directly as string - no conversion needed
- OLE objects may be large (11.8MB Activity.xml file size indicates embedded images)
- Consider logging Icon data sizes for monitoring

### File Locations

| Component | Path |
|-----------|------|
| Import Runner | `apps/backend/tools/DataMigration/ActivityImportRunner.cs` |
| XML Source | `docs/legacy/3_Activity/Activity.xml` |
| XSD Schema | `docs/legacy/3_Activity/Activity.xsd` |

### CLI Usage

```bash
# Preview import (dry run)
dotnet run --project apps/backend/tools/DataMigration -- import activities --preview --source docs/legacy/3_Activity/Activity.xml

# Execute import
dotnet run --project apps/backend/tools/DataMigration -- import activities --source docs/legacy/3_Activity/Activity.xml
```

### Testing Strategy

1. Run preview to verify parsing and mapping
2. Run import against test database
3. Verify record counts match XML
4. Verify Icon data stored correctly
5. Query API to confirm data accessible
6. Check UI displays imported activities

### Project Structure Notes

- Follow existing DataMigration tool patterns
- Activity import is simpler than Children (only 7 fields)
- No foreign key relationships to handle (unlike ClassGroup)

### References

- [Source: docs/legacy/3_Activity/Activity.xsd] - Complete XSD definition
- [Source: docs/legacy/3_Activity/Activity.xml] - Source data file
- [Source: apps/backend/tools/DataMigration/ClassGroupImportRunner.cs] - Reference implementation pattern
- [Source: _bmad-output/planning-artifacts/epics.md#Story-8.3] - Story requirements

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
