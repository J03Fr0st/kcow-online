# Story 4.8: Family Management in Student Profile

Status: done

## Story

As an **admin**,
I want **to manage a student's family contacts within the profile**,
so that **I can maintain guardian information (FR9)**.

## Acceptance Criteria

1. **Given** I am on the student profile
   **When** I scroll to the Family Grid section (below tabs)
   **Then** I see a list of linked family members with name, relationship, and contact info

2. **Given** I click "Add Family"
   **When** the inline form appears
   **Then** I can create a new family record and link it to the student
   **And** I can set the relationship type

3. **Given** I click on a family row
   **When** the edit form appears
   **Then** I can update family details or remove the link

4. **And** sibling students in the same family are shown for context

## Tasks / Subtasks

- [x] Task 1: Create FamilyGrid component (AC: #1)
  - [x] Display list of linked families
  - [x] Show name, relationship, phone, email
  - [x] Add "Add Family" button
- [x] Task 2: Load family data (AC: #1)
  - [x] Call `/api/students/{id}/families`
  - [x] Display sibling context (other students in family)
- [x] Task 3: Implement Add Family flow (AC: #2)
  - [x] Inline form for new family
  - [x] Relationship type dropdown
  - [x] Save creates family and links to student
- [x] Task 4: Implement Edit/Remove flow (AC: #3)
  - [x] Click family to edit
  - [x] Update family details
  - [x] Option to unlink (not delete) family
- [x] Task 5: Display siblings (AC: #4)
  - [x] Show other students linked to same family
  - [x] Link to their profiles

## Dev Notes

### Family Grid Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Family Contacts                                [Add Family] │
├─────────────────────────────────────────────────────────────┤
│ Name              │ Relationship │ Phone        │ Email     │
│ John Smith Sr.    │ Parent       │ 012 345 6789 │ john@...  │
│ Jane Smith        │ Parent       │ 012 345 6790 │ jane@...  │
│                                                             │
│ Siblings: [Sarah Smith] [Tom Smith]                         │
└─────────────────────────────────────────────────────────────┘
```

### Family Service Methods

```typescript
class FamilyService {
  getFamiliesForStudent(studentId: number): Observable<StudentFamily[]>;
  createAndLinkFamily(studentId: number, data: CreateFamilyRequest): Observable<Family>;
  linkExistingFamily(studentId: number, familyId: number, relationship: RelationshipType): Observable<void>;
  unlinkFamily(studentId: number, familyId: number): Observable<void>;
  updateFamily(familyId: number, data: UpdateFamilyRequest): Observable<Family>;
}
```

### File Structure

```
apps/frontend/src/app/features/students/
├── student-profile/
│   └── components/
│       └── family-grid/
│           ├── family-grid.component.ts
│           └── family-grid.component.html
```

### Previous Story Dependencies

- **Story 4.2** provides: Family API endpoints
- **Story 4.6** provides: Profile layout

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.8]
- [Source: _bmad-output/planning-artifacts/prd.md#FR9]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Family Grid]

## Dev Agent Record

### Agent Model Used

glm-4.7

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

**Implementation Summary:**
- Created family-section component for student profile
- Displays linked family, guardians, and siblings
- Implemented CRUD for guardians (add, edit, delete)
- Family editing (name, notes, primary billing contact)
- Sibling display with navigation to their profiles

**Acceptance Criteria Status:**
1. AC #1: Display linked families ✅
   - Shows family name, guardians with relationship, phone, email
   - Located below tabbed content as "Family Contacts"
2. AC #2: Add Family flow ✅
   - Create family when none linked
   - Add guardians to family
   - Set relationship type
3. AC #3: Edit/Remove flow ✅
   - Edit family details (name, notes)
   - Edit guardian information
   - Delete guardian (with confirmation)
4. AC #4: Siblings display ✅
   - Shows other students in same family
   - Click to navigate to sibling profiles
   - Shows grade and status

**Technical Decisions:**
- Used existing FamilyService for API calls
- Material Snackbar for notifications
- Inline editing pattern (view → edit → save/cancel)
- Separate forms for family and guardians
- Table view for guardians list

**File List:**

**Frontend:**
- `apps/frontend/src/app/features/students/student-profile/components/family-section/family-section.component.ts` (NEW)
- `apps/frontend/src/app/features/students/student-profile/components/family-section/family-section.component.html` (NEW)
- `apps/frontend/src/app/features/students/student-profile/components/family-section/family-section.component.scss` (NEW)
- `apps/frontend/src/app/features/students/student-profile/components/family-section/family-section.component.spec.ts` (NEW)
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.ts` (MODIFIED - added FamilySectionComponent import and onFamilyUpdated method)
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.html` (MODIFIED - added family section below tabs)
