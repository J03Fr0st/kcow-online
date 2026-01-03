---
stepsCompleted:
  - 'step-01-init'
  - 'step-02-discovery'
  - 'step-03-success'
  - 'step-04-journeys'
  - 'step-07-project-type'
  - 'step-08-scoping'
  - 'step-09-functional'
  - 'step-10-nonfunctional'
  - 'step-11-complete'
inputDocuments:
  - "docs/index.md"
  - "docs/architecture.md"
  - "docs/domain-models.md"
  - "docs/development-guide.md"
  - "docs/source-tree.md"
  - "docs/ux-design-specification.md"
  - "docs/project_context.md"
workflowType: 'prd'
lastStep: 11
documentCounts:
  briefCount: 0
  researchCount: 0
  brainstormingCount: 0
  projectDocsCount: 7
---

# Product Requirements Document - kcow-online

**Author:** Joe
**Date:** 2026-01-03

## Executive Summary

KCOW is a web-based admin system for a small organization managing schools, class groups/schedules, students, attendance, billing, and reporting. The immediate focus is migrating from a hard-to-maintain legacy system into a modern, structured web app while preserving core workflows and enabling faster student lookup for the single admin user.

### What Makes This Special

This migration replaces a legacy, difficult-to-maintain system with a web-based admin platform that enforces better validation and proper foreign key relationships. The result is faster student lookup, clearer data integrity, and a foundation for future feature growth once the migration is complete.

## Project Classification

**Technical Type:** web_app
**Domain:** edtech
**Complexity:** medium
**Project Context:** Brownfield - extending existing system

Classification rationale: existing Angular SPA admin UI, school/class/student workflows, and education-focused domain data.

## Success Criteria

### User Success

- Admin can locate a student and complete all core updates from a single screen without navigating away.
- Daily admin tasks (attendance, evaluation, billing updates) can be completed in under 2 minutes per student.
- Admin reports zero confusion about where to find or update key student, class, and family data.

### Business Success

- Legacy data is fully migrated with no critical data loss and a clear audit log of any exceptions.
- Admin time spent on routine data maintenance drops by at least 30% compared to the legacy system.
- 3 months: a working system in full use by the admin.
- 12 months: v2 released.

### Technical Success

- Legacy XML/XSD import completes with >=99% record accuracy and repeatable, documented migration steps.
- Core pages (student profile, class groups, attendance, billing) load in <2 seconds on standard office hardware.
- System is stable with no data corruption and reliable CRUD for all legacy workflows.

### Measurable Outcomes

- 100% of legacy workflows are available in v1 with redesigned UX.
- 0 critical migration blockers at go-live; any rejected records are documented with reasons.
- Admin can complete "find student -> update attendance -> update billing" in a single uninterrupted flow.

## Product Scope

### MVP - Minimum Viable Product

- Legacy data import from XML/XSD and verified mapping to new database.
- Full parity workflows: student profiles, class groups, attendance, evaluations, billing/financials, fleet/scheduling, school registry, curriculum tracking.
- One-screen student administration view as primary workflow surface.
- Web-only deployment for a single admin user.

### Growth Features (Post-MVP)

- UX enhancements beyond parity (bulk actions, quick filters, smarter search).
- Reporting dashboards and exportable summaries.
- Improved data quality tooling (duplicate detection, validation suggestions).

### Vision (Future)

- v2 enhancements beyond legacy: streamlined end-to-end operations and automation of repetitive admin work.
- Expanded analytics for attendance, progress, and billing trends.
- Multi-user support and role-based access if growth demands it.

## User Journeys

**Journey 1: Thandi - The One-Screen Student Update (Primary Admin, Success Path)**  
Thandi is the sole admin for KCOW. Her mornings start with a list of students who need updates from the day’s classes. In the legacy system she bounced between tabs and screens just to mark attendance, update billing status, and note evaluations. On the new system, she searches a student once and lands on a single, dense profile screen. From there she updates attendance, records a payment, and logs a progress note without leaving the view.  
The breakthrough comes when she completes a full student update in one flow — no hunting, no back-and-forth. By midday she notices she’s processed more records than usual with less fatigue, and she trusts that every change is visible immediately.

**Journey 2: Thandi - Fixing Attendance on the Wrong Day (Primary Admin, Edge Case)**  
At the end of the day, Thandi realizes she marked attendance for the wrong day. In the legacy system, fixing this meant digging through multiple screens and double-checking class groups, which often led to uncertainty about what was actually saved. In the new system, she opens the same student profile, navigates to the attendance history, and corrects the date with a clear audit trail of the change. The system highlights the correction and confirms the updated record immediately, restoring confidence without a detour.

**Journey 3: Thandi - New Term Schedule Updates (Primary Admin, Periodic Task)**  
At the start of a new term, Thandi needs to adjust class group schedules and update the roster for each school. She opens the schedule management area, updates time slots and truck assignments, and then applies those changes across the relevant class groups. The system validates conflicts and confirms the updated schedule, allowing her to complete term setup in one focused session rather than scattered edits across multiple screens.

### Journey Requirements Summary

These journeys reveal requirements for:
- Single-screen student profile with attendance, billing, and evaluation updates in one flow.
- Fast global search with unambiguous student selection.
- Attendance history with correction support and audit trail.
- Schedule management with conflict validation and bulk updates across class groups.
- Immediate visual confirmation of saved changes to reduce admin uncertainty.

## web_app Specific Requirements

### Project-Type Overview

KCOW is a single-page web application (SPA) intended for desktop use by a single admin user. It targets modern Chromium browsers (Chrome/Edge), does not require SEO, and does not include real-time collaboration features. Accessibility is basic best-effort.

### Technical Architecture Considerations

- SPA architecture with route-based navigation under the admin layout.
- Desktop-first UI with dense, information-rich screens.
- No SEO or server-side rendering requirements.
- No real-time features; standard request/response interactions are sufficient.

### Responsive Design

- Responsive behavior should prioritize desktop layouts; mobile support is not required for v1.

### Performance Targets

- Maintain sub-2s page loads for core screens (student profile, class groups, attendance, billing).

### Accessibility Level

- Basic best-effort accessibility; follow standard semantic HTML and contrast guidance where practical.

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Experience MVP
**Resource Requirements:** Medium team (3-5) with strong frontend, backend, and data migration skills

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:**
- One-screen student administration (primary daily workflow)
- Migration/import as a dev use dotnet commands

**Must-Have Capabilities:**
- Legacy XML/XSD import with validated mapping
- Full parity of all legacy workflows in v1
- Single-screen student profile for fast updates
- Reliable CRUD and consistent validation across modules
- Audit log for import exceptions and corrections

### Post-MVP Features

**Phase 2 (Post-MVP):**
- Reporting dashboards and exportable summaries
- Automation of repetitive admin tasks
- Data quality tools (duplicate detection, validation suggestions)

**Phase 3 (Expansion):**
- Smarter search and bulk actions for higher throughput
- Multi-user roles and permissions if growth demands it
- Advanced analytics for attendance, progress, and billing trends

### Risk Mitigation Strategy

**Technical Risks:** Import accuracy is the highest risk. Mitigate with staged imports, validation checks, and a clear exception workflow before launch.
**Market Risks:** Low; scope is parity with existing workflows. Validate by confirming all legacy tasks are supported.
**Resource Risks:** If capacity is tight, protect the one-screen workflow and import integrity first, defer enhancements to Phase 2.

## Functional Requirements

### Trucks and Fleet Scheduling

- FR1: Admin can manage truck records.

### Schools Management

- FR2: Admin can create, view, edit, and archive schools.
- FR3: Admin can manage school contacts and billing settings.

### Class Groups and Scheduling

- FR4: Admin can create, view, edit, and archive class groups.
- FR5: Admin can set class group schedules (day, time, sequence) and assign trucks.
- FR6: Admin can detect and resolve scheduling conflicts before saving changes.

### Student Management

- FR7: Admin can create, view, edit, and archive student records.
- FR8: Admin can assign a student to a school, class group, and seat.
- FR9: Admin can manage student family/guardian contacts and relationships.
- FR10: Admin can view a single-screen student profile that includes demographics, attendance, evaluation, and billing details.
- FR11: Admin can search students by name, school, grade, and class group with unambiguous results.

### Data Migration (Dev-Only)

- FR12: Developer can run migration/import tooling to load legacy data into the system.

### Data Integrity and Auditability

- FR13: Admin can see validation errors before saving invalid data.
- FR14: Admin can view an audit trail for changes to attendance and billing records.

## Non-Functional Requirements

### Performance

- Student search results return in under 2 seconds.
- Student profile and core workflow pages load in under 2 seconds.
- All primary admin actions complete within 2 seconds under normal office-network conditions.

### Security

- Student and guardian PII is protected in line with POPIA requirements.
- Billing and contact data is treated as sensitive data.
- Access is restricted to authorized admin users only.

### Reliability

- System availability supports no more than 1 hour of downtime per month.
- Critical admin workflows remain usable during standard operating hours.

### Accessibility

- Basic accessibility: keyboard operability for form fields, visible focus states, and readable typography.

### Integration

- No external integrations required for v1.
