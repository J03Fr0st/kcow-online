---
generated: 2026-01-09T00:00:00+02:00
project: kcow-online
change_type: New Epic Addition
scope: Minor
workflow: correct-course
---

# Sprint Change Proposal: Add Epic 8 - Activity Management

## Section 1: Issue Summary

### Problem Statement

The Activity entity has an XSD schema definition (`docs/legacy/3_Activity/Activity.xsd` with 7 fields including an Icon/OLEObject field) but **no dedicated epic** for managing Activity records. Currently, Activity is only referenced within Epic 5 (Evaluations - Story 5.4) as a dependency for the Evaluation entity, which requires an `ActivityId` foreign key.

### Context & Discovery

- **Source of Issue:** Epic/Story gap analysis during Correct Course workflow
- **Evidence:**
  - Activity XSD exists at `docs/legacy/3_Activity/Activity.xsd` with 7 fields
  - Story 5.4 (Evaluations) references Activity entity but assumes it exists
  - Activity is listed in docs/index.md as a domain entity
  - No CRUD stories exist for Activity management

### Impact Severity

**Low** - This is a gap in the original epic breakdown. Activity records are lookup/maintenance data that must exist before Epic 5 (Evaluations) can be implemented.

---

## Section 2: Impact Analysis

### Epic Impact

| Epic | Impact | Details |
|------|--------|---------|
| **Epic 8 (NEW)** | **CREATE** | New epic: Activity Management - CRUD operations only |
| Epic 5 | **DEPENDENCY** | Story 5.4 requires Activity entity to exist first |
| Epics 1-4, 6-7 | **NONE** | No impact |

### Story Impact

**New Stories Required (Epic 8):**
- 8.1: Activity Entity & API Endpoints
- 8.2: Activities Management UI
- 8.3: Data Migration - Activities
- 8.4: E2E Tests - Activities CRUD

**Modified Story:**
- Story 5.4 should note dependency on Epic 8 completion

### Artifact Conflicts

| Artifact | Conflict | Changes Required |
|----------|----------|------------------|
| **Epics** | Missing Epic | Add Epic 8 to epics.md |
| **PRD** | Minor | Activity already mentioned; no changes needed |
| **Architecture** | Minor | Add Activity to domain entities list, project structure |
| **UX Design** | Minor | Add "Activities" to sidebar navigation |
| **Sprint Status** | Update | Add Epic 8 tracking entries |

### Technical Impact

**Backend:**
- Add `Activity` entity to `Domain/Entities/`
- Add `Activities` to `Application/` folder
- Add `/api/activities` endpoints
- EF Core migration for `activities` table

**Frontend:**
- Add `features/activities/` feature module
- Add "Activities" link to sidebar navigation
- Activities list and CRUD forms

**Database:**
- New `activities` table with 7 fields per XSD
- Icon field stored as TEXT/base64 string (SQLite has size limits, ensure field is large enough)

---

## Section 3: Recommended Approach

### Selected Path: Direct Adjustment (Add Epic 8)

**Rationale:**
1. **Clean Addition** - No breaking changes to existing epics or stories
2. **Dependency Resolution** - Epic 5 (Evaluations) requires Activity to exist first
3. **Low Effort** - CRUD-only scope follows established patterns from Epic 2
4. **Low Risk** - Maintenance entity, no complex business logic

### Effort & Risk Assessment

| Metric | Level | Details |
|--------|-------|---------|
| **Effort** | Low | 4 stories following established CRUD patterns |
| **Risk** | Low | Isolated feature, no dependencies on incomplete work |
| **Timeline Impact** | Minimal | ~1-2 days of work, parallelizable with other maintenance epics |

### Epic Sequencing

**Recommended Order:**
- Epic 8 should be implemented **BEFORE Epic 5** (Evaluations)
- Story 5.4 (Evaluation Entity & API Endpoints) has `ActivityId` FK dependency
- Can be implemented in parallel with Epic 2 (Trucks & Schools) as both are maintenance/reference data

---

## Section 4: Detailed Change Proposals

### 4.1 Epics Document Changes

**Location:** `_bmad-output/planning-artifacts/epics.md`

**CHANGE 1: Add Epic 8 to Epic List (after Epic 7, before "Epic 1" section)**

```markdown
### Epic 8: Activity Management

Admin can manage the educational program catalog (activities) that are delivered during class sessions and evaluated in student progress tracking.

**FRs covered:** Foundation for Epic 5 (Evaluations - Activity reference)

**Implementation Notes:**
- Activity CRUD with program code, grade level, and curriculum info
- API endpoints + frontend feature module
- Legacy data migration from Activity XML/XSD
- Simple maintenance entity following Epic 2 patterns

---
```

**CHANGE 2: Add Epic 8 Stories (after Epic 7 stories)**

```markdown
## Epic 8: Activity Management

Admin can manage the educational program catalog (activities) that are delivered during class sessions.

### Story 8.1: Activity Entity & API Endpoints

**As a** developer,
**I want** the Activity domain entity and REST API endpoints,
**So that** activity data can be managed through the API.

**Acceptance Criteria:**

**Given** the backend project
**When** the Activity entity is created
**Then** the `Activity` entity exists with properties from XSD:
- Id (ActivityID)
- Code (Program)
- Name (ProgramName)
- Description (Educational_Focus)
- Folder
- GradeLevel (Grade)
- Icon (base64Binary - OleObject image stored as base64 string)

**And** EF Core configuration and migration create the `activities` table
**And** the Icon field is stored as TEXT/base64 string in SQLite (large enough for OLE object data)
**And** `/api/activities` endpoints support:
- GET (list all activities)
- GET `/:id` (get single activity)
- POST (create activity)
- PUT `/:id` (update activity)
- DELETE `/:id` (archive activity)

**And** endpoints require authentication
**And** ProblemDetails errors for validation failures (FR13)

---

### Story 8.2: Activities Management UI

**As an** admin,
**I want** an activities management page to view and manage the program catalog,
**So that** I can maintain the educational activities offered.

**Acceptance Criteria:**

**Given** I am authenticated and click "Activities" in sidebar
**When** the Activities page loads
**Then** I see a table listing activities with Icon thumbnail, Code, Name, Grade Level, Folder, Status columns
**And** I see "Add Activity" button

**Given** I click "Add Activity"
**When** the create form appears
**Then** I can enter activity details including:
- Code, Name, Description, Folder, Grade Level
- Icon upload (image file converted to base64 on save)
**And** the new activity appears in the list with icon thumbnail

**Given** I click an activity row
**When** the edit form appears
**Then** I can update activity details and save
**And** I see a success confirmation

**Given** I click delete on an activity
**When** I confirm the action
**Then** the activity is archived (soft-deleted) and removed from the active list

**And** validation errors display inline
**And** loading states are shown during API calls

---

### Story 8.3: Data Migration - Activities

**As a** developer,
**I want** legacy Activity data parsed, mapped, and imported into the database,
**So that** the activities catalog is populated with existing programs.

**Acceptance Criteria:**

**Given** legacy XML/XSD data for Activities (docs/legacy/3_Activity/)
**When** the migration import executes for Activities
**Then** all valid Activity records are inserted into the database
**And** field mappings transform legacy data to new schema:
  - ActivityID → Id
  - Program → Code
  - ProgramName → Name
  - Educational_Focus → Description
  - Folder → Folder
  - Grade → GradeLevel
  - Icon (OleObject/base64Binary) → Icon (base64 string)
**And** the Icon OLE object data is extracted and stored as base64 string
**And** validation errors are captured and logged to the migration audit log
**And** a summary report shows imported count, skipped count, and error count
**And** the imported data is available in the UI and API

---

### Story 8.4: E2E Tests - Activities CRUD

**As a** developer,
**I want** E2E tests covering activity management workflows,
**So that** CRUD operations and validation are validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running and authenticated as admin
**When** I run E2E tests for activities
**Then** the following scenarios are covered:

**Activities Management:**
- User can navigate to Activities page and see activity list
- User can create a new activity with valid data
- User can edit existing activity details
- User can soft-delete an activity (removed from active list)
- Validation errors display inline for invalid data

**Data Integrity:**
- Activity code uniqueness is enforced
- Grade level mapping persists correctly
- Search and filtering work on list page
- Icon base64 data is correctly stored and retrieved
- Icon thumbnails display correctly in the list

**And** E2E tests are organized in `e2e/activities/`
**And** Tests use seeded test data (multiple activities)
**And** Tests clean up test data after completion
**And** All tests must pass for Epic 8 completion

---
```

### 4.2 Architecture Document Changes

**Location:** `_bmad-output/planning-artifacts/architecture.md`

**CHANGE: Update Domain Entities section**

```markdown
### Data Architecture

- **Database:** SQLite (v1), migration path to PostgreSQL.
- **ORM:** EF Core; entities in Domain, configs in Infrastructure/Data.
- **Schema Source:** ⚠️ **XSD files are authoritative**—all entity schemas must strictly align with `docs/legacy/*.xsd`.
  | Entity | XSD Location | Field Count |
  |--------|--------------|-------------|
  | School | `docs/legacy/1_School/School.xsd` | 30 fields |
  | Class Group | `docs/legacy/2_Class_Group/Class Group.xsd` | 15 fields |
  | Activity | `docs/legacy/3_Activity/Activity.xsd` | 7 fields |
  | Children | `docs/legacy/4_Children/Children.xsd` | 92 fields |
```

**CHANGE: Update Feature/FR Mapping**

```markdown
### Requirements to Structure Mapping

**Feature/FR Mapping:**
- FR1 Trucks & Fleet -> `features/trucks`, `Application/Trucks`
- FR2-3 Schools Management -> `features/schools`, `Application/Schools`
- FR4-6 Class Groups & Scheduling -> `features/class-groups`, `Application/ClassGroups`
- FR7-11 Student Management -> `features/students`, `features/families`, `Application/Students`, `Application/Families`
- **Activity Management -> `features/activities`, `Application/Activities`** ← NEW
- FR12 Data Migration -> `features/import`, `Application/Import`
```

**CHANGE: Update Project Structure**

```markdown
│   │   │   │   │   ├── activities/       # activity management ← NEW
│   │   │   │   │   ├── attendance/     # attendance tracking
```

### 4.3 UX Design Document Changes

**Location:** `_bmad-output/planning-artifacts/ux-design-specification.md`

**CHANGE: Update sidebar navigation**

```markdown
**Given** I am authenticated
**When** I access the root path (`/`)
**Then** I see the AdminLayout with:
- Sidebar navigation with module links (Dashboard, Trucks, Schools, Class Groups, Students, Activities, Billing)
```

### 4.4 Sprint Status Changes

**Location:** `_bmad-output/implementation-artifacts/sprint-status.yaml`

**CHANGE: Add Epic 8 section (after Epic 7, before retrospective sections)**

```yaml
  # ========================
  # Epic 8: Activity Management
  # ========================
  epic-8: backlog
  8-1-activity-entity-api-endpoints: ready-for-dev
  8-2-activities-management-ui: ready-for-dev
  8-3-data-migration-activities: ready-for-dev
  8-4-e2e-tests-activities-crud: ready-for-dev
  epic-8-retrospective: optional
```

---

## Section 5: Implementation Handoff

### Change Scope Classification

**Scope: MINOR**

**Justification:**
- Single new epic with CRUD-only scope
- No modifications to existing epics/stories
- Follows established patterns from Epic 2
- No breaking changes

### Handoff Recipients

| Role | Responsibility |
|------|----------------|
| **Development Team** | Implement Epic 8 stories following established CRUD patterns |
| **Product Owner / Scrum Master** | Update sprint backlog to include Epic 8 stories |
| **Product Manager** | Review and approve Epic 8 addition to epics.md |

### Success Criteria

- [ ] Epic 8 added to epics.md with 4 stories defined
- [ ] Architecture.md updated with Activity entity references
- [ ] UX Design updated with Activities navigation
- [ ] Sprint status updated with Epic 8 entries
- [ ] Epic 5 (Story 5.4) notes dependency on Epic 8 completion
- [ ] All 4 Epic 8 stories completed and tested
- [ ] E2E tests passing for Activity CRUD

### Implementation Notes

1. **Prerequisite:** Epic 8 should be completed **before** Epic 5 (Evaluations) due to ActivityId FK dependency
2. **Pattern Reference:** Follow Epic 2 (Trucks & Schools) implementation patterns
3. **XSD Compliance:** Implement all 7 fields from `docs/legacy/3_Activity/Activity.xsd`
4. **Parallel Execution:** Epic 8 can be developed in parallel with Epic 2

---

## Appendix: Activity XSD Fields

For reference, the Activity entity must include these 7 fields from the XSD:

```xml
<xs:element name="Activity">
  <xs:complexType>
    <xs:sequence>
      <xs:element name="ActivityID" type="xs:int"/>           <!-- Primary Key -->
      <xs:element name="Program" type="xs:string"/>          <!-- Activity code -->
      <xs:element name="ProgramName" type="xs:string"/>      <!-- Activity name -->
      <xs:element name="Educational_Focus" type="xs:string"/><!-- Description (memo) -->
      <xs:element name="Folder" type="xs:string"/>           <!-- Category/filter -->
      <xs:element name="Grade" type="xs:string"/>            <!-- Grade level -->
      <xs:element name="Icon" type="xs:base64Binary"/>       <!-- OLE Object image (store as base64 string) -->
    </xs:sequence>
  </xs:complexType>
</xs:element>
```

**Note:**
- XSD field names are used as-is (no translation needed - they're already in English)
- `Educational_Focus` has `x0020` for space character (XML encoding)
- `Icon` is `od:jetType="oleobject"` / `od:sqlSType="image"` - store as base64 string in SQLite
- The Activity.xml file is 11.8MB due to embedded OLE object data
