---
stepsCompleted: []
inputDocuments:

workflowType: 'ux-design'
lastStep: 1
project_name: 'kcow-online'
user_name: 'Joe'
date: '2025-12-06'
---

# UX Design Specification

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each UX decision together._

## Executive Summary

### Project Vision
A single-admin web app to run two mobile computer literacy trucks across multiple school.

### Target Users
- Mobile Computer Lab Administrator: manages schedustudentsles, attendance/progress, activities, billing, for two trucks serving many schools.
- Supporting roles (indirect): teachers/drivers as assignees; parents/students as records/billing contacts.

### Key Design Challenges
- Keep navigation and task switching fast for one admin spanning many modules.
- Prevent overbooking and make schedule/timeslot capacity obvious.
- Reduce friction in heavy form workflows (people, schools, billing) with validation and defaults.
- Provide clear operational status for trucks, routes, and visits.

### Design Opportunities
- Sidebar/breadcrumb structure that mirrors the domain modules.
- Form helpers (search/prefill/validation) for people and schools.

## Core User Experience

### Defining Experience
The core loop is instant student lookup with a rich profile view: search, land on the correct student, and immediately see status plus next actions (attendance, progress, billing). Everything else should orbit this flow.

### Platform Strategy
Desktop web as the primary surface; no offline requirement. Keyboard-first but touch-tolerant layouts are acceptable; optimize for fast scanning and data density on desktop.

### Effortless Interactions
- Instant search/typeahead that narrows by name/school/grade and resists ambiguity.
- Profile page that surfaces attendance history, progress highlights, and billing state without drilling.
- One-click/low-friction actions from the profile: mark attendance, update billing, assign to timeslot/lesson.
- Capacity/eligibility cues inline when acting (e.g., warn/prevent overbooking).

### Critical Success Moments
- Finding the right student quickly with confidence (no name collisions confusion).
- Seeing a clear status snapshot (attendance streaks, progress level, billing standing) at a glance.
- Acting immediately (mark attendance, record payment, assign) without losing context or switching modules.

### Experience Principles
- Prioritize the lookup-to-action flow above everything else.
- Make state obvious: attendance, progress, and billing should be legible without clicks.
- Reduce hops: keep actions in the same view; avoid cross-module detours.
- Prevent mistakes: highlight capacity/conflicts and confirm destructive changes.

## Desired Emotional Response

### Primary Emotional Goals
- Confident accuracy: the admin feels sure every lookup, status view, and update is correct and reflected immediately.

### Emotional Journey Mapping
- First-time: reassured that navigation and search are obvious; nothing feels hidden.
- During core flow: calm, confident, and in control while searching, reviewing status, and acting.
- After actions: certainty that changes stuck (attendance/billing updates are saved and visible).
- Error states: guided, not blamed; clear recovery without doubt.
- Return visits: trust that data is current and the same fast flow awaits.

### Micro-Emotions
- Confidence over skepticism: clear identifiers and confirmations reduce doubt.
- Clarity over confusion: unambiguous naming, search disambiguation, and inline cues.
- Control over anxiety: preview effects (capacity/eligibility) before committing.

### Design Implications
- Show strong disambiguation in search results (name + school + grade) and clear selection state.
- Surface confirmations and recent changes inline (e.g., “Attendance saved · just now”).
- Keep error handling prescriptive: explain what went wrong and the exact next fix.
- Use consistent status chips for attendance/progress/billing; avoid ambiguous colors/labels.

### Emotional Design Principles
- Make accuracy feel visible: reinforce correctness with confirmations and stable identifiers.
- Never leave the user guessing: remove ambiguity in search, actions, and status.
- Calm guidance over blame: errors and conflicts come with clear, low-friction fixes.

## Legacy System Reference

_Based on existing Access database interface (see screenshots in `docs/legacy/4_Children/`)_

### ⚠️ XSD Schema Alignment

**All UI implementations must align with the legacy XSD schema definitions.** The XSD files define the authoritative data model including all fields, types, and constraints. Form fields, validation rules, and data display must match XSD specifications exactly.

- **School**: `docs/legacy/1_School/School.xsd` (30 fields)
- **Class Group**: `docs/legacy/2_Class_Group/Class Group.xsd` (15 fields)
- **Activity**: `docs/legacy/3_Activity/Activity.xsd` (7 fields)
- **Children**: `docs/legacy/4_Children/Children.xsd` (92 fields)

**Use English field names** where XSD uses Afrikaans (e.g., `Trok` → `Truck`, `Taal` → `Language`). See `docs/domain-models.md` for complete XSD-to-implementation mappings and translations.

### Legacy UI Screenshots

| Tab | Screenshot | Key Features |
|-----|------------|--------------|
| Child Information | [View](./legacy/4_Children/1_Child_Information.png) | 3-column layout: Demographics, Payment/T-shirt, Parent contacts |
| Child Financial | [View](./legacy/4_Children/2_Child_Financial.png) | Invoice/Receipt grids, Balance display, Certificate/Statement printing |
| Class Groups | [View](./legacy/4_Children/3_Class_Group.png) | Student list grid with balance and basic info per class |
| Class Groups Attendance | [View](./legacy/4_Children/4_Class_Group_Attendance.png) | Attendance grid with Value, Grade, Planned Date, Seat, Activity |
| Child Evaluation | [View](./legacy/4_Children/5_Child_Evaluation.png) | 14-activity score matrix (1-5 ratings), Speed/Accuracy metrics |
| Class Groups Evaluation | [View](./legacy/4_Children/6_Class_Groups_Evaluation.png) | Consolidated evaluation grid for all students in group |

### Target Layout: Single-Screen Student Profile

The new system should replicate the proven dense, information-rich student profile layout from the legacy system. Everything the admin needs is visible on one screen without drilling into sub-pages.

### Tab Navigation Structure

Horizontal tabs across the top of the student profile:
1. **Child Information** (primary view)
2. **Child Financial**
3. **Class Groups**
4. **Child Attendance**
5. **Class Groups Attendance**
6. **Child Evaluation**
7. **Class Groups Evaluation**

### Three-Column Layout

#### Left Column: Child Demographics
- **Reference Number**: Auto-generated unique ID (e.g., 20180560)
- **Child Name & Surname**: Primary identifiers (color-highlighted)
- **Birthdate**: Date field
- **Grade**: Dropdown (GR 1–12)
- **Class Group**: Dropdown with **Seat Number** assignment
- **Teacher**: Assignment field
- **Attend KCOW at**: Location/branch dropdown
- **Aftercare**: Status dropdown with Extra notes
- **Home Time**: Time field
- **Start Classes**: Date enrolled
- **School Name**: Text field
- **Sex**: Dropdown (F/M)
- **Language**: Dropdown
- **Print ID Card**: Checkbox
- **Social Media**: Status dropdown
- **Terms**: Year terms enrolled (e.g., 1234 = all 4 terms)
- **Indicator 1 & 2**: Year indicators (2023, 2024)
- **Photo**: Child photo with **PhotoUpdated** date

#### Middle Column: Payment & Communication
- **Pay Date**: Last payment date
- **Charge**: Amount due (e.g., R 200,00)
- **Truck**: Assignment dropdown (Truck 1 or 2)
- **Deposit**: Status indicator (color-coded)
- **T-shirt Code**: Uniform tracking
- **T-shirt Money 1 & 2**: Payment amounts
- **T-shirt Received 1 & 2**: Receipt status
- **Receive Notes 1 & 2**: Notes fields
- **Email Social?**: Action button
- **Email Continue?**: Action button

#### Right Column: Parent/Guardian Contacts

**Account Person (Primary Billing Contact):**
- Name, Surname
- Relation dropdown (Father/Pa, Mother/Ma, Guardian, etc.)
- ID Number
- Cellphone (color-highlighted for quick reference)
- Office phone
- Home phone
- Email

**Mother Details:**
- Name, Surname
- Cell, Office, Home phones
- Email

**Father Details:**
- Name, Surname
- Cell (color-highlighted)
- Office, Home phones
- Email

**Family:**
- Family name display (combined parent names)
- SMS Bank Details button

### Bottom Section: Family Grid

Displays all children belonging to the same family:
- **Columns**: Reference, Child Name, Child Surname, Class Group
- **Status indicators**: Billing/attendance status per child
- **EMAIL dropdown**: Communication preferences (YesBook, etc.)
- **Actions**: Refresh, Print ID Card, Generate Family

### Color Coding System

Visual differentiation for quick scanning:
- **Green**: Child name/surname fields
- **Magenta/Pink**: Account person (billing contact) fields
- **Cyan**: Father contact fields
- **Orange/Red**: Deposit status or alerts
- **Purple**: Family grouping identifier

### Child Financial Tab Layout

Invoice and receipt management for the student:

#### Top Header
- Student Reference, Name, Surname, Class Group, Seat (read-only context)

#### Two-Grid Layout
| Invoice Grid | Receipt Grid |
|-------------|--------------|
| Invoice Date, Description, Amount | Receipt Date, Description, Barcode, Amount |

#### Summary Row
- **Invoice Total**: Sum of all invoices
- **Receipt Total**: Sum of all receipts  
- **Balance**: Invoice Total - Receipt Total (color-coded: Green = zero, Red = owing)

#### Action Buttons
- Select Certificate, Print Certificate
- Print Envelope (T-shirt), Print Envelope, Print Envelope aangaan (Afrikaans)
- Print Statement, Dates Fees Payment
- Dates Fees Payment Summary and Account
- Email Dates Fees Payment Summary and Account
- Email Statement

#### Status Tracking
- CertificatePrinted: dropdown status
- AccountGivenOut: dropdown status

### Child Evaluation Tab Layout

Progress tracking with a 14-activity evaluation matrix:

#### Evaluation Matrix (Two Sets)
| Set 1 (Columns 1_1 to 1_14) | Set 2 (Columns 2_1 to 2_14) |
|-----------------------------|-----------------------------|
| Ratings 1-5 for each of 14 activities | Ratings 1-5 for each of 14 activities |

#### Additional Metrics
- **Speed** (1_13 Speed, 2_13 Speed): Numeric score
- **Accuracy** (1_14 Accuracy, 2_14 Accuracy): Numeric score
- **Notes** (1_12Note, 2_12Note): Free text evaluation notes

#### Report Distribution
- Report1GivenOut, Report2GivenOut: Yes/No status
- Print Evaluation Report 1 / 2 (full), Print Evaluation Report 1 / 2 Empty
- Email Prog Act Report 1 / 2

#### Notes Grid
- NoteLink: Reference ID
- Note: Free text field for evaluation comments (e.g., "Could not evaluate activities. Reason: new registration.")

### Class Groups Evaluation Tab Layout

Consolidated view of all students in a class group with their evaluation scores:

| Column | Description |
|--------|-------------|
| Child Name, Surname | Student identity |
| Grade | Current grade |
| Class Group | Group assignment |
| Seat | Seat number |
| 1_1 to 1_14 | Set 1 evaluation scores |
| 1_12N | Notes indicator |
| 1_13, 1_14 | Speed/Accuracy scores |

This grid enables quick scanning across all students in a class for progress comparison.

### Key Design Principles from Legacy

1. **Everything on one screen**: No drilling into sub-pages for core student info
2. **Dense but organized**: Three-column layout maximizes information density
3. **Color-coded sections**: Quick visual parsing of child vs. parent vs. billing
4. **Family context visible**: Always see siblings and family billing relationship
5. **Quick actions inline**: Email, Print ID, SMS all accessible without navigation
6. **Dropdown-heavy**: Reduce typing with predefined options (Grade, Class, Relation, etc.)
7. **Status at a glance**: Color-coded fields surface payment/attendance issues immediately
8. **Grid views for bulk operations**: Class-level views enable quick scanning and batch actions
9. **Dual evaluation sets**: Two parallel evaluation tracks (likely for different time periods)
10. **Print + Email parity**: Every printable report has an email equivalent
