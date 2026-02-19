import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { NotificationService } from '@core/services/notification.service';
import {
  type ProblemDetails,
  type Student,
  StudentService,
  type UpdateStudentRequest,
} from '@core/services/student.service';
import { type Observable, of, throwError } from 'rxjs';
import { ChildInfoTabComponent } from './child-info-tab.component';

interface MockStudentService {
  updateStudent: jest.Mock<Observable<Student>, [number, UpdateStudentRequest]>;
}

describe('ChildInfoTabComponent', () => {
  let component: ChildInfoTabComponent;
  let fixture: ComponentFixture<ChildInfoTabComponent>;
  let mockStudentService: MockStudentService;
  let mockNotificationService: jest.Mocked<Partial<NotificationService>>;

  const mockStudent: Student = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    dateOfBirth: '2015-03-15',
    gender: 'M',
    language: 'Eng',
    grade: 'Grade 5',
    generalNote: 'Some notes',
    schoolName: 'Test School',
    classGroupCode: '5A',
    seat: '12',
    reference: 'REF001',
    photoUrl: '',
    isActive: true,
    printIdCard: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  };

  beforeEach(async () => {
    mockStudentService = {
      updateStudent: jest.fn().mockReturnValue(of(mockStudent)),
    };

    mockNotificationService = {
      success: jest.fn().mockReturnValue('notification-id'),
      error: jest.fn().mockReturnValue('notification-id'),
    };

    await TestBed.configureTestingModule({
      imports: [ChildInfoTabComponent, ReactiveFormsModule, FormsModule],
      providers: [
        { provide: StudentService, useValue: mockStudentService },
        { provide: NotificationService, useValue: mockNotificationService },
        FormBuilder,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ChildInfoTabComponent);
    component = fixture.componentInstance;

    // Set the required input
    component.student.set(mockStudent);
  });

  describe('Component Creation', () => {
    it('should create', () => {
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should initialize form with student data', () => {
      fixture.detectChanges();
      expect(component.form.value.firstName).toBe('John');
      expect(component.form.value.lastName).toBe('Doe');
      expect(component.form.value.gender).toBe('M');
      expect(component.form.value.language).toBe('Eng');
    });

    it('should disable form by default', () => {
      fixture.detectChanges();
      expect(component.form.disabled).toBe(true);
    });
  });

  describe('Display Fields (AC #1)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display first name', () => {
      const input = fixture.debugElement.query(By.css('#firstName'));
      expect(input).toBeTruthy();
      expect(input.nativeElement.value).toBe('John');
    });

    it('should display last name', () => {
      const input = fixture.debugElement.query(By.css('#lastName'));
      expect(input).toBeTruthy();
      expect(input.nativeElement.value).toBe('Doe');
    });

    it('should display date of birth', () => {
      const input = fixture.debugElement.query(By.css('#dateOfBirth'));
      expect(input).toBeTruthy();
    });

    it('should display gender', () => {
      const select = fixture.debugElement.query(By.css('#gender'));
      expect(select).toBeTruthy();
      expect(select.nativeElement.value).toBe('M');
    });

    it('should display language', () => {
      const select = fixture.debugElement.query(By.css('#language'));
      expect(select).toBeTruthy();
      expect(select.nativeElement.value).toBe('Eng');
    });

    it('should display grade', () => {
      const input = fixture.debugElement.query(By.css('#grade'));
      expect(input).toBeTruthy();
      expect(input.nativeElement.value).toBe('Grade 5');
    });

    it('should display general notes', () => {
      const textarea = fixture.debugElement.query(By.css('#generalNote'));
      expect(textarea).toBeTruthy();
      expect(textarea.nativeElement.value).toBe('Some notes');
    });

    it('should display school assignment section', () => {
      const content = fixture.nativeElement.textContent;
      expect(content).toContain('School Assignment');
      expect(content).toContain('Test School');
      expect(content).toContain('5A');
      expect(content).toContain('12');
    });
  });

  describe('Edit Mode (AC #2)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have edit button when not editing', () => {
      const editButton = fixture.debugElement.query(By.css('button.btn-primary'));
      expect(editButton).toBeTruthy();
      expect(editButton.nativeElement.textContent).toContain('Edit Information');
    });

    it('should enable edit mode when edit button is clicked', () => {
      const editButton = fixture.debugElement.query(By.css('button.btn-primary'));
      editButton.nativeElement.click();
      fixture.detectChanges();

      expect(component.isEditing()).toBe(true);
      expect(component.form.enabled).toBe(true);
    });

    it('should disable form when cancel is clicked', () => {
      component.enableEdit();
      fixture.detectChanges();

      const cancelButton = fixture.debugElement
        .queryAll(By.css('button'))
        .find((b) => b.nativeElement.textContent.includes('Cancel'));
      cancelButton?.nativeElement.click();
      fixture.detectChanges();

      expect(component.isEditing()).toBe(false);
      expect(component.form.disabled).toBe(true);
    });
  });

  describe('Save Functionality (AC #2)', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.enableEdit();
      fixture.detectChanges();
    });

    it('should save changes when save button is clicked', () => {
      component.form.setValue({
        firstName: 'Jane',
        lastName: 'Smith',
        dateOfBirth: '2016-04-20',
        gender: 'F',
        language: 'Afr',
        grade: 'Grade 6',
        generalNote: 'Updated notes',
      });

      const saveButton = fixture.debugElement
        .queryAll(By.css('button'))
        .find((b) => b.nativeElement.textContent.includes('Save'));
      saveButton?.nativeElement.click();
      fixture.detectChanges();

      expect(mockStudentService.updateStudent).toHaveBeenCalledWith(
        1,
        expect.objectContaining({
          firstName: 'Jane',
          lastName: 'Smith',
        }),
      );
    });

    it('should emit updated event on successful save', () => {
      const updatedStudent = { ...mockStudent, firstName: 'Jane' };
      mockStudentService.updateStudent.mockReturnValue(of(updatedStudent));

      spyOn(component.updated, 'emit');

      component.form.patchValue({ firstName: 'Jane' });
      component.save();
      fixture.detectChanges();

      expect(component.updated.emit).toHaveBeenCalledWith(updatedStudent);
    });

    it('should show success message on successful save', () => {
      component.save();
      fixture.detectChanges();

      expect(mockNotificationService.success).toHaveBeenCalledWith(
        'Student information updated successfully',
        undefined,
        3000,
      );
    });

    it('should show error message on save failure', () => {
      const errorDetails: ProblemDetails = {
        title: 'Error',
        detail: 'Update failed',
        status: 400,
      };
      mockStudentService.updateStudent.mockReturnValue(throwError(() => errorDetails));

      component.save();
      fixture.detectChanges();

      expect(component.error()).toEqual(errorDetails);
      expect(mockNotificationService.error).toHaveBeenCalledWith('Update failed', undefined, 5000);
    });
  });

  describe('Validation (AC #3, #4)', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.enableEdit();
      fixture.detectChanges();
    });

    it('should require first name', () => {
      component.form.patchValue({ firstName: '' });
      component.form.get('firstName')?.markAsTouched();
      fixture.detectChanges();

      expect(component.hasError('firstName', 'required')).toBe(true);
      expect(component.form.valid).toBe(false);
    });

    it('should require last name', () => {
      component.form.patchValue({ lastName: '' });
      component.form.get('lastName')?.markAsTouched();
      fixture.detectChanges();

      expect(component.hasError('lastName', 'required')).toBe(true);
      expect(component.form.valid).toBe(false);
    });

    it('should validate first name max length', () => {
      component.form.patchValue({ firstName: 'A'.repeat(51) });
      component.form.get('firstName')?.markAsTouched();
      fixture.detectChanges();

      expect(component.hasError('firstName', 'maxlength')).toBe(true);
    });

    it('should validate last name max length', () => {
      component.form.patchValue({ lastName: 'A'.repeat(51) });
      component.form.get('lastName')?.markAsTouched();
      fixture.detectChanges();

      expect(component.hasError('lastName', 'maxlength')).toBe(true);
    });

    it('should validate grade max length', () => {
      component.form.patchValue({ grade: 'A'.repeat(6) });
      component.form.get('grade')?.markAsTouched();
      fixture.detectChanges();

      expect(component.hasError('grade', 'maxlength')).toBe(true);
    });

    it('should validate notes max length', () => {
      component.form.patchValue({ generalNote: 'A'.repeat(256) });
      component.form.get('generalNote')?.markAsTouched();
      fixture.detectChanges();

      expect(component.hasError('generalNote', 'maxlength')).toBe(true);
    });

    it('should not save when form is invalid', () => {
      component.form.patchValue({ firstName: '' });
      const originalSpy = mockStudentService.updateStudent;
      mockStudentService.updateStudent = jest.fn();

      component.save();
      fixture.detectChanges();

      expect(mockStudentService.updateStudent).not.toHaveBeenCalled();
      mockStudentService.updateStudent = originalSpy;
    });
  });

  describe('Helper Methods', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should get error message for required field', () => {
      component.form.get('firstName')?.setErrors({ required: true });
      component.form.get('firstName')?.markAsTouched();

      expect(component.getErrorMessage('firstName')).toBe('This field is required');
    });

    it('should get error message for maxlength', () => {
      component.form.get('firstName')?.setErrors({ maxlength: { requiredLength: 50 } });
      component.form.get('firstName')?.markAsTouched();

      expect(component.getErrorMessage('firstName')).toBe('Maximum length is 50 characters');
    });

    it('should return empty string for no errors', () => {
      component.form.get('firstName')?.setErrors(null);

      expect(component.getErrorMessage('firstName')).toBe('');
    });

    it('should format date for input', () => {
      const formatted = component.formatDateForInput('2015-03-15T00:00:00Z');
      expect(formatted).toContain('2015-03-15');
    });
  });

  describe('Loading State', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.enableEdit();
      fixture.detectChanges();
    });

    it('should show loading spinner when saving', () => {
      component.isSaving.set(true);
      fixture.detectChanges();

      const spinner = fixture.debugElement.query(By.css('.loading-spinner'));
      expect(spinner).toBeTruthy();
    });

    it('should disable save button when saving', () => {
      component.isSaving.set(true);
      fixture.detectChanges();

      const saveButton = fixture.debugElement
        .queryAll(By.css('button'))
        .find((b) => b.nativeElement.textContent.includes('Save'));
      expect(saveButton?.nativeElement.disabled).toBe(true);
    });
  });
});
