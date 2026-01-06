# Story 4.9: Data Migration - Students & Families

Status: ready-for-dev

## Story

As a developer,
I want legacy Children (Students) and Family data parsed, mapped, and imported into the database,
So that the single-screen profile and global search operate on real migrated records.

## Acceptance Criteria

1. **Given** legacy XML/XSD data for Children (`docs/legacy/4_Children/`)
   **When** the migration import executes for Children/Students
   **Then** all valid Student records are inserted into the database

2. **Given** the migration imports Children data
   **Then** Family records are created and linked to Students

3. **Given** the migration processes Children records
   **Then** school and class group assignments are linked via imported IDs

4. **Given** the migration maps Children fields
   **Then** contact information, guardian details, and medical info are mapped

5. **Given** validation errors occur during import
   **Then** errors are captured and logged to the migration audit log

6. **Given** the import completes
   **Then** a summary report shows imported count, skipped count, and error count

7. **Given** the migration is complete
   **Then** the imported data is searchable and visible in the student profile UI

## Tasks / Subtasks

- [ ] Task 1: Create Legacy Schema Parser for Children (AC: #1)
  - [ ] Read and validate Children.xml against Children.xsd schema
  - [ ] Extract all 92 fields from Children XSD
  - [ ] Handle encoding and format variations
  - [ ] Parse nested contact/guardian structures

- [ ] Task 2: Implement Family Record Creation (AC: #2)
  - [ ] Extract family/guardian data from Children records
  - [ ] Create Family entities with contact info
  - [ ] Link families to students via StudentFamily join table
  - [ ] Set relationship types (parent, guardian, sibling)

- [ ] Task 3: Implement School/Class Group Assignment Linking (AC: #3)
  - [ ] Map legacy School IDs to imported School records
  - [ ] Map legacy Class Group IDs to imported ClassGroup records
  - [ ] Handle orphaned students (school/class not found)
  - [ ] Log association errors

- [ ] Task 4: Implement Contact and Medical Info Mapping (AC: #4)
  - [ ] Map legacy contact fields (phone, email, address)
  - [ ] Map guardian information
  - [ ] Map medical/health information fields
  - [ ] Translate Afrikaans field names to English

- [ ] Task 5: Implement Validation and Error Logging (AC: #5)
  - [ ] Validate imported records against XSD constraints
  - [ ] Create audit log entries for validation errors
  - [ ] Include file/line information in error logs

- [ ] Task 6: Create Import Summary Report (AC: #6)
  - [ ] Track imported, skipped, and error counts
  - [ ] Separate counts for Students and Families
  - [ ] Include association errors in report

- [ ] Task 7: Verify Search and Profile Visibility (AC: #7)
  - [ ] Test that imported Students appear in GET /api/students
  - [ ] Test global search returns imported students
  - [ ] Test student profile displays all imported data
  - [ ] Test Family Grid shows linked families

## Dev Notes

### Architecture Requirements
- **Legacy XSD Location**: `docs/legacy/4_Children/Children.xsd` (92 fields!)
- **Entity Locations**: 
  - `apps/backend/src/Domain/Entities/Student.cs`
  - `apps/backend/src/Domain/Entities/Family.cs`
  - `apps/backend/src/Domain/Entities/StudentFamily.cs`
- **Import Service Location**: `apps/backend/src/Application/Import/`
- **CLI Command**: `dotnet run import students`

### ⚠️ CRITICAL: XSD Schema Alignment
The Children.xsd has **92 fields** - this is the largest entity. All fields must be:
- Included in the Student domain entity
- Mapped from legacy data
- Validated against XSD constraints
- Exposed in API DTOs

### Field Mapping Reference
See `docs/domain-models.md` for complete Afrikaans → English translations:
- Student (Children) entity: 92 fields
- Family extraction pattern from Children data
- Contact consolidation approach

### Dependency Requirements
- Story 2-6 (Schools migration) must be completed first
- Story 3-6 (Class Groups migration) must be completed first
- Schools and Class Groups must be imported before Students

### Import Order
1. Schools (Story 2-6)
2. Class Groups (Story 3-6)
3. Students & Families (this story)

### Family Linking Strategy
- Extract guardian/parent info from Children records
- Create shared Family records where siblings exist
- Use surname matching for family grouping (configurable)

### Previous Story Context
- Story 4-8 completed Family Management in Student Profile
- Student CRUD UI is functional
- Global search is implemented
- API endpoints operational

### Testing Standards
- Integration tests in `apps/backend/tests/Integration/Import/`
- Use sample XML files from `docs/legacy/4_Children/`
- Test with siblings to validate family linking
- Test global search performance with imported data

### Project Structure Notes
- Reuse import infrastructure from previous migration stories
- Extend ImportAuditLog for student imports

### References
- [Source: docs/legacy/4_Children/Children.xsd#92-fields]
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
