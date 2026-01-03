import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, type OnInit, signal } from '@angular/core';
import {
  type AbstractControl,
  type FormArray,
  FormBuilder,
  type FormGroup,
  ReactiveFormsModule,
  type ValidationErrors,
  Validators,
} from '@angular/forms';

@Component({
  selector: 'app-forms',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './forms.component.html',
  styleUrls: ['./forms.component.css'],
})
export class FormsComponent implements OnInit {
  private fb = inject(FormBuilder);

  registrationForm!: FormGroup;
  contactForm!: FormGroup;
  showRegistrationSuccess = signal(false);
  showContactSuccess = signal(false);

  categoryOptions = [
    { value: 'bug', label: 'Bug' },
    { value: 'feature', label: 'Feature' },
    { value: 'question', label: 'Question' },
  ];

  ngOnInit(): void {
    this.initForms();
  }

  private initForms(): void {
    // Registration Form
    this.registrationForm = this.fb.group(
      {
        name: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(8), this.passwordValidator]],
        confirmPassword: ['', Validators.required],
        role: ['', Validators.required],
        terms: [false, Validators.requiredTrue],
      },
      { validators: this.passwordMatchValidator },
    );

    // Contact Form
    this.contactForm = this.fb.group({
      subject: ['', [Validators.required, Validators.minLength(5)]],
      priority: ['medium'],
      categories: this.fb.array(this.categoryOptions.map(() => this.fb.control(false))),
      message: ['', [Validators.required, Validators.minLength(10)]],
    });
  }

  // Custom password validator
  private passwordValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumber = /[0-9]/.test(value);

    const valid = hasUpperCase && hasLowerCase && hasNumber;
    return valid ? null : { passwordStrength: true };
  }

  // Password match validator
  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registrationForm.get(fieldName);
    return !!(field?.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.registrationForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors.required) return 'This field is required';
    if (field.errors.email) return 'Invalid email address';
    if (field.errors.minlength)
      return `Minimum ${field.errors.minlength.requiredLength} characters required`;
    if (field.errors.passwordStrength)
      return 'Password must contain uppercase, lowercase, and number';
    if (field.errors.passwordMismatch) return 'Passwords do not match';
    if (field.errors.requiredTrue) return 'You must accept the terms';

    return 'Invalid value';
  }

  isContactFieldInvalid(fieldName: string): boolean {
    const field = this.contactForm.get(fieldName);
    return !!(field?.invalid && field.touched);
  }

  getContactFieldError(fieldName: string): string {
    const field = this.contactForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors.required) return 'This field is required';
    if (field.errors.minlength)
      return `Minimum ${field.errors.minlength.requiredLength} characters required`;

    return 'Invalid value';
  }

  onRegisterSubmit(): void {
    if (this.registrationForm.valid) {
      console.log('Registration form submitted:', this.registrationForm.value);
      this.showRegistrationSuccess.set(true);
      this.registrationForm.reset();
      setTimeout(() => this.showRegistrationSuccess.set(false), 5000);
    }
  }

  onContactSubmit(): void {
    if (this.contactForm.valid) {
      // Get selected categories
      const formValue = this.contactForm.value;
      const selectedCategories = this.categoryOptions
        .map((option, index) => (formValue.categories[index] ? option.value : null))
        .filter((value): value is string => value !== null);

      const payload = {
        ...formValue,
        categories: selectedCategories,
      };

      console.log('Contact form submitted:', payload);
      this.showContactSuccess.set(true);
      this.contactForm.reset({ priority: 'medium' });
      // Reset categories FormArray
      const categoriesArray = this.contactForm.get('categories') as FormArray;
      categoriesArray.controls.forEach((control) => {
        control.setValue(false);
      });
      setTimeout(() => this.showContactSuccess.set(false), 5000);
    }
  }

  get categoriesFormArray(): FormArray {
    return this.contactForm.get('categories') as FormArray;
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      console.log('File selected:', file.name);
    }
  }
}
