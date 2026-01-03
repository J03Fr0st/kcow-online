---
stepsCompleted:
  - 'step-01-validate-prerequisites'
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/architecture.md"
  - "_bmad-output/planning-artifacts/ux-design-specification.md"
workflowType: 'epics-and-stories'
project_name: 'kcow-online'
user_name: 'Joe'
date: '2026-01-03'
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
   - EF Core + SQLite database configuration
   - Better Auth server-side integration
   - ProblemDetails error handling middleware
   - Serilog logging configuration
   - CORS configuration for frontend

2. **Frontend Better Auth Integration**: Add Better Auth client to the existing Angular 21 frontend:
   - Install and configure Better Auth client library
   - Create AuthService wrapper using Angular Signals
   - Implement login/logout flows
   - Add auth guard for protected routes
   - Add HTTP interceptor for auth tokens

---

**From Architecture:**

- **Starter Template**: Existing Angular 21 scaffold in `apps/frontend/` is retained (no re-initialization required).
- **Database**: SQLite for v1, migration path to PostgreSQL post-MVP.
- **ORM**: EF Core with entities in Domain layer, configurations in Infrastructure/Data.
- **Authorization**: Role-based checks at API endpoints (Admin role).
- **API Style**: RESTful JSON endpoints with plural kebab-case paths (e.g., `/students`, `/class-groups`).
- **Frontend State**: Angular Signals for UI state; RxJS for async flows.
- **Forms**: Reactive Forms only with shared validators.
- **Routing**: Lazy-loaded feature routes under AdminLayout shell.
- **Performance**: OnPush change detection throughout.
- **Legacy Import**: XML/XSD import workflow with preview, exception handling, and audit log.

**Implementation Sequence (Updated):**
1. **Backend base setup** + EF Core + Better Auth server-side.
2. **Frontend Better Auth integration** (login/logout, guards, interceptors).
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

{{requirements_coverage_map}}

## Epic List

{{epics_list}}
