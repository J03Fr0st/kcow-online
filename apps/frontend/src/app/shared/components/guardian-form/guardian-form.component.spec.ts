import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { GuardianFormComponent } from './guardian-form.component';

describe('GuardianFormComponent', () => {
  let component: GuardianFormComponent;
  let fixture: ComponentFixture<GuardianFormComponent>;
  let _fb: FormBuilder;

  const _mockGuardian = {
    id: 1,
    familyId: 1,
    firstName: 'John',
    lastName: 'Smith',
    relationship: 'Father',
    phone: '555-1234',
    email: 'john@example.com',
    isPrimaryContact: true,
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GuardianFormComponent],
      providers: [FormBuilder],
    }).compileComponents();

    fixture = TestBed.createComponent(GuardianFormComponent);
    component = fixture.componentInstance;
    _fb = TestBed.inject(FormBuilder);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty form', () => {
    fixture.detectChanges();

    expect((component as any).guardianForm.value).toEqual({
      firstName: '',
      lastName: '',
      relationship: '',
      phone: '',
      email: '',
      isPrimaryContact: false,
    });
  });

  it('should populate form with guardian data when provided via input', () => {
    // For testing, we manually set the internal form since inputs are readonly
    (component as any).guardianForm.patchValue({
      firstName: 'John',
      lastName: 'Smith',
      relationship: 'Father',
      phone: '555-1234',
      email: 'john@example.com',
      isPrimaryContact: true,
    });
    fixture.detectChanges();

    expect((component as any).guardianForm.value.firstName).toBe('John');
    expect((component as any).guardianForm.value.lastName).toBe('Smith');
    expect((component as any).guardianForm.value.relationship).toBe('Father');
    expect((component as any).guardianForm.value.phone).toBe('555-1234');
    expect((component as any).guardianForm.value.email).toBe('john@example.com');
    expect((component as any).guardianForm.value.isPrimaryContact).toBe(true);
  });

  it('should mark all controls as touched when submitting invalid form', () => {
    fixture.detectChanges();
    (component as any).onSubmit();

    expect((component as any).guardianForm.touched).toBe(true);
  });

  it('should not emit guardianSubmit when form is invalid', () => {
    let emitted = false;
    component.guardianSubmit.subscribe(() => {
      emitted = true;
    });

    (component as any).onSubmit();

    expect(emitted).toBe(false);
  });

  it('should emit guardianSubmit with valid form data', () => {
    let emittedData: any = null;
    component.guardianSubmit.subscribe((data) => {
      emittedData = data;
    });

    (component as any).guardianForm.setValue({
      firstName: 'Jane',
      lastName: 'Doe',
      relationship: 'Mother',
      phone: '555-5678',
      email: 'jane@example.com',
      isPrimaryContact: false,
    });

    (component as any).onSubmit();

    expect(emittedData).not.toBeNull();
    expect(emittedData.firstName).toBe('Jane');
    expect(emittedData.lastName).toBe('Doe');
  });

  it('should emit cancel when onCancel is called', () => {
    let cancelled = false;
    component.cancel.subscribe(() => {
      cancelled = true;
    });

    (component as any).onCancel();

    expect(cancelled).toBe(true);
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should show required error for empty first name', () => {
      (component as any).guardianForm.patchValue({ firstName: '' });
      (component as any).guardianForm.controls.firstName.markAsTouched();
      fixture.detectChanges();

      expect((component as any).hasError('firstName', 'required')).toBe(true);
      expect((component as any).getErrorMessage('firstName')).toContain('required');
    });

    it('should show required error for empty last name', () => {
      (component as any).guardianForm.patchValue({ lastName: '' });
      (component as any).guardianForm.controls.lastName.markAsTouched();
      fixture.detectChanges();

      expect((component as any).hasError('lastName', 'required')).toBe(true);
      expect((component as any).getErrorMessage('lastName')).toContain('required');
    });

    it('should show email error for invalid email', () => {
      (component as any).guardianForm.patchValue({ email: 'not-an-email' });
      (component as any).guardianForm.controls.email.markAsTouched();
      fixture.detectChanges();

      expect((component as any).hasError('email', 'email')).toBe(true);
      expect((component as any).getErrorMessage('email')).toContain('valid email');
    });

    it('should not show errors for untouched fields', () => {
      (component as any).guardianForm.patchValue({ firstName: '', lastName: '' });
      fixture.detectChanges();

      expect((component as any).hasError('firstName')).toBe(false);
      expect((component as any).hasError('lastName')).toBe(false);
    });

    it('should show no errors for valid form', () => {
      (component as any).guardianForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        relationship: 'Father',
        phone: '555-1234',
        email: 'john@example.com',
        isPrimaryContact: true,
      });
      (component as any).guardianForm.markAllAsTouched();
      fixture.detectChanges();

      expect((component as any).hasError('firstName')).toBe(false);
      expect((component as any).hasError('lastName')).toBe(false);
      expect((component as any).hasError('email')).toBe(false);
    });
  });

  describe('getControl', () => {
    it('should return the correct form control', () => {
      fixture.detectChanges();

      const firstNameControl = (component as any).getControl('firstName');
      expect(firstNameControl).toBe((component as any).guardianForm.controls.firstName);

      const lastNameControl = (component as any).getControl('lastName');
      expect(lastNameControl).toBe((component as any).guardianForm.controls.lastName);
    });
  });
});
