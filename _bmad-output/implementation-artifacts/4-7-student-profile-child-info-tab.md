# Story 4.7: Student Profile - Child Info Tab

Status: done

## Story

As an **admin**,
I want **to view and edit student demographics in the Child Info tab**,
so that **I can maintain accurate student records**.

## Acceptance Criteria

1. **Given** I am on the student profile
   **When** I select the "Child Info" tab
   **Then** I see all demographic fields: Name, DOB, Grade, Gender, Language, Notes, etc.

2. **Given** I edit any field
   **When** I save changes
   **Then** the changes are persisted immediately
   **And** I see a confirmation message

3. **And** inline validation prevents invalid data (FR13)

4. **And** validation errors appear next to the relevant fields

## Tasks / Subtasks

- [x] Task 1: Create ChildInfoTab component (AC: #1)
  - [x] Display all student demographic fields
  - [x] Use reactive form for editing
  - [x] Layout fields in logical groups
- [x] Task 2: Implement inline editing (AC: #2)
  - [x] Edit mode for the tab
  - [x] Save button to persist changes
  - [x] Call student update API
  - [x] Show success notification
- [x] Task 3: Add validation (AC: #3, #4)
  - [x] Add validators for required fields
  - [x] Display inline error messages
  - [x] Prevent save if invalid
- [x] Task 4: Style and layout
  - [x] Group fields logically (Personal, Contact, Medical)
  - [x] Use form field components for consistency

## Dev Notes

### Child Info Fields

| Group | Fields |
|-------|--------|
| Personal | FirstName*, LastName*, DOB, Gender, Language |
| Assignment | School, ClassGroup, SeatNumber |
| Notes | Notes, MedicalNotes |

### Form Pattern

```typescript
@Component({...})
export class ChildInfoTabComponent {
  @Input() student!: Student;
  @Output() updated = new EventEmitter<Student>();
  
  form = inject(FormBuilder).group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    dateOfBirth: [''],
    gender: [''],
    language: [''],
    grade: [''],
    notes: [''],
    medicalNotes: [''],
  });
  
  onSave() {
    if (this.form.valid) {
      this.studentService.update(this.student.id, this.form.value)
        .subscribe(updated => this.updated.emit(updated));
    }
  }
}
```

### Previous Story Dependencies

- **Story 4.6** provides: Profile layout with tabs

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 4.7]
- [Source: docs/legacy/4_Children/Children.xsd] - Field reference

## Dev Agent Record

### Agent Model Used

glm-4.7

### Debug Log References

No issues encountered during implementation.

### Completion Notes List

**Implementation Summary:**
- Created dedicated child-info-tab component with inline editing
- Used Angular reactive forms with validators
- Implemented edit/save/cancel workflow
- Added Material Snackbar for notifications
- Integrated with student profile page

**Acceptance Criteria Status:**
1. AC #1: Display all demographic fields ✅
   - Personal: firstName*, lastName*, DOB, Gender, Language
   - Assignment: School, ClassGroup, Seat (read-only display)
   - Notes: General notes
2. AC #2: Save changes ✅
   - Edit button enables form
   - Save button calls updateStudent API
   - Success notification appears on save
3. AC #3: Inline validation ✅
   - Required field validators on firstName, lastName
   - Max length validators (50 for names, 5 for grade, 255 for notes)
4. AC #4: Validation errors next to fields ✅
   - Error messages appear below invalid fields
   - Form is marked touched on save attempt

**Technical Decisions:**
- Angular reactive forms with FormBuilder
- Material MatSnackBar for notifications (success/error)
- Edit mode pattern: view → edit → save/cancel → view
- School assignment displayed as read-only (changed via Edit Student button)
- Signal-based state management (isEditing, isSaving, error)

**File List:**

**Frontend:**
- `apps/frontend/src/app/features/students/student-profile/components/child-info-tab/child-info-tab.component.ts` (NEW)
- `apps/frontend/src/app/features/students/student-profile/components/child-info-tab/child-info-tab.component.html` (NEW)
- `apps/frontend/src/app/features/students/student-profile/components/child-info-tab/child-info-tab.component.scss` (NEW)
- `apps/frontend/src/app/features/students/student-profile/components/child-info-tab/child-info-tab.component.spec.ts` (NEW)
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.ts` (MODIFIED - added ChildInfoTabComponent import and onStudentUpdated method)
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.html` (MODIFIED - replaced inline content with app-child-info-tab component)
