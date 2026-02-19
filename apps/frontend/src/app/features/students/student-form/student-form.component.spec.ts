import { type ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { FamilyService } from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import { SchoolService } from '@core/services/school.service';
import { type ProblemDetails, type Student, StudentService } from '@core/services/student.service';
import { of, throwError } from 'rxjs';
import { StudentFormComponent } from './student-form.component';

describe('StudentFormComponent', () => {
  let component: StudentFormComponent;
  let fixture: ComponentFixture<StudentFormComponent>;
  let mockStudentService: jest.Mocked<Partial<StudentService>>;
  let mockFamilyService: jest.Mocked<Partial<FamilyService>>;
  let mockSchoolService: jest.Mocked<Partial<SchoolService>>;
  let mockNotificationService: jest.Mocked<Partial<NotificationService>>;

  const mockStudent: Student = {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    dateOfBirth: '2010-05-15',
    gender: 'M',
    language: 'en',
    grade: '5',
    reference: 'REF001',
    photoUrl: 'https://example.com/photo.jpg',
    schoolId: 1,
    familyId: 1,
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    schoolName: 'Test School',
    familyName: 'Doe Family',
  };

  beforeEach(async () => {
    mockStudentService = {
      getStudentById: jest.fn().mockReturnValue(of(mockStudent)),
      createStudent: jest.fn().mockReturnValue(of(mockStudent)),
      updateStudent: jest.fn().mockReturnValue(of(mockStudent)),
    };

    mockFamilyService = {
      getFamilyById: jest.fn().mockReturnValue(
        of({
          id: 1,
          familyName: 'Doe Family',
          guardians: [],
          isActive: true,
          createdAt: '',
          updatedAt: '',
        }),
      ),
      getActiveFamilies: jest.fn().mockReturnValue(of([])),
    };

    mockSchoolService = {
      getActiveSchools: jest.fn().mockReturnValue(of([])),
      schools: jest.fn().mockReturnValue([]) as any,
    };

    mockNotificationService = {
      success: jest.fn(),
      error: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [StudentFormComponent, ReactiveFormsModule],
      providers: [
        { provide: StudentService, useValue: mockStudentService },
        { provide: FamilyService, useValue: mockFamilyService },
        { provide: SchoolService, useValue: mockSchoolService },
        { provide: NotificationService, useValue: mockNotificationService },
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: jest.fn().mockReturnValue(null),
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StudentFormComponent);
    component = fixture.componentInstance;
  });

  describe('Form Rendering', () => {
    it('should create', () => {
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should render form with all required fields', () => {
      fixture.detectChanges();

      const firstNameInput = fixture.debugElement.query(
        By.css('input[formControlName="firstName"]'),
      );
      const lastNameInput = fixture.debugElement.query(By.css('input[formControlName="lastName"]'));
      const dateOfBirthInput = fixture.debugElement.query(
        By.css('input[formControlName="dateOfBirth"]'),
      );
      const genderSelect = fixture.debugElement.query(By.css('select[formControlName="gender"]'));
      const languageSelect = fixture.debugElement.query(
        By.css('select[formControlName="language"]'),
      );
      const _gradeInput = fixture.debugElement.query(By.css('input[formControlName="grade"]'));
      const referenceInput = fixture.debugElement.query(
        By.css('input[formControlName="reference"]'),
      );

      expect(firstNameInput).toBeTruthy();
      expect(lastNameInput).toBeTruthy();
      expect(dateOfBirthInput).toBeTruthy();
      expect(genderSelect).toBeTruthy();
      expect(languageSelect).toBeTruthy();
      // Grade is in Class Groups tab, checked separately
      // expect(gradeInput).toBeTruthy();
      expect(referenceInput).toBeTruthy();
    });

    it('should render grade field when Class Groups tab is active', () => {
      fixture.detectChanges();
      const tabs = fixture.debugElement.queryAll(By.css('.tab'));
      const classGroupsTab = tabs.find(
        (t) => t.nativeElement.textContent.trim() === 'Class Groups',
      );
      classGroupsTab?.nativeElement.click();
      fixture.detectChanges();

      const gradeInput = fixture.debugElement.query(By.css('input[formControlName="grade"]'));
      expect(gradeInput).toBeTruthy();
    });

    it('should render school select component', () => {
      fixture.detectChanges();
      const schoolSelect = fixture.debugElement.query(By.css('app-school-select'));
      expect(schoolSelect).toBeTruthy();
    });

    it('should render family select component', () => {
      fixture.detectChanges();
      const familySelect = fixture.debugElement.query(By.css('app-family-select'));
      expect(familySelect).toBeTruthy();
    });

    it('should render Save and Cancel buttons', () => {
      fixture.detectChanges();
      const saveButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      const cancelButton = fixture.debugElement.query(By.css('button[type="button"]'));

      expect(saveButton).toBeTruthy();
      expect(cancelButton).toBeTruthy();
      expect(saveButton.nativeElement.textContent).toContain('Save');
      expect(cancelButton.nativeElement.textContent).toContain('Cancel');
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should require firstName', () => {
      const control = component.form.get('firstName');
      control?.setValue('');
      control?.markAsTouched();
      expect(control?.hasError('required')).toBe(true);
    });

    it('should require lastName', () => {
      const control = component.form.get('lastName');
      control?.setValue('');
      control?.markAsTouched();
      expect(control?.hasError('required')).toBe(true);
    });

    it('should require schoolId', () => {
      const control = component.form.get('schoolId');
      control?.setValue(null);
      control?.markAsTouched();
      expect(control?.hasError('required')).toBe(true);
    });

    it('should not require familyId (optional field)', () => {
      const control = component.form.get('familyId');
      control?.setValue(null);
      expect(control?.valid).toBe(true);
    });

    it('should disable Save button when form is invalid', () => {
      // Form starts invalid (required fields empty)
      fixture.detectChanges();
      const saveButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      expect(saveButton.nativeElement.disabled).toBe(true);
    });

    it('should enable Save button when form is valid', () => {
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        schoolId: 1,
      });
      fixture.detectChanges();
      const saveButton = fixture.debugElement.query(By.css('button[type="submit"]'));
      expect(saveButton.nativeElement.disabled).toBe(false);
    });

    it('should show inline error messages for invalid fields', () => {
      const control = component.form.get('firstName');
      control?.setValue('');
      control?.markAsTouched();
      fixture.detectChanges();

      const errorLabel = fixture.debugElement.query(By.css('.text-error'));
      expect(errorLabel).toBeTruthy();
    });
  });

  describe('Create Flow', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should call createStudent on valid form submission in create mode', fakeAsync(() => {
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        schoolId: 1,
      });

      component.onSubmit();
      tick();

      expect(mockStudentService.createStudent).toHaveBeenCalled();
    }));

    it('should show success notification on successful create', fakeAsync(() => {
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        schoolId: 1,
      });

      component.onSubmit();
      tick();

      expect(mockNotificationService.success).toHaveBeenCalledWith('Student created successfully');
    }));

    it('should emit saved event on successful create', fakeAsync(() => {
      const savedSpy = jest.fn();
      component.saved.subscribe(savedSpy);

      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        schoolId: 1,
      });

      component.onSubmit();
      tick();

      expect(savedSpy).toHaveBeenCalledWith(mockStudent);
    }));

    it('should show error notification on failed create', fakeAsync(() => {
      const error: ProblemDetails = { title: 'Error', detail: 'Creation failed' };
      mockStudentService.createStudent = jest.fn().mockReturnValue(throwError(() => error));

      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        schoolId: 1,
      });

      component.onSubmit();
      tick();

      expect(mockNotificationService.error).toHaveBeenCalledWith('Creation failed');
    }));
  });

  describe('Edit Flow', () => {
    beforeEach(async () => {
      // Reconfigure with route param for edit mode
      await TestBed.resetTestingModule();
      await TestBed.configureTestingModule({
        imports: [StudentFormComponent, ReactiveFormsModule],
        providers: [
          { provide: StudentService, useValue: mockStudentService },
          { provide: FamilyService, useValue: mockFamilyService },
          { provide: SchoolService, useValue: mockSchoolService },
          { provide: NotificationService, useValue: mockNotificationService },
          provideRouter([]),
          {
            provide: ActivatedRoute,
            useValue: {
              snapshot: {
                paramMap: {
                  get: jest.fn().mockReturnValue('1'),
                },
              },
            },
          },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(StudentFormComponent);
      component = fixture.componentInstance;
    });

    it('should load student data in edit mode', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      expect(mockStudentService.getStudentById).toHaveBeenCalledWith(1);
    }));

    it('should pre-populate form with existing student values', fakeAsync(() => {
      fixture.detectChanges();
      tick();
      fixture.detectChanges();

      expect(component.form.get('firstName')?.value).toBe('John');
      expect(component.form.get('lastName')?.value).toBe('Doe');
      expect(component.form.get('schoolId')?.value).toBe(1);
      expect(component.form.get('familyId')?.value).toBe(1);
    }));

    it('should call updateStudent on form submission in edit mode', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      component.form.patchValue({ firstName: 'Jane' });
      component.onSubmit();
      tick();

      expect(mockStudentService.updateStudent).toHaveBeenCalled();
    }));

    it('should show success notification on successful update', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      component.onSubmit();
      tick();

      expect(mockNotificationService.success).toHaveBeenCalledWith('Student updated successfully');
    }));
  });

  describe('Cancel Flow', () => {
    it('should emit cancelled event on cancel button click', () => {
      fixture.detectChanges();
      const cancelledSpy = jest.fn();
      component.cancelled.subscribe(cancelledSpy);

      component.onCancel();

      expect(cancelledSpy).toHaveBeenCalled();
    });
  });
});
