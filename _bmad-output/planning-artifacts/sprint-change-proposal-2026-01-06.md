# Sprint Change Proposal - Migration Stories Before UI E2E Tests

**Project:** kcow-online
**Date:** 2026-01-06
**Prepared For:** Joe

---

## 1. Issue Summary

A gap was identified in the epic sequencing: data migration was not explicitly captured before UI Playwright E2E tests in each epic. Because each epic depends on data from previous work, adding incremental data migration per epic reduces rework and keeps tests grounded in realistic migrated data.

**Trigger:** All epics require migrated data before UI tests.
**Problem Type:** Misunderstanding of original requirements.
**Evidence:** Incremental migration per epic prevents rework and supports dependent workflows with valid data.

---

## 2. Impact Analysis

### Epic Impact
- All epics remain viable, but each needs a data migration story inserted before its UI Playwright tests.
- No epic order changes required.
- No new epics required.
- No epics are obsolete.

### Artifact Conflicts
- PRD: No changes.
- Architecture: No changes.
- UI/UX: No changes.
- Other artifacts (testing strategy/docs/CI): No changes.

---

## 3. Recommended Approach

**Chosen Path:** Direct Adjustment

**Rationale:** Low effort and low risk. This adds explicit data migration per epic, aligning test data needs with incremental delivery, without changing scope or timelines.

**Effort:** Low
**Risk:** Low
**Timeline Impact:** Minimal

---

## 4. Detailed Change Proposals

### Stories (Epic 2 - Trucks & Schools)

**Insert before Playwright tests**

**OLD**
```
### Story 2.5: School Contacts & Billing Settings
...
---

### Story 2.6: E2E Tests - Trucks & Schools CRUD
```

**NEW**
```
### Story 2.5: School Contacts & Billing Settings
...
---

### Story 2.6: Data Migration - Trucks & Schools

**As a** developer,
**I want** legacy Trucks/Schools data parsed, mapped, and imported into the database,
**So that** CRUD flows and E2E tests operate on real migrated records.

**Acceptance Criteria:**

**Given** legacy XML/XSD data for Schools (docs/legacy/1_School/)
**When** the migration import executes for Schools
**Then** all valid School records are inserted into the database
**And** field mappings transform legacy data to new schema (e.g., contact fields, billing settings)
**And** Truck seed data is loaded for route assignments
**And** validation errors are captured and logged to the migration audit log
**And** a summary report shows imported count, skipped count, and error count
**And** the imported data is available in the UI and API

---

### Story 2.7: E2E Tests - Trucks & Schools CRUD
```

### Stories (Epic 3 - Class Groups & Scheduling)

**Insert before Playwright tests**

**OLD**
```
### Story 3.5: Class Group Scheduling & Conflict Detection
...
---

### Story 3.6: E2E Tests - Class Groups & Scheduling Conflicts
```

**NEW**
```
### Story 3.5: Class Group Scheduling & Conflict Detection
...
---

### Story 3.6: Data Migration - Class Groups & Schedules

**As a** developer,
**I want** legacy Class Group data parsed, mapped, and imported into the database,
**So that** scheduling and conflict detection operate on real migrated schedules.

**Acceptance Criteria:**

**Given** legacy XML/XSD data for Class Groups (docs/legacy/2_Class_Group/)
**When** the migration import executes for Class Groups
**Then** all valid Class Group records are inserted into the database
**And** school associations are linked via imported school IDs
**And** schedule/time slot data is mapped to the new schema
**And** validation errors are captured and logged to the migration audit log
**And** a summary report shows imported count, skipped count, and error count
**And** the imported data is available in the UI and API

---

### Story 3.7: E2E Tests - Class Groups & Scheduling Conflicts
```

### Stories (Epic 4 - Student & Family Management)

**Insert before Playwright tests**

**OLD**
```
### Story 4.8: Family Grid + Relationship Management
...
---

### Story 4.9: E2E Tests - Student Management & Global Search
```

**NEW**
```
### Story 4.8: Family Grid + Relationship Management
...
---

### Story 4.9: Data Migration - Students & Families

**As a** developer,
**I want** legacy Children (Students) and Family data parsed, mapped, and imported into the database,
**So that** the single-screen profile and global search operate on real migrated records.

**Acceptance Criteria:**

**Given** legacy XML/XSD data for Children (docs/legacy/4_Children/)
**When** the migration import executes for Children/Students
**Then** all valid Student records are inserted into the database
**And** Family records are created and linked to Students
**And** school and class group assignments are linked via imported IDs
**And** contact information, guardian details, and medical info are mapped
**And** validation errors are captured and logged to the migration audit log
**And** a summary report shows imported count, skipped count, and error count
**And** the imported data is searchable and visible in the student profile UI

---

### Story 4.10: E2E Tests - Student Management & Global Search
```

### Stories (Epic 5 - Attendance & Evaluations)

**Insert before Playwright tests**

**OLD**
```
### Story 5.6: Bulk Attendance Entry
...
---

### Story 5.7: E2E Tests - Attendance Tracking & Audit Trail
```

**NEW**
```
### Story 5.6: Bulk Attendance Entry
...
---

### Story 5.7: Data Migration - Attendance & Evaluations

**As a** developer,
**I want** legacy attendance and evaluation data parsed, mapped, and imported into the database,
**So that** tracking flows and audit trails operate on real historical records.

**Acceptance Criteria:**

**Given** legacy XML/XSD data for Activities (docs/legacy/3_Activity/) and related attendance records
**When** the migration import executes for Attendance and Evaluations
**Then** all valid attendance records are inserted with correct student and class group links
**And** evaluation/assessment records are imported with proper date and score mappings
**And** historical data maintains accurate timestamps for audit trail purposes
**And** validation errors are captured and logged to the migration audit log
**And** a summary report shows imported count, skipped count, and error count
**And** the imported data appears correctly in the attendance tab and evaluation tab

---

### Story 5.8: E2E Tests - Attendance Tracking & Audit Trail
```

### Stories (Epic 6 - Billing & Financials)

**Insert before Playwright tests**

**OLD**
```
### Story 6.6: Billing Status in Profile Header
...
---

### Story 6.7: E2E Tests - Billing Management & Financial Accuracy
```

**NEW**
```
### Story 6.6: Billing Status in Profile Header
...
---

### Story 6.7: Data Migration - Billing & Financials

**As a** developer,
**I want** legacy billing, invoice, and payment data parsed, mapped, and imported into the database,
**So that** financial flows and balance calculations operate on real migrated records.

**Acceptance Criteria:**

**Given** legacy billing data from Children records and school billing configurations
**When** the migration import executes for Billing records
**Then** all valid invoice records are inserted with correct student and family links
**And** payment history is imported with accurate dates and amounts
**And** outstanding balances are calculated correctly from migrated data
**And** school billing settings are applied to imported records
**And** validation errors are captured and logged to the migration audit log
**And** a summary report shows imported count, skipped count, and error count
**And** the imported data displays correctly in the financial tab and profile header

---

### Story 6.8: E2E Tests - Billing Management & Financial Accuracy
```

**Note:** Epic 7 is the migration tooling epic (parser, mapping service, preview, execution, audit log, re-run). The migration stories above use the infrastructure built in Epic 7. If Epic 7 is not yet complete, the migration stories should stub/seed representative data that matches the expected migrated structure.

---

## 5. Implementation Handoff

**Scope Classification:** Moderate (backlog reorganization across multiple epics)

**Recipients and Responsibilities:**
- Development team: Update epic story ordering and add data migration stories as specified.
- PO/SM and PM/Architect: Optional review only; no strategic changes required.

**Success Criteria:**
- Each epic includes a data migration story placed immediately before its UI Playwright tests story.
- Migration stories actually import legacy data into the database (not just preview/mapping).
- Playwright tests validate UI behavior against the imported migrated data.
- No changes required to PRD, architecture, or UX artifacts.

---

**End of Proposal**
