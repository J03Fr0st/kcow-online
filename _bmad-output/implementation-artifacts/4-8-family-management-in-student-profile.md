# Story 4.8: Family Management in Student Profile

Status: ready-for-dev

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

- [ ] Task 1: Create FamilyGrid component (AC: #1)
  - [ ] Display list of linked families
  - [ ] Show name, relationship, phone, email
  - [ ] Add "Add Family" button
- [ ] Task 2: Load family data (AC: #1)
  - [ ] Call `/api/students/{id}/families`
  - [ ] Display sibling context (other students in family)
- [ ] Task 3: Implement Add Family flow (AC: #2)
  - [ ] Inline form for new family
  - [ ] Relationship type dropdown
  - [ ] Save creates family and links to student
- [ ] Task 4: Implement Edit/Remove flow (AC: #3)
  - [ ] Click family to edit
  - [ ] Update family details
  - [ ] Option to unlink (not delete) family
- [ ] Task 5: Display siblings (AC: #4)
  - [ ] Show other students linked to same family
  - [ ] Link to their profiles

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
