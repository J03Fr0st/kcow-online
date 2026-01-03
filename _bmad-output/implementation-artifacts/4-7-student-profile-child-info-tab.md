# Story 4.7: Student Profile - Child Info Tab

Status: ready-for-dev

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

- [ ] Task 1: Create ChildInfoTab component (AC: #1)
  - [ ] Display all student demographic fields
  - [ ] Use reactive form for editing
  - [ ] Layout fields in logical groups
- [ ] Task 2: Implement inline editing (AC: #2)
  - [ ] Edit mode for the tab
  - [ ] Save button to persist changes
  - [ ] Call student update API
  - [ ] Show success notification
- [ ] Task 3: Add validation (AC: #3, #4)
  - [ ] Add validators for required fields
  - [ ] Display inline error messages
  - [ ] Prevent save if invalid
- [ ] Task 4: Style and layout
  - [ ] Group fields logically (Personal, Contact, Medical)
  - [ ] Use form field components for consistency

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
