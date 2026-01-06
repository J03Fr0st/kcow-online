# Sprint Change Proposal: E2E Testing After Each Epic

**Date:** 2026-01-05
**Author:** Joe (via Correct Course Workflow)
**Change Classification:** Moderate
**Status:** ✅ APPROVED
**Approved By:** Joe

---

## Executive Summary

Add comprehensive E2E and integration testing stories to each epic to ensure feature integrity, validate critical user workflows, and establish regression protection. This quality enhancement adds 7 new stories (one per epic) without changing feature scope or requirements.

---

## Section 1: Issue Summary

**Problem Statement:**
E2E testing coverage is not explicitly planned after epic completion, creating risk that features may have integration issues, performance problems, or regressions that only surface in complete user workflows.

**Discovery Context:**
- Triggered during implementation planning phase
- Recognition that unit tests alone won't validate end-to-end workflows
- Playwright is already configured but no test stories exist in epics

**Evidence:**
- Architecture document mentions E2E testing (`apps/frontend/e2e/`) but lacks defined test stories
- Critical user journeys (global search, student profile, scheduling conflicts) need workflow validation
- Performance targets (NFR1, NFR2: <2s response times) require E2E measurement
- Audit trail requirements (FR14) need integration validation

---

## Section 2: Impact Analysis

### Epic Impact

| Epic | Impact Level | Changes Required | Rationale |
|------|-------------|------------------|-----------|
| Epic 1: Auth | HIGH | Add Story 1.6 - Auth E2E tests | Login/logout flows and protected routes are foundational |
| Epic 2: Trucks/Schools | HIGH | Add Story 2.6 - CRUD E2E tests | First CRUD workflows; need validation before use in scheduling |
| Epic 3: Class Groups | HIGH | Add Story 3.6 - Scheduling E2E tests | Conflict detection (FR6) is critical; must test thoroughly |
| Epic 4: Students | CRITICAL | Add Story 4.9 - Core journey E2E tests | Primary user workflow; global search and profile are daily use |
| Epic 5: Attendance | HIGH | Add Story 5.7 - Attendance E2E tests | Audit trail (FR14) requires integration validation |
| Epic 6: Billing | HIGH | Add Story 6.7 - Billing E2E tests | Financial accuracy requires calculation validation |
| Epic 7: Migration | MEDIUM | Add Story 7.7 - Import integration tests | Developer tooling; integration tests vs E2E |

### Story Impact

**Total New Stories:** 7
- 6 E2E testing stories (Epics 1-6)
- 1 integration testing story (Epic 7)

**No Existing Stories Modified:** All existing stories remain unchanged

### Artifact Impact

**Epics.md:**
- ✅ **Requires Update** - Add 7 new stories to respective epics
- No existing story content changed
- Epic completion criteria updated to include test passing

**Architecture.md:**
- ✅ **Requires Update** - Add E2E testing strategy section
- Document test data seeding approach
- Define test environment configuration
- Specify test organization structure

**PRD:**
- ✅ **No Changes** - E2E testing supports existing requirements
- Reinforces technical success criteria
- Strengthens reliability (NFR7) and performance (NFR1-3) assurance

**UX Design Specification:**
- ✅ **No Changes** - E2E tests validate existing UX flows
- Covers user journeys: One-Screen Update, Fix Attendance, Term Scheduling

### Technical Impact

**Code Changes:**
- New E2E test files: `e2e/auth/`, `e2e/trucks-schools/`, `e2e/class-groups/`, `e2e/students/`, `e2e/attendance-evaluations/`, `e2e/billing/`
- New integration tests: `apps/backend/tests/Integration/Import/`
- Test data fixtures and seeding utilities
- Test environment configuration

**Infrastructure:**
- Test database setup (SQLite or in-memory)
- Test data seed scripts
- CI/CD integration for E2E test execution
- Test reporting and coverage tracking

**Timeline Impact:**
- +1-2 days per epic for E2E test creation
- Tests run in parallel with development
- Minimal impact on overall timeline (quality investment)

---

## Section 3: Recommended Approach

**Selected Path:** Option 1 - Direct Adjustment

**Rationale:**
- Maintains current epic structure without disruption
- Adds quality assurance without changing feature scope
- Each epic gains explicit test completion criteria
- Establishes regression protection for future development
- Follows testing best practices for web applications

**Effort Estimate:**
- Medium effort overall
- 7 new stories (~2-3 days each for test creation)
- Total: ~14-21 days of development effort spread across sprints

**Risk Assessment:**
- **Low Risk:** Tests validate existing behavior, don't change features
- **High Value:** Catches integration issues early
- **Long-term Benefit:** Regression protection as codebase grows

**Timeline Impact:**
- Minimal delay to feature delivery
- Tests can be developed in parallel with features
- Net positive: prevents costly bug fixes later

---

## Section 4: Detailed Change Proposals

### Epic 1: Project Foundation & Authentication

**New Story:** Story 1.6 - E2E Tests - Authentication Flows

**What Changes:**
- Add E2E test story as last story in Epic 1
- Tests cover: login, logout, protected routes, auth tokens
- Tests organized in `e2e/auth/`
- Epic 1 complete only when tests pass

**OLD:**
```markdown
### Story 1.5: Theme System & Accessibility Baseline
... [existing content] ...
```

**NEW:**
```markdown
### Story 1.5: Theme System & Accessibility Baseline
... [existing content] ...

---

### Story 1.6: E2E Tests - Authentication Flows

**As a** developer,
**I want** E2E tests covering authentication workflows,
**So that** login, logout, and protected routes are validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running
**When** I run E2E tests for authentication
**Then** the following scenarios are covered:

**Login Flow:**
- User can navigate to `/login` and see the login form
- User can enter valid credentials and submit successfully
- User is redirected to dashboard after successful login
- User sees inline error message with invalid credentials
- User remains on login page after failed login

**Protected Routes:**
- Unauthenticated user accessing `/` is redirected to `/login`
- Authenticated user can access dashboard and sidebar links
- Auth tokens are attached to API requests

**Logout Flow:**
- User can click logout and be redirected to login
- Session is cleared (token removed)

**And** E2E tests are organized in `e2e/auth/`
**And** Tests use test data seeding for admin user creation
**And** All tests must pass for Epic 1 completion
```

---

### Epic 2: Trucks & Schools Management

**New Story:** Story 2.6 - E2E Tests - Trucks & Schools CRUD

**What Changes:**
- Add E2E test story as last story in Epic 2
- Tests cover: CRUD operations, validation, data persistence
- Tests organized in `e2e/trucks-schools/`

**OLD:**
```markdown
### Story 2.5: School Billing Settings
... [existing content] ...
```

**NEW:**
```markdown
### Story 2.5: School Billing Settings
... [existing content] ...

---

### Story 2.6: E2E Tests - Trucks & Schools CRUD

**As a** developer,
**I want** E2E tests covering truck and school management workflows,
**So that** CRUD operations and validation are validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running and authenticated as admin
**When** I run E2E tests for trucks and schools
**Then** the following scenarios are covered:

**Trucks Management:**
- User can navigate to Trucks page and see truck list
- User can create a new truck with valid data
- User can edit existing truck details
- User can soft-delete a truck (removed from active list)
- Validation errors display inline for invalid data

**Schools Management:**
- User can navigate to Schools page and see school list
- User can create a new school with contact information
- User can edit school details including contacts
- User can configure school billing settings (default rate, cycle)
- User can archive a school

**Data Integrity:**
- School contacts and billing settings persist correctly
- Truck status changes are reflected in UI
- Search and filtering work on list pages

**And** E2E tests are organized in `e2e/trucks-schools/`
**And** Tests use seeded test data (multiple trucks, schools)
**And** Tests clean up test data after completion
**And** All tests must pass for Epic 2 completion
```

---

### Epic 3: Class Groups & Scheduling

**New Story:** Story 3.6 - E2E Tests - Class Groups & Scheduling Conflicts

**What Changes:**
- Add E2E test story as last story in Epic 3
- Tests cover: scheduling, conflict detection (FR6), weekly view
- Tests organized in `e2e/class-groups/`

**OLD:**
```markdown
### Story 3.5: Weekly Schedule Overview
... [existing content] ...
```

**NEW:**
```markdown
### Story 3.5: Weekly Schedule Overview
... [existing content] ...

---

### Story 3.6: E2E Tests - Class Groups & Scheduling Conflicts

**As a** developer,
**I want** E2E tests covering class group scheduling and conflict detection,
**So that** the scheduling workflow and conflict resolution are validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running and authenticated as admin
**When** I run E2E tests for class groups and scheduling
**Then** the following scenarios are covered:

**Class Groups CRUD:**
- User can navigate to Class Groups page and see list
- User can create a class group with school, truck, day/time
- User can edit class group schedule
- User can archive a class group
- Filters work for school and truck

**Schedule Configuration:**
- User can select day of week, start time, end time, sequence
- User can assign truck to class group
- Schedule appears in weekly view
- Schedule blocks show class group, school, truck details

**Conflict Detection (FR6 - Critical):**
- User attempts to create overlapping schedule for same truck
- Schedule Conflict Banner appears with warning
- Conflicting class group details are shown
- User is blocked from saving until conflict resolved
- User can adjust time or truck to resolve conflict
- After resolution, class group saves successfully

**Weekly View:**
- Calendar-style grid displays correctly
- Conflicts are visually highlighted
- Clicking schedule block navigates to edit

**And** E2E tests are organized in `e2e/class-groups/`
**And** Tests use seeded test data (schools, trucks, existing schedules)
**And** Conflict detection scenarios are thoroughly tested
**And** All tests must pass for Epic 3 completion
```

---

### Epic 4: Student & Family Management

**New Story:** Story 4.9 - E2E Tests - Student Management & Global Search

**What Changes:**
- Add E2E test story as last story in Epic 4
- Tests cover: global search (FR11), student profile (FR10), CRUD
- Tests validate performance targets (NFR1, NFR2)
- Tests organized in `e2e/students/`

**OLD:**
```markdown
### Story 4.8: Family Management in Student Profile
... [existing content] ...
```

**NEW:**
```markdown
### Story 4.8: Family Management in Student Profile
... [existing content] ...

---

### Story 4.9: E2E Tests - Student Management & Global Search

**As a** developer,
**I want** E2E tests covering student management and the global search workflow,
**So that** the primary user journey (student lookup and profile) is validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running and authenticated as admin
**When** I run E2E tests for student management
**Then** the following scenarios are covered:

**Global Student Search (FR11 - Critical):**
- User can access global search bar from any page
- Typing in search shows typeahead results with students
- Each result shows: Full Name, School, Grade, Class Group
- Search disambiguates students with similar names
- Selecting a result navigates to student profile
- Search returns results in under 2 seconds (NFR1 validation)
- "No results found" displays when no matches

**Student Profile Navigation:**
- User can navigate to student profile from list or search
- Profile loads with 3-column header (photo, demographics, school, status)
- Page loads in under 2 seconds (NFR2 validation)

**Single-Screen Student Profile (FR10 - Critical):**
- Profile header shows: basic info, school assignment, status indicators
- Tabbed sections visible: Child Info, Financial, Attendance, Evaluation
- User can switch between tabs without page reload
- Inline editing works in Child Info tab
- Save confirmation appears after edits
- Validation errors display inline

**Student CRUD:**
- User can create new student with school/class group/seat assignment
- Class group dropdown filters by selected school
- User can edit student demographics
- User can archive a student

**Family Management:**
- Family Grid displays at bottom of profile
- User can add family member with relationship type
- User can edit family contact information
- Sibling students show in Family Grid

**And** E2E tests are organized in `e2e/students/`
**And** Tests use seeded test data (schools, class groups, students, families)
**And** Global search performance is validated
**And** All tests must pass for Epic 4 completion
```

---

### Epic 5: Attendance & Evaluations

**New Story:** Story 5.7 - E2E Tests - Attendance Tracking & Audit Trail

**What Changes:**
- Add E2E test story as last story in Epic 5
- Tests cover: attendance CRUD, bulk entry, audit trail (FR14)
- Tests organized in `e2e/attendance-evaluations/`

**OLD:**
```markdown
### Story 5.6: Bulk Attendance Entry
... [existing content] ...
```

**NEW:**
```markdown
### Story 5.6: Bulk Attendance Entry
... [existing content] ...

---

### Story 5.7: E2E Tests - Attendance Tracking & Audit Trail

**As a** developer,
**I want** E2E tests covering attendance tracking and audit trail functionality,
**So that** attendance workflows and correction traceability are validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running and authenticated as admin
**When** I run E2E tests for attendance and evaluations
**Then** the following scenarios are covered:

**Attendance Tab in Student Profile:**
- User can navigate to Attendance tab
- List shows recent attendance with Date, Class Group, Status, Notes
- Status chips display correctly (Present=green, Absent=red, Late=yellow)
- User can click attendance row to activate inline edit
- User can change status or add notes
- Save confirms changes immediately
- User can add new attendance entry with date, class group, status

**Bulk Attendance Entry:**
- User can navigate to Class Groups page
- User can click "Take Attendance" on a class group
- All enrolled students display with status toggles
- User can mark attendance for all students (Present/Absent/Late)
- "Save All" creates attendance records for all students
- Existing attendance for same date pre-fills with current values
- Success confirmation appears after save

**Audit Trail (FR14 - Critical):**
- User updates an attendance record
- Audit log entry is created with timestamp, previous value, new value, user
- User can click "View History" on attendance record
- Audit Trail Panel appears showing all changes
- Audit trail is read-only
- Each change shows: timestamp, field changed, old value, new value, actor

**Evaluations Tab:**
- User can navigate to Evaluation tab
- List shows evaluations with Activity, Date, Score, Speed, Accuracy
- User can add new evaluation with activity, date, scores, notes
- User can edit existing evaluation inline
- Visual indicators display for score levels

**And** E2E tests are organized in `e2e/attendance-evaluations/`
**And** Tests use seeded test data (students, class groups, existing attendance)
**And** Audit trail creation and display are thoroughly tested
**And** All tests must pass for Epic 5 completion
```

---

### Epic 6: Billing & Financials

**New Story:** Story 6.7 - E2E Tests - Billing Management & Financial Accuracy

**What Changes:**
- Add E2E test story as last story in Epic 6
- Tests cover: invoicing, payments, balance calculations, audit trail (FR14)
- Tests organized in `e2e/billing/`

**OLD:**
```markdown
### Story 6.6: Billing Status in Profile Header
... [existing content] ...
```

**NEW:**
```markdown
### Story 6.6: Billing Status in Profile Header
... [existing content] ...

---

### Story 6.7: E2E Tests - Billing Management & Financial Accuracy

**As a** developer,
**I want** E2E tests covering billing workflows and financial data integrity,
**So that** invoicing, payments, and balance calculations are validated end-to-end.

**Acceptance Criteria:**

**Given** the application is running and authenticated as admin
**When** I run E2E tests for billing and financials
**Then** the following scenarios are covered:

**Financial Tab in Student Profile:**
- User can navigate to Financial tab
- Billing summary shows: current balance, last payment date/amount, outstanding invoices
- Invoices list displays with Date, Amount, Status
- Payments list displays with Date, Amount, Receipt #
- Summary and lists update correctly after changes

**Record Payment:**
- User can click "Record Payment" to open inline form
- Form displays: amount input, payment method dropdown, optional invoice, notes
- User can submit valid payment
- Payment appears in payments list
- Balance is updated correctly
- Receipt number is generated automatically
- If applied to invoice, invoice status updates

**Create Invoice:**
- User can click "Create Invoice" to open inline form
- Form displays: amount input, due date picker, description/notes
- User can submit valid invoice
- Invoice appears in invoices list
- Balance is updated
- Status is set to "Pending"

**Billing Status in Profile Header:**
- Profile header third column shows billing status
- Current balance displays with color indicator (green if 0, red if outstanding)
- Text shows "Up to date" or "Balance due"
- Warning indicator displays for overdue invoices

**Audit Trail (FR14 - Critical):**
- User updates a payment or invoice record
- Audit log entry is created with timestamp, previous value, new value, user
- User can click "View History" on billing record
- Audit Trail Panel appears showing all changes
- Audit trail is read-only

**Financial Calculations:**
- Balance updates correctly when invoice is created
- Balance updates correctly when payment is recorded
- Applied payments reduce invoice balance
- Calculations are accurate and consistent

**And** E2E tests are organized in `e2e/billing/`
**And** Tests use seeded test data (students, existing invoices/payments)
**And** Financial calculations and audit trails are thoroughly tested
**And** All tests must pass for Epic 6 completion
```

---

### Epic 7: Legacy Data Migration

**New Story:** Story 7.7 - Integration Tests - Legacy Data Import Pipeline

**What Changes:**
- Add integration test story as last story in Epic 7
- Tests cover: XML parsing, data mapping, import preview, execution, audit
- Tests organized in `apps/backend/tests/Integration/Import/`
- **Note:** Integration tests (not E2E UI) because this is CLI tooling

**OLD:**
```markdown
### Story 7.6: Import Re-run Capability
... [existing content] ...
```

**NEW:**
```markdown
### Story 7.6: Import Re-run Capability
... [existing content] ...

---

### Story 7.7: Integration Tests - Legacy Data Import Pipeline

**As a** developer,
**I want** integration tests covering the legacy XML/XSD import pipeline,
**So that** data migration accuracy and error handling are validated.

**Acceptance Criteria:**

**Given** the backend project with import infrastructure
**When** I run integration tests for data migration
**Then** the following scenarios are covered:

**XML/XSD Parsing:**
- Parser reads and validates XML against XSD schema
- Parser extracts School, ClassGroup, Activity, and Children data
- Parser handles encoding and format variations
- Parser errors are logged with file/line information

**Data Mapping:**
- Legacy fields map correctly to new entity properties
- Data transformations apply correctly (date formats, enum conversions)
- Missing or invalid values are flagged for review
- Mapping rules are enforced

**Import Preview Mode:**
- Preview mode shows total records to import by entity type
- Sample mapped records display correctly
- Validation warnings and errors are captured
- NO data is written to database in preview mode

**Import Execution:**
- Valid records insert into database correctly
- Failed records log to exceptions file with details
- Summary shows: X imported, Y failed, Z skipped
- Exceptions file includes: record ID, field, error reason, original value

**Import Audit Log:**
- Import run creates audit log entry with timestamp, user, source files, counts
- Audit log is queryable for import history
- All import operations are traceable

**Re-run Capability:**
- Existing records matched by unique key (legacy ID)
- Update mode: existing records updated with new values
- Changes logged to audit trail
- Conflict modes work correctly (skip/update/fail)

**And** Integration tests are organized in `apps/backend/tests/Integration/Import/`
**And** Tests use sample XML files from `docs/legacy/`
**And** Database is reset between test runs
**And** All tests must pass for Epic 7 completion
**Note:** No E2E UI tests needed - this is developer CLI tooling
```

---

### Architecture Document Updates

**New Section:** E2E Testing Strategy (to be added to Architecture.md)

**Where:** After "Project Structure & Boundaries" section, before "Architecture Validation Results"

**Content:**
```markdown
## E2E Testing Strategy

### Testing Philosophy

E2E tests validate complete user workflows and critical integration points that unit tests cannot cover. Each epic includes dedicated E2E testing stories to ensure feature integrity before marking epic completion.

### Test Scope by Epic

**Epic 1 - Authentication:**
- Login/logout flows
- Protected route redirects
- Auth token handling

**Epic 2 - Trucks & Schools:**
- CRUD workflows for reference data
- Validation and error handling
- Data persistence

**Epic 3 - Class Groups & Scheduling:**
- Schedule configuration
- Conflict detection (FR6)
- Weekly view rendering

**Epic 4 - Students & Families:**
- Global search (FR11) with disambiguation
- Student profile navigation (FR10)
- CRUD operations with related entities
- Performance targets (NFR1, NFR2)

**Epic 5 - Attendance & Evaluations:**
- Attendance tracking and bulk entry
- Audit trail creation and display (FR14)
- Evaluation recording

**Epic 6 - Billing & Financials:**
- Invoice and payment workflows
- Balance calculations
- Audit trail for financial records (FR14)

**Epic 7 - Data Migration:**
- Integration tests for import pipeline
- XML parsing and data mapping
- Import preview and execution

### Test Organization

**Frontend E2E Tests:**
```
apps/frontend/e2e/
├── auth/
│   ├── login.spec.ts
│   ├── logout.spec.ts
│   └── protected-routes.spec.ts
├── trucks-schools/
│   ├── trucks-crud.spec.ts
│   └── schools-crud.spec.ts
├── class-groups/
│   ├── scheduling.spec.ts
│   └── conflict-detection.spec.ts
├── students/
│   ├── global-search.spec.ts
│   ├── student-profile.spec.ts
│   └── family-management.spec.ts
├── attendance-evaluations/
│   ├── attendance.spec.ts
│   ├── bulk-attendance.spec.ts
│   └── audit-trail.spec.ts
└── billing/
    ├── invoicing.spec.ts
    ├── payments.spec.ts
    └── financial-calculations.spec.ts
```

**Backend Integration Tests:**
```
apps/backend/tests/Integration/Import/
├── xml-parsing.spec.ts
├── data-mapping.spec.ts
├── import-preview.spec.ts
├── import-execution.spec.ts
└── import-audit.spec.ts
```

### Test Data Strategy

**Test Data Seeding:**
- Dedicated test database (SQLite in-memory or separate file)
- Seed scripts create baseline data for each test suite
- Tests run in isolation; database reset between tests
- Seed data includes:
  - Admin user credentials
  - Sample schools, trucks, class groups
  - Sample students with families
  - Existing attendance/evaluations/billing records

**Test Data Fixtures:**
```typescript
// e2e/fixtures/test-data.ts
export const testFixtures = {
  schools: [...],    // 3-5 sample schools
  trucks: [...],     // 2 sample trucks
  classGroups: [...], // 5-10 sample class groups
  students: [...],   // 10-15 sample students
  families: [...],   // Related family records
};
```

**Clean Up Strategy:**
- Each test suite cleans up its own data
- Transactions rolled back after each test
- No persistent test data in main database

### Test Environment Configuration

**Playwright Configuration:**
```typescript
// playwright.config.ts
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { browserName: 'chromium' },
    },
  ],
  webServer: {
    command: 'npm run dev',
    port: 4200,
    reuseExistingServer: !process.env.CI,
  },
});
```

### CI/CD Integration

**Test Execution:**
- E2E tests run on every pull request
- Tests run in parallel for speed
- Failed tests block merge
- Test reports uploaded for review

**Pre-merge Requirements:**
- All E2E tests must pass
- Code coverage maintained (target: 80%+)
- Performance tests validate <2s targets

### Epic Completion Criteria

**Each Epic is COMPLETE only when:**
1. All feature stories are implemented
2. All E2E/integration tests for the epic pass
3. Critical user journeys are validated
4. Performance targets are met (where applicable)
5. No regressions in existing functionality

### Quality Gates

**Definition of Done for E2E Tests:**
- [ ] Tests cover all happy paths for epic features
- [ ] Tests cover critical edge cases and error scenarios
- [ ] Tests validate audit trail creation (where applicable)
- [ ] Tests use seeded data and clean up after themselves
- [ ] Tests run reliably in CI (no flakiness)
- [ ] Test reports provide clear failure diagnostics
```

---

## Section 5: Implementation Handoff

### Change Scope Classification

**Scope:** Moderate

**Rationale:**
- Adds 7 new testing stories (no feature scope changes)
- Requires backlog reorganization (stories added to epics)
- Requires architecture documentation update
- No fundamental replan needed

### Handoff Plan

**Primary Recipients:** Development Team + Product Owner

**Responsibilities:**

**Development Team:**
- Implement E2E/integration test stories as part of each epic
- Create test data fixtures and seeding utilities
- Ensure tests pass before marking epic complete
- Maintain test reliability and fix flaky tests

**Product Owner:**
- Update epics.md with new story content
- Adjust sprint planning to accommodate test creation
- Update epic completion criteria to include test passing
- Track test coverage and quality metrics

**Scrum Master:**
- Reorganize backlog to include test stories
- Ensure testing effort is estimated and planned
- Monitor test execution and failure rates
- Facilitate test review and refinement sessions

### Implementation Sequence

**Recommended Order:**
1. **Sprint 1-2:** Implement Epic 1 E2E tests (establishes auth baseline)
2. **Sprint 3-4:** Implement Epic 2 E2E tests (establishes CRUD patterns)
3. **Sprint 5-6:** Implement Epic 3 E2E tests (validates critical conflict detection)
4. **Sprint 7-9:** Implement Epic 4 E2E tests (validates primary user journey)
5. **Sprint 10-11:** Implement Epic 5 E2E tests (validates audit trails)
6. **Sprint 12-13:** Implement Epic 6 E2E tests (validates financial integrity)
7. **Sprint 14-15:** Implement Epic 7 integration tests (validates import pipeline)

**Parallel Development:**
- Test stories can be developed in parallel with feature stories
- Tests written as features are completed
- Test execution integrated into daily CI/CD

### Success Criteria

**Epic Level:**
- ✅ All E2E/integration tests for epic pass consistently
- ✅ Critical user workflows validated
- ✅ Performance targets met (where applicable)
- ✅ No regressions in existing functionality

**Project Level:**
- ✅ All 7 epics have passing test suites
- ✅ Overall test coverage ≥80%
- ✅ CI/CD pipeline runs all tests automatically
- ✅ Test reports provide clear failure diagnostics
- ✅ Flaky test rate <5%

**Quality Metrics:**
- Test reliability: >95% pass rate on CI
- Test execution time: <10 minutes for full suite
- Test maintenance: <2 hours per week for updates
- Regression detection: All critical bugs caught by tests

---

## Section 6: Approval & Next Steps

### Review Checklist

- [x] All epic impacts documented
- [x] Artifact conflicts identified and resolved
- [x] Detailed change proposals created for all 7 epics
- [x] Architecture updates specified
- [x] Implementation handoff plan defined
- [x] Success criteria established
- [x] Timeline impact assessed (+1-2 days per epic)

### User Approval Required

**Question:** Do you approve this Sprint Change Proposal for implementation?

**Options:**
- **Yes** - Proceed with implementing all 7 test stories and architecture updates
- **No** - Provide specific feedback on what needs adjustment
- **Revise** - Request changes to specific proposals

### After Approval

**Immediate Next Steps:**
1. Update `epics.md` with all 7 new stories
2. Update `architecture.md` with E2E testing strategy section
3. Reorganize backlog to include test stories in each epic
4. Adjust sprint planning to accommodate test creation effort
5. Begin implementation with Epic 1 E2E tests

**Tools/Scripts to Create:**
- Test data seeding utilities
- E2E test templates and fixtures
- CI/CD integration for test execution
- Test reporting and coverage tracking

---

## Appendix: Quick Reference

### New Stories Summary

| Story ID | Epic | Type | Test Coverage | Priority |
|----------|------|------|---------------|----------|
| 1.6 | Auth | E2E | Login, logout, protected routes | HIGH |
| 2.6 | Trucks/Schools | E2E | CRUD workflows, validation | HIGH |
| 3.6 | Class Groups | E2E | Scheduling, conflicts (FR6) | HIGH |
| 4.9 | Students | E2E | Search (FR11), profile (FR10) | CRITICAL |
| 5.7 | Attendance | E2E | Tracking, audit trail (FR14) | HIGH |
| 6.7 | Billing | E2E | Invoicing, payments, audit (FR14) | HIGH |
| 7.7 | Migration | Integration | Import pipeline, XML parsing | MEDIUM |

### Effort Estimate

| Epic | Feature Stories | Test Stories | Total Effort | Test Overhead |
|------|----------------|--------------|--------------|---------------|
| Epic 1 | 5 | +1 | ~6 stories | +17% |
| Epic 2 | 5 | +1 | ~6 stories | +17% |
| Epic 3 | 5 | +1 | ~6 stories | +17% |
| Epic 4 | 8 | +1 | ~9 stories | +11% |
| Epic 5 | 6 | +1 | ~7 stories | +14% |
| Epic 6 | 6 | +1 | ~7 stories | +14% |
| Epic 7 | 6 | +1 | ~7 stories | +14% |
| **TOTAL** | **41** | **+7** | **48 stories** | **+15% avg** |

**Timeline Impact:** ~+15% overall project duration (quality investment that prevents costly rework)

---

**End of Sprint Change Proposal**

**Document Location:** `_bmad-output/planning-artifacts/sprint-change-proposal-2026-01-05.md`

**Status:** ✅ APPROVED - Implementation Ready
**Changes Applied:**
- ✅ epics.md updated: 38 stories → 45 stories (+7 test stories)
- ✅ architecture.md updated: Added E2E Testing Strategy section
- ✅ Sprint Change Proposal approved and archived
