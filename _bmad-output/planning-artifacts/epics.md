---
stepsCompleted:
  - 'step-01-validate-prerequisites'
  - 'step-02-design-epics'
  - 'step-03-create-stories'
  - 'step-04-final-validation'
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/architecture.md"
  - "_bmad-output/planning-artifacts/ux-design-specification.md"
workflowType: 'epics-and-stories'
project_name: 'kcow-online'
user_name: 'Joe'
date: '2026-01-03'
status: 'complete'
completedAt: '2026-01-03'
totalEpics: 9
totalStories: 58
---

# kcow-online - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for kcow-online, decomposing the requirements from the PRD, UX Design, and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

**Trucks and Fleet Scheduling**
- FR1: Admin can manage truck records.

**Schools Management**
- FR2: Admin can create, view, edit, and archive schools.
- FR3: Admin can manage school contacts and billing settings.

**Class Groups and Scheduling**
- FR4: Admin can create, view, edit, and archive class groups.
- FR5: Admin can set class group schedules (day, time, sequence) and assign trucks.
- FR6: Admin can detect and resolve scheduling conflicts before saving changes.

**Student Management**
- FR7: Admin can create, view, edit, and archive student records.
- FR8: Admin can assign a student to a school, class group, and seat.
- FR9: Admin can manage student family/guardian contacts and relationships.
- FR10: Admin can view a single-screen student profile that includes demographics, attendance, evaluation, and billing details.
- FR11: Admin can search students by name, school, grade, and class group with unambiguous results.

**Data Migration (Dev-Only)**
- FR12: Developer can run migration/import tooling to load legacy data into the system.

**Data Integrity and Auditability**
- FR13: Admin can see validation errors before saving invalid data.
- FR14: Admin can view an audit trail for changes to attendance and billing records.

### NonFunctional Requirements

**Performance**
- NFR1: Student search results return in under 2 seconds.
- NFR2: Student profile and core workflow pages load in under 2 seconds.
- NFR3: All primary admin actions complete within 2 seconds under normal office-network conditions.

**Security**
- NFR4: Student and guardian PII is protected in line with POPIA requirements.
- NFR5: Billing and contact data is treated as sensitive data.
- NFR6: Access is restricted to authorized admin users only.

**Reliability**
- NFR7: System availability supports no more than 1 hour of downtime per month.
- NFR8: Critical admin workflows remain usable during standard operating hours.

**Accessibility**
- NFR9: Basic accessibility: keyboard operability for form fields, visible focus states, and readable typography.

**Integration**
- NFR10: No external integrations required for v1.

### Additional Requirements

**🚨 Foundational Setup Requirements (MUST BE FIRST):**

These setup tasks are prerequisites for all feature development and must be completed first:

1. **Backend Base Setup**: Create ASP.NET Core Web API project (net10.0) in `apps/backend/` with:
   - Clean Architecture folder structure (Api, Application, Domain, Infrastructure)
   - Dapper + SQLite database configuration with repository pattern
   - DbUp migration runner with versioned SQL scripts
   - ASP.NET Core JWT Bearer Authentication
   - ProblemDetails error handling middleware
   - Serilog logging configuration
   - CORS configuration for frontend

2. **Frontend Auth Integration**: Connect to backend JWT authentication:
   - Create AuthService wrapper using Angular Signals
   - Implement login/logout flows against API endpoints
   - Add auth guard for protected routes
   - Add HTTP interceptor for JWT tokens

---

**From Architecture:**

- **Starter Template**: Existing Angular 21 scaffold in `apps/frontend/` is retained (no re-initialization required).
- **Database**: SQLite for v1, migration path to PostgreSQL post-MVP.
- **Data Access**: Dapper with repository pattern; entities in Domain layer, repositories in Infrastructure/Repositories.
- **Migrations**: DbUp with versioned SQL scripts in Infrastructure/Migrations/Scripts.
- **Authorization**: Role-based checks at API endpoints (Admin role).
- **API Style**: RESTful JSON endpoints with plural kebab-case paths (e.g., `/students`, `/class-groups`).
- **Frontend State**: Angular Signals for UI state; RxJS for async flows.
- **Forms**: Reactive Forms only with shared validators.
- **Routing**: Lazy-loaded feature routes under AdminLayout shell.
- **Performance**: OnPush change detection throughout.
- **Legacy Import**: XML/XSD import workflow with preview, exception handling, and audit log.

**Implementation Sequence (Updated):**
1. **Backend base setup** + Dapper + DbUp + JWT Authentication server-side.
2. **Frontend Auth integration** (login/logout, guards, interceptors).
3. Legacy import workflow + audit log.
4. Core API endpoints (students, families, class groups, schools).
5. Frontend shell + student profile + search flow.
6. Reporting + dashboards.

**From UX Design:**

- **Global Student Search**: Persistent search bar in top navigation with typeahead, name/school/grade disambiguation.
- **Single-Screen Student Profile**: Three-column grid layout with tabbed sub-sections (Child Info, Financial, Attendance, Evaluation).
- **Inline Editing**: Fast updates without leaving the screen for attendance, billing, and evaluation.
- **Status Chips**: Consistent visual indicators for attendance/billing/evaluation status.
- **Audit Trail Panel**: Visible traceability for corrections in attendance and billing.
- **Family Grid**: Show sibling context and linked records at bottom of student profile.
- **Schedule Conflict Banner**: Visual warning when scheduling conflicts are detected.
- **No Modals**: Use inline confirmation rows or drawer panels instead of modal dialogs.
- **Desktop-First**: Optimize for keyboard-first desktop workflows; tablet/mobile not primary targets.
- **Design System**: Tailwind CSS + DaisyUI with dark theme default.
- **Density**: Comfortable spacing with 8px scale while maintaining data-rich screens.

### FR Coverage Map

| FR | Epic | Description |
|----|------|-------------|
| FR1 | Epic 2 | Truck management |
| FR2 | Epic 2 | Schools CRUD |
| FR3 | Epic 2 | School contacts and billing settings |
| FR4 | Epic 3 | Class groups CRUD |
| FR5 | Epic 3 | Class group schedules and truck assignment |
| FR6 | Epic 3 | Scheduling conflict detection |
| FR7 | Epic 4 | Student records CRUD |
| FR8 | Epic 4 | Student assignment to school/class group/seat |
| FR9 | Epic 4 | Family/guardian contact management |
| FR10 | Epic 4 | Single-screen student profile |
| FR11 | Epic 4 | Student search |
| FR12 | Epic 7 | Legacy data migration |
| FR13 | Epic 4 | Validation errors before save |
| FR14 | Epic 5 & 6 | Audit trail for attendance and billing |

## Epic List

### Epic 1: Project Foundation & Authentication
Admin can securely log in to the system and access a protected dashboard with the core application shell.

**FRs covered:** NFR4, NFR5, NFR6 (Security), NFR9 (Accessibility baseline)

**Implementation Notes:**
- Backend scaffold (ASP.NET Core + Dapper + DbUp + SQLite)
- JWT Bearer Authentication (server + client)
- AdminLayout shell with sidebar navigation
- Basic dashboard placeholder
- Auth guards, interceptors, theme system

---

### Epic 2: Trucks & Schools Management
Admin can manage the core reference data - schools and trucks - that underpin all scheduling and student operations.

**FRs covered:** FR1, FR2, FR3

**Implementation Notes:**
- Trucks CRUD with fleet status
- Schools CRUD with contacts and billing settings
- API endpoints + frontend features
- Validation and error handling patterns established

---

### Epic 3: Class Groups & Scheduling
Admin can create and manage class group schedules, assign trucks to schools, and detect/resolve scheduling conflicts.

**FRs covered:** FR4, FR5, FR6

**Implementation Notes:**
- Class groups CRUD
- Schedule management (day/time/sequence)
- Truck assignment to class groups
- Conflict detection and resolution UI
- Schedule Conflict Banner (UX requirement)

---

### Epic 4: Student & Family Management
Admin can create, search, and manage student records with family/guardian relationships, and view the single-screen student profile.

**FRs covered:** FR7, FR8, FR9, FR10, FR11, FR13

**Implementation Notes:**
- Students CRUD with assignment to school/class group/seat
- Global Student Search with typeahead and disambiguation
- Family/guardian contact management
- Single-screen Student Profile (3-column layout, tabbed sections)
- Family Grid component
- Inline validation (FR13)

---

### Epic 5: Attendance & Evaluations
Admin can track student attendance per class session, record progress evaluations, and make corrections with a visible audit trail.

**FRs covered:** FR14 (audit trail for attendance)

**Implementation Notes:**
- Attendance tracking per class group session
- Evaluation/progress recording (activity scores, speed/accuracy)
- Inline editing in student profile tabs
- Audit Trail Panel for corrections
- Status chips for attendance/evaluation state

---

### Epic 6: Billing & Financials
Admin can manage student billing records, track payments, and view financial status with audit trail.

**FRs covered:** FR14 (audit trail for billing)

**Implementation Notes:**
- Billing tab in student profile
- Invoice, receipt, payment tracking
- Balance per student
- Audit trail for billing changes
- Inline edit for billing updates

---

### Epic 7: Legacy Data Migration
Developer can import legacy XML/XSD data with preview, exception handling, and audit logging.

**FRs covered:** FR12

**Implementation Notes:**
- XML/XSD import pipeline (dev-only CLI/tooling)
- Preview imports before commit
- Exception handling and rejected record tracking
- Audit log for import operations
- Re-run capability

---

### Epic 8: Activity Management
Admin can manage the educational program catalog (activities) that are delivered during class sessions and evaluated in student progress tracking.

**FRs covered:** Foundation for Epic 5 (Evaluations - Activity reference)

**Implementation Notes:**
- Activity CRUD with program code, grade level, folder, and curriculum info
- Icon field: OLE Object from legacy XML stored as base64 string
- API endpoints + frontend feature module
- Legacy data migration from Activity XML/XSD (11.8MB with embedded images)
- Simple maintenance entity following Epic 2 patterns
- **Prerequisite for Epic 5** (Evaluations depend on Activity entity)

---

## Epic 0: Infrastructure & Architectural Foundation

**Goal:** Handle architectural changes and infrastructure migrations that span multiple features.

**Rationale:** This epic provides a dedicated space for cross-cutting infrastructure work that doesn't fit neatly into feature epics. It allows architectural decisions to be implemented without disrupting feature-focused sprint planning.

**Stories:**
- **0.1:** Migrate Data Access from EF Core to Dapper + DbUp

---

### Story 0.1: Migrate Data Access from EF Core to Dapper + DbUp

**Priority:** High (Blocking - should be completed before new backend stories)

**Description:**
As a developer, I want to migrate the data access layer from Entity Framework Core to Dapper with DbUp migrations, so that I have explicit SQL control, simpler debugging, and reduced ORM complexity.

**Acceptance Criteria:**

**Given** the existing EF Core implementation
**When** the migration is complete
**Then** all existing functionality continues to work with the new data access layer

**Backend Changes:**

**And** `Infrastructure/Data/` is replaced with:
- `Infrastructure/Repositories/` - Dapper repository implementations
- `Infrastructure/Database/` - IDbConnectionFactory, DbUp bootstrapper
- `Infrastructure/Sql/` - Complex SQL queries as constants
- `Infrastructure/Migrations/Scripts/` - Versioned SQL scripts

**And** `AppDbContext` is removed and replaced with `IDbConnectionFactory`
**And** all entity configurations are converted to DbUp SQL migration scripts
**And** repository pattern is implemented with interfaces in `Application/` and implementations in `Infrastructure/Repositories/`
**And** existing EF Core migrations are converted to DbUp scripts with naming: `YYYYMMDD_NN_Description.sql`

**Repository Implementation:**

**And** each entity has a corresponding repository:
- `ITruckRepository` / `TruckRepository`
- `ISchoolRepository` / `SchoolRepository`
- `IClassGroupRepository` / `ClassGroupRepository`
- `IStudentRepository` / `StudentRepository`
- `IFamilyRepository` / `FamilyRepository`
- `IActivityRepository` / `ActivityRepository`
- `IAttendanceRepository` / `AttendanceRepository` (if implemented)
- `IEvaluationRepository` / `EvaluationRepository` (if implemented)
- `IBillingRepository` / `BillingRepository` (if implemented)

**And** repositories use Dapper's `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`
**And** all queries use parameterized SQL (no string concatenation)

**Migration Scripts:**

**And** DbUp is configured to run on application startup in Development
**And** existing database schema is preserved (data migration not required)
**And** migration scripts are idempotent where possible

**Testing:**

**And** all existing API endpoints continue to work
**And** all existing integration tests pass
**And** all existing E2E tests pass

**Technical Notes:**
- Reference: Architecture document "Data Access Patterns" section
- NuGet packages needed: `Dapper`, `DbUp`, `DbUp.SQLite`
- Connection string configuration remains in `appsettings.json`

---

## Epic 1: Project Foundation & Authentication

Admin can securely log in to the system and access a protected dashboard with the core application shell.

### Story 1.1: Backend Project Scaffold

**As a** developer,
**I want** an ASP.NET Core Web API project with Clean Architecture structure,
**So that** I have a properly organized codebase to build features upon.

**Acceptance Criteria:**

**Given** the project root at `apps/backend/`
**When** the scaffold is created
**Then** the following structure exists:
- `src/Api/` with Program.cs and Controllers folder
- `src/Application/` for business logic
- `src/Domain/` for entities
- `src/Infrastructure/` for data access and external services
- `tests/Unit/` and `tests/Integration/` for testing

**And** Dapper with SQLite is configured with IDbConnectionFactory
**And** DbUp migration runner is configured with initial schema script
**And** Serilog logging is configured
**And** ProblemDetails error handling middleware is added
**And** CORS is configured to allow the frontend origin
**And** the API runs successfully with `dotnet run`

---

### Story 1.2: JWT Authentication Integration

**As a** developer,
**I want** JWT authentication configured on the backend API,
**So that** the system can authenticate admin users securely.

**Acceptance Criteria:**

**Given** the backend scaffold from Story 1.1
**When** JWT authentication is integrated
**Then** the Admin role and user are seeded in the database
**And** `/api/auth/login` endpoint accepts credentials and returns a token
**And** `/api/auth/logout` endpoint invalidates the session
**And** protected endpoints return 401 for unauthenticated requests
**And** protected endpoints return 403 for unauthorized roles

---

### Story 1.3: Frontend Auth Service & Login Page

**As an** admin,
**I want** a login page to authenticate with my credentials,
**So that** I can securely access the admin system.

**Acceptance Criteria:**

**Given** I am on the login page (`/login`)
**When** I enter valid admin credentials and submit
**Then** I am redirected to the dashboard
**And** my session is persisted (token stored)

**Given** I enter invalid credentials
**When** I submit the form
**Then** I see an inline error message
**And** I remain on the login page

**Given** I am authenticated
**When** I click logout
**Then** my session is cleared
**And** I am redirected to the login page

**And** AuthService uses Angular Signals for state
**And** HTTP interceptor attaches auth token to API requests
**And** Auth guard redirects unauthenticated users to login

---

### Story 1.4: Admin Layout Shell & Dashboard

**As an** admin,
**I want** a dashboard with sidebar navigation after login,
**So that** I can navigate to different modules of the system.

**Acceptance Criteria:**

**Given** I am authenticated
**When** I access the root path (`/`)
**Then** I see the AdminLayout with:
- Sidebar navigation with module links (Dashboard, Trucks, Schools, Class Groups, Students, Activities, Billing)
- Top navbar with user info and logout button
- Main content area showing the Dashboard

**Given** I am on the dashboard
**When** I view the page
**Then** I see a placeholder welcome message and quick stats area

**And** the layout uses DaisyUI components with dark theme
**And** sidebar is persistent on desktop
**And** keyboard navigation works for all interactive elements (NFR9)

---

### Story 1.5: Theme System & Accessibility Baseline

**As an** admin,
**I want** consistent theming and accessible focus states,
**So that** the UI is readable and keyboard-navigable.

**Acceptance Criteria:**

**Given** the application loads
**When** I view any page
**Then** the DaisyUI dark theme is applied consistently

**Given** I use keyboard navigation
**When** I tab through interactive elements
**Then** visible focus rings appear on buttons, inputs, and links

**And** form fields have proper labels and error states
**And** contrast ratios meet basic readability standards

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

---

## Epic 2: Trucks & Schools Management

Admin can manage the core reference data - trucks and schools - that underpin all scheduling and student operations.

### Story 2.1: Truck Entity & API Endpoints

**As a** developer,
**I want** the Truck domain entity and REST API endpoints,
**So that** truck data can be managed through the API.

**Acceptance Criteria:**

**Given** the backend project from Epic 1
**When** the Truck entity is created
**Then** the `Truck` entity exists in `Domain/Entities` with properties:
- Id, Name, RegistrationNumber, Status, Notes

**And** TruckRepository exists in `Infrastructure/Repositories`
**And** DbUp migration script creates the `trucks` table
**And** `/api/trucks` endpoints support:
- GET (list all trucks)
- GET `/:id` (get single truck)
- POST (create truck)
- PUT `/:id` (update truck)
- DELETE `/:id` (archive/soft-delete truck)

**And** all endpoints return ProblemDetails on error
**And** endpoints require authentication

---

### Story 2.2: Trucks Management UI

**As an** admin,
**I want** a trucks management page to view and manage fleet records,
**So that** I can maintain accurate truck information.

**Acceptance Criteria:**

**Given** I am authenticated and click "Trucks" in the sidebar
**When** the Trucks page loads
**Then** I see a table listing all trucks with Name, Registration, Status columns
**And** I see "Add Truck" button

**Given** I click "Add Truck"
**When** the create form appears
**Then** I can enter truck details and save
**And** the new truck appears in the list

**Given** I click a truck row
**When** the edit form appears
**Then** I can update truck details and save
**And** I see a success confirmation

**Given** I click delete on a truck
**When** I confirm the action
**Then** the truck is archived (soft-deleted) and removed from the active list

**And** validation errors display inline
**And** loading states are shown during API calls

---

### Story 2.3: School Entity & API Endpoints

**As a** developer,
**I want** the School domain entity and REST API endpoints,
**So that** school data can be managed through the API.

**Acceptance Criteria:**

**Given** the backend project
**When** the School entity is created
**Then** the `School` entity exists with properties:
- Id, Name, Address, ContactName, ContactPhone, ContactEmail, BillingSettings (JSON or relation), IsActive, Notes

**And** SchoolRepository and DbUp migration script create the `schools` table
**And** `/api/schools` endpoints support:
- GET (list schools)
- GET `/:id` (single school with contacts)
- POST (create school)
- PUT `/:id` (update school)
- DELETE `/:id` (archive school)

**And** endpoints require authentication
**And** ProblemDetails errors are returned for validation failures (FR13)

---

### Story 2.4: Schools Management UI

**As an** admin,
**I want** a schools management page to view and edit school records,
**So that** I can maintain school information and contacts.

**Acceptance Criteria:**

**Given** I am authenticated and click "Schools" in sidebar
**When** the Schools page loads
**Then** I see a table listing schools with Name, Contact, Address columns

**Given** I click "Add School"
**When** the create form appears
**Then** I can enter school details including contact info
**And** the new school appears in the list after save

**Given** I click a school row
**When** the edit form appears
**Then** I can update school details (FR2) and manage contacts/billing settings (FR3)
**And** changes are saved and confirmed

**And** form validation prevents invalid data (FR13)
**And** inline errors display for failed validation

---

### Story 2.5: School Billing Settings

**As an** admin,
**I want** to configure billing settings per school,
**So that** student billing calculations use the correct rates.

**Acceptance Criteria:**

**Given** I am editing a school
**When** I view the billing settings section
**Then** I can configure:
- Default rate per session
- Billing cycle (monthly/termly)
- Any school-specific billing notes

**Given** I save billing settings
**When** the form submits
**Then** the settings are persisted with the school record
**And** I see a success confirmation

**And** this fulfills FR3 (manage school billing settings)

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

---

## Epic 3: Class Groups & Scheduling

Admin can create and manage class group schedules, assign trucks to schools, and detect/resolve scheduling conflicts.

### Story 3.1: Class Group Entity & API Endpoints

**As a** developer,
**I want** the ClassGroup domain entity and REST API endpoints,
**So that** class group data can be managed through the API.

**Acceptance Criteria:**

**Given** the backend project with School and Truck entities
**When** the ClassGroup entity is created
**Then** the `ClassGroup` entity exists with properties:
- Id, Name, SchoolId (FK), TruckId (FK), DayOfWeek, StartTime, EndTime, Sequence, IsActive, Notes

**And** ClassGroupRepository with methods handling School and Truck relationships
**And** DbUp migration script creates the `class_groups` table with foreign keys
**And** `/api/class-groups` endpoints support:
- GET (list class groups with optional school/truck filters)
- GET `/:id` (single class group with school and truck details)
- POST (create class group)
- PUT `/:id` (update class group)
- DELETE `/:id` (archive class group)

**And** endpoints require authentication
**And** ProblemDetails errors for validation failures

---

### Story 3.2: Class Groups Management UI

**As an** admin,
**I want** a class groups management page to view and manage class schedules,
**So that** I can organize school visits and truck assignments (FR4).

**Acceptance Criteria:**

**Given** I am authenticated and click "Class Groups" in sidebar
**When** the Class Groups page loads
**Then** I see a table listing class groups with Name, School, Truck, Day, Time columns
**And** I can filter by school or truck

**Given** I click "Add Class Group"
**When** the create form appears
**Then** I can enter class group details
**And** select a school from a dropdown
**And** select a truck from a dropdown
**And** set day of week and time slot

**Given** I add or edit a class group
**When** I save the form
**Then** the class group is created/updated
**And** I see a success confirmation

---

### Story 3.3: Schedule Time Slot Configuration

**As an** admin,
**I want** to set class group schedules with day, time, and sequence,
**So that** I can organize the weekly school visit calendar (FR5).

**Acceptance Criteria:**

**Given** I am creating or editing a class group
**When** I configure the schedule
**Then** I can select:
- Day of week (Monday-Friday dropdown)
- Start time and end time
- Sequence number (order within the day)

**Given** I assign a truck to the class group
**When** I save
**Then** the truck assignment is persisted
**And** the schedule appears in a weekly view or list

**And** this fulfills FR5 (set class group schedules and assign trucks)

---

### Story 3.4: Scheduling Conflict Detection

**As an** admin,
**I want** the system to detect scheduling conflicts before I save,
**So that** I don't accidentally double-book a truck (FR6).

**Acceptance Criteria:**

**Given** I am creating or editing a class group
**When** I select a truck, day, and time that overlaps with an existing class group
**Then** a Schedule Conflict Banner appears warning me of the conflict
**And** the conflicting class group details are shown

**Given** a conflict is detected
**When** I try to save
**Then** I am blocked from saving until the conflict is resolved
**And** I can adjust the time or truck to resolve

**Given** I resolve the conflict
**When** I save
**Then** the class group is saved successfully
**And** no conflict warning appears

**And** this fulfills FR6 (detect and resolve scheduling conflicts)

---

### Story 3.5: Weekly Schedule Overview

**As an** admin,
**I want** a visual weekly schedule view showing all class groups,
**So that** I can see truck utilization and school coverage at a glance.

**Acceptance Criteria:**

**Given** I am on the Class Groups page
**When** I switch to "Weekly View" tab
**Then** I see a calendar-style grid with days as columns and time slots as rows
**And** class groups are displayed as blocks on the grid

**Given** I click on a schedule block
**When** the detail appears
**Then** I see the class group name, school, and truck
**And** I can navigate to edit the class group

**And** conflicts are visually highlighted with a warning indicator

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

---

## Epic 4: Student & Family Management

Admin can create, search, and manage student records with family/guardian relationships, and view the single-screen student profile.

### Story 4.1: Student Entity & API Endpoints

**As a** developer,
**I want** the Student domain entity and REST API endpoints,
**So that** student data can be managed through the API (FR7).

**Acceptance Criteria:**

**Given** the backend project with School and ClassGroup entities
**When** the Student entity is created
**Then** the `Student` entity exists with properties:
- Id, FirstName, LastName, DateOfBirth, Grade, SchoolId (FK), ClassGroupId (FK), SeatNumber, IsActive, Notes
- Additional fields from legacy XSD: Gender, Language, etc.

**And** StudentRepository with methods handling foreign key relationships
**And** DbUp migration script creates the `students` table
**And** `/api/students` endpoints support:
- GET (list with pagination and filters)
- GET `/:id` (single student with school, class group, family details)
- POST (create student)
- PUT `/:id` (update student)
- DELETE `/:id` (archive student)

**And** validation returns ProblemDetails errors (FR13)

---

### Story 4.2: Family Entity & API Endpoints

**As a** developer,
**I want** the Family domain entity and relationship endpoints,
**So that** guardian/contact data can be managed and linked to students (FR9).

**Acceptance Criteria:**

**Given** the backend project
**When** the Family entity is created
**Then** the `Family` entity exists with properties:
- Id, FamilyName, PrimaryContactName, Phone, Email, Address, Notes

**And** DbUp migration creates `student_family` join table linking students to families with relationship type
**And** FamilyRepository handles family queries and student-family links
**And** `/api/families` endpoints support CRUD
**And** `/api/students/:id/families` returns families linked to a student
**And** POST `/api/students/:id/families` links a family to a student

---

### Story 4.3: Student Assignment to School & Class Group

**As an** admin,
**I want** to assign a student to a school, class group, and seat,
**So that** the student's schedule and location are tracked (FR8).

**Acceptance Criteria:**

**Given** I am creating or editing a student
**When** I view the assignment section
**Then** I can select a school from a dropdown
**And** I can select a class group (filtered by school)
**And** I can enter a seat number

**Given** I save the student with assignments
**When** the form submits
**Then** the assignments are persisted
**And** the student appears in the class group roster

**And** changing school clears the class group selection (validation)

---

### Story 4.4: Global Student Search

**As an** admin,
**I want** a global search bar to quickly find students by name, school, or grade,
**So that** I can locate any student within seconds (FR11).

**Acceptance Criteria:**

**Given** I am on any page in the admin layout
**When** I type in the global search bar
**Then** typeahead results appear showing matching students
**And** each result shows: Full Name, School, Grade, Class Group

**Given** there are students with similar names
**When** I view the results
**Then** disambiguation is clear (showing school/grade) to select the correct student

**Given** I select a search result
**When** I click on it
**Then** I navigate to the student's profile page

**And** search returns results in under 2 seconds (NFR1)
**And** "No results found" appears when no matches exist

---

### Story 4.5: Students List Page

**As an** admin,
**I want** a students list page to browse and filter all students,
**So that** I can find students by various criteria.

**Acceptance Criteria:**

**Given** I click "Students" in the sidebar
**When** the Students page loads
**Then** I see a paginated table with Name, School, Grade, Class Group columns
**And** I can filter by school and class group

**Given** I click "Add Student"
**When** the create form appears
**Then** I can create a new student record

**Given** I click on a student row
**When** I navigate
**Then** I am taken to the student profile page

---

### Story 4.6: Single-Screen Student Profile Layout

**As an** admin,
**I want** a single-screen student profile with all key information,
**So that** I can view and update a student without navigating away (FR10).

**Acceptance Criteria:**

**Given** I navigate to a student profile (`/students/:id`)
**When** the page loads
**Then** I see a 3-column summary header with:
- Column 1: Photo placeholder, name, basic demographics
- Column 2: School and class group assignment
- Column 3: Quick status indicators (attendance, billing)

**And** below the header, I see tabbed sections for:
- Child Info (demographics details)
- Financial (billing - placeholder for Epic 6)
- Attendance (placeholder for Epic 5)
- Evaluation (placeholder for Epic 5)

**And** the page loads in under 2 seconds (NFR2)

---

### Story 4.7: Student Profile - Child Info Tab

**As an** admin,
**I want** to view and edit student demographics in the Child Info tab,
**So that** I can maintain accurate student records.

**Acceptance Criteria:**

**Given** I am on the student profile
**When** I select the "Child Info" tab
**Then** I see all demographic fields: Name, DOB, Grade, Gender, Language, Notes, etc.

**Given** I edit any field
**When** I save changes
**Then** the changes are persisted immediately
**And** I see a confirmation message

**And** inline validation prevents invalid data (FR13)
**And** validation errors appear next to the relevant fields

---

### Story 4.8: Family Management in Student Profile

**As an** admin,
**I want** to manage a student's family contacts within the profile,
**So that** I can maintain guardian information (FR9).

**Acceptance Criteria:**

**Given** I am on the student profile
**When** I scroll to the Family Grid section (below tabs)
**Then** I see a list of linked family members with name, relationship, and contact info

**Given** I click "Add Family"
**When** the inline form appears
**Then** I can create a new family record and link it to the student
**And** I can set the relationship type

**Given** I click on a family row
**When** the edit form appears
**Then** I can update family details or remove the link

**And** sibling students in the same family are shown for context

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

---

## Epic 5: Attendance & Evaluations

Admin can track student attendance per class session, record progress evaluations, and make corrections with a visible audit trail.

### Story 5.1: Attendance Entity & API Endpoints

**As a** developer,
**I want** the Attendance domain entity and REST API endpoints,
**So that** attendance data can be tracked per student per session.

**Acceptance Criteria:**

**Given** the backend project with Student and ClassGroup entities
**When** the Attendance entity is created
**Then** the `Attendance` entity exists with properties:
- Id, StudentId (FK), ClassGroupId (FK), SessionDate, Status (Present/Absent/Late), Notes, CreatedAt, ModifiedAt

**And** AttendanceRepository with methods handling student and class group relationships
**And** DbUp migration script creates the `attendance` table
**And** `/api/attendance` endpoints support:
- GET (list with student/class group/date filters)
- GET `/:id` (single attendance record)
- POST (create attendance entry)
- PUT `/:id` (update attendance - triggers audit)
- GET `/api/students/:id/attendance` (attendance history for a student)

**And** endpoints require authentication

---

### Story 5.2: Attendance Tab in Student Profile

**As an** admin,
**I want** to view and manage attendance in the student profile,
**So that** I can track and update attendance without leaving the profile (FR10).

**Acceptance Criteria:**

**Given** I am on the student profile
**When** I select the "Attendance" tab
**Then** I see a list of recent attendance records with Date, Class Group, Status, Notes columns
**And** status is shown with color-coded chips (Present=green, Absent=red, Late=yellow)

**Given** I click on an attendance row
**When** inline edit mode activates
**Then** I can change the status or add notes
**And** save the change immediately

**Given** I click "Add Attendance"
**When** the inline form appears
**Then** I can select date, class group, and status
**And** the new record appears in the list

---

### Story 5.3: Attendance Audit Trail

**As an** admin,
**I want** to see an audit trail for attendance corrections,
**So that** I have traceability for any changes (FR14).

**Acceptance Criteria:**

**Given** I update an attendance record
**When** the change is saved
**Then** an audit log entry is created with:
- Timestamp, Previous value, New value, User who made the change

**Given** I view an attendance record
**When** I click "View History"
**Then** an Audit Trail Panel appears showing all changes to that record

**And** the audit trail is read-only
**And** this fulfills FR14 for attendance

---

### Story 5.4: Evaluation Entity & API Endpoints

**As a** developer,
**I want** the Evaluation domain entity and REST API endpoints,
**So that** student progress evaluations can be recorded.

**Acceptance Criteria:**

**Given** the backend project with Student and Activity entities
**When** the Evaluation entity is created
**Then** the `Evaluation` entity exists with properties:
- Id, StudentId (FK), ActivityId (FK), EvaluationDate, Score, SpeedMetric, AccuracyMetric, Notes, CreatedAt, ModifiedAt

**And** Activity entity exists with basic activity/curriculum info
**And** EvaluationRepository and DbUp migration scripts
**And** `/api/evaluations` endpoints support CRUD
**And** GET `/api/students/:id/evaluations` returns evaluation history

---

### Story 5.5: Evaluation Tab in Student Profile

**As an** admin,
**I want** to view and record evaluations in the student profile,
**So that** I can track progress without leaving the profile.

**Acceptance Criteria:**

**Given** I am on the student profile
**When** I select the "Evaluation" tab
**Then** I see a list of evaluations with Activity, Date, Score, Speed, Accuracy columns
**And** scores are shown with visual indicators (status chips)

**Given** I click "Add Evaluation"
**When** the inline form appears
**Then** I can select an activity, enter date, scores, and notes
**And** the new evaluation appears in the list

**Given** I click on an evaluation row
**When** inline edit mode activates
**Then** I can update scores and notes

---

### Story 5.6: Bulk Attendance Entry

**As an** admin,
**I want** to record attendance for an entire class group session at once,
**So that** I can efficiently mark attendance for a class.

**Acceptance Criteria:**

**Given** I am on the Class Groups page
**When** I click "Take Attendance" on a class group
**Then** I see a list of all students in the class
**And** each student has a status toggle (Present/Absent/Late)

**Given** I mark attendance for all students
**When** I click "Save All"
**Then** attendance records are created for all students
**And** I see a success confirmation

**And** existing attendance for the same date shows pre-filled values

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

---

## Epic 6: Billing & Financials

Admin can manage student billing records, track payments, and view financial status with audit trail.

### Story 6.1: Billing Entity & API Endpoints

**As a** developer,
**I want** the Billing domain entities and REST API endpoints,
**So that** financial data can be tracked per student.

**Acceptance Criteria:**

**Given** the backend project with Student entity
**When** the billing entities are created
**Then** the following entities exist:
- `Invoice`: Id, StudentId (FK), InvoiceDate, Amount, DueDate, Status (Pending/Paid/Overdue), Notes
- `Payment`: Id, StudentId (FK), InvoiceId (FK, optional), PaymentDate, Amount, PaymentMethod, ReceiptNumber, Notes
- `Receipt`: Id, PaymentId (FK), ReceiptDate, ReceiptNumber

**And** BillingRepository and DbUp migration scripts create the billing tables
**And** `/api/billing` endpoints support:
- GET `/api/students/:id/billing` (billing summary with balance)
- GET `/api/students/:id/invoices` (list invoices)
- POST `/api/students/:id/invoices` (create invoice)
- GET `/api/students/:id/payments` (list payments)
- POST `/api/students/:id/payments` (record payment)

**And** endpoints require authentication

---

### Story 6.2: Financial Tab in Student Profile

**As an** admin,
**I want** to view and manage billing in the student profile,
**So that** I can track and update financial status without leaving the profile.

**Acceptance Criteria:**

**Given** I am on the student profile
**When** I select the "Financial" tab
**Then** I see a billing summary showing:
- Current balance
- Last payment date and amount
- Outstanding invoices count

**And** below the summary, I see two sections:
- Invoices list (Date, Amount, Status)
- Payments list (Date, Amount, Receipt #)

**Given** I click on an invoice
**When** the detail appears
**Then** I can view invoice details and status

---

### Story 6.3: Record Payment

**As an** admin,
**I want** to record a payment for a student,
**So that** I can track payments against invoices.

**Acceptance Criteria:**

**Given** I am on the student's Financial tab
**When** I click "Record Payment"
**Then** an inline form appears with:
- Amount input
- Payment method dropdown
- Optional invoice to apply against
- Notes field

**Given** I submit a valid payment
**When** it saves
**Then** the payment appears in the payments list
**And** the balance is updated
**And** a receipt number is generated

**And** if applied to an invoice, the invoice status updates

---

### Story 6.4: Create Invoice

**As an** admin,
**I want** to create invoices for a student,
**So that** I can track amounts owed.

**Acceptance Criteria:**

**Given** I am on the student's Financial tab
**When** I click "Create Invoice"
**Then** an inline form appears with:
- Amount input
- Due date picker
- Description/notes

**Given** I submit a valid invoice
**When** it saves
**Then** the invoice appears in the invoices list
**And** the balance is updated
**And** status is set to "Pending"

---

### Story 6.5: Billing Audit Trail

**As an** admin,
**I want** to see an audit trail for billing changes,
**So that** I have traceability for financial corrections (FR14).

**Acceptance Criteria:**

**Given** I update a payment or invoice record
**When** the change is saved
**Then** an audit log entry is created with:
- Timestamp, Previous value, New value, User who made the change

**Given** I view a billing record
**When** I click "View History"
**Then** an Audit Trail Panel appears showing all changes

**And** the audit trail is read-only
**And** this fulfills FR14 for billing

---

### Story 6.6: Billing Status in Profile Header

**As an** admin,
**I want** to see quick billing status in the profile header,
**So that** I immediately know if a student has outstanding balance.

**Acceptance Criteria:**

**Given** I view a student profile
**When** the header loads
**Then** the third column shows:
- Current balance with color indicator (green if 0, red if outstanding)
- "Up to date" or "Balance due" text

**Given** the student has overdue invoices
**When** I view the header
**Then** a warning indicator is shown

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

---

## Epic 7: Legacy Data Migration

Developer can import legacy XML/XSD data with preview, exception handling, and audit logging.

### Story 7.1: Legacy Schema Parser

**As a** developer,
**I want** a parser that reads legacy XML/XSD files,
**So that** I can extract data from the Access export format.

**Acceptance Criteria:**

**Given** the legacy XML and XSD files in `docs/legacy/`
**When** the parser runs
**Then** it reads and validates XML against the XSD schema
**And** extracts School, ClassGroup (Class Group), Activity, and Children (Student) data
**And** handles encoding and format variations in the legacy data

**And** parser errors are logged with file/line information
**And** the parser can be run from CLI (`dotnet run import parse`)

---

### Story 7.2: Data Mapping Service

**As a** developer,
**I want** a mapping service that transforms legacy data to domain entities,
**So that** parsed data can be loaded into the new database.

**Acceptance Criteria:**

**Given** parsed legacy data
**When** the mapping service runs
**Then** legacy fields are mapped to new entity properties:
- Children → Student entity
- Class Group → ClassGroup entity
- School → School entity
- Activity → Activity entity

**And** data transformations are applied (date formats, enum conversions, etc.)
**And** missing or invalid values are flagged for review
**And** mapping rules are documented in code

---

### Story 7.3: Import Preview Mode

**As a** developer,
**I want** to preview import results before committing,
**So that** I can verify the data before it goes live (FR12).

**Acceptance Criteria:**

**Given** I run the import command with `--preview` flag
**When** the import runs
**Then** it shows:
- Total records to import by entity type
- Sample of mapped records
- Validation warnings and errors
- Records that will be skipped

**And** NO data is written to the database
**And** I can review the preview output before running full import

---

### Story 7.4: Import Execution with Exception Handling

**As a** developer,
**I want** to run the full import with proper error handling,
**So that** valid records are imported and failures are tracked.

**Acceptance Criteria:**

**Given** I run the import command without `--preview`
**When** the import executes
**Then** valid records are inserted into the database
**And** failed records are logged to an exceptions file (JSON or CSV)
**And** each exception includes: record ID, field, error reason, original value

**Given** some records fail validation
**When** the import completes
**Then** a summary shows: X imported, Y failed, Z skipped
**And** the exceptions file is saved for review

---

### Story 7.5: Import Audit Log

**As a** developer,
**I want** an audit log of all import operations,
**So that** I have traceability for data migration.

**Acceptance Criteria:**

**Given** I run an import
**When** records are created
**Then** an import audit log entry is created with:
- Import run timestamp
- User/process that ran the import
- Source file(s) used
- Records created, updated, skipped, failed counts

**And** I can query the audit log to see import history
**And** this fulfills the audit requirement for FR12

---

### Story 7.6: Import Re-run Capability

**As a** developer,
**I want** to re-run the import to update or fix records,
**So that** I can iterate on data quality.

**Acceptance Criteria:**

**Given** data has been previously imported
**When** I run the import again
**Then** existing records are matched by a unique key (e.g., legacy ID)
**And** I can choose: skip existing, update existing, or fail on conflict

**Given** I run with `--update` flag
**When** matching records exist
**Then** they are updated with new values
**And** changes are logged to the audit trail

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


---

### Epic 8: Activity Management

Admin can manage the educational program catalog (activities) that are delivered during class sessions and evaluated in student progress tracking.

**FRs covered:** Foundation for Epic 5 (Evaluations - Activity reference)

**Implementation Notes:**
- Activity CRUD with program code, grade level, folder, and curriculum info
- Icon field: OLE Object from legacy XML stored as base64 string
- API endpoints + frontend feature module
- Legacy data migration from Activity XML/XSD (11.8MB with embedded images)
- Simple maintenance entity following Epic 2 patterns

---

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

**And** ActivityRepository and DbUp migration script create the `activities` table
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


