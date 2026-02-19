import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, type OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import type { LoginRequest } from './models/login-request.model';

/**
 * Login Component
 *
 * Handles user authentication with email/password.
 * Displays error messages from failed login attempts.
 * Redirects to return URL or dashboard on success.
 *
 * @example
 * <app-login></app-login>
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  loginForm: FormGroup;
  errorMessage = '';
  isLoading = false;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    // Check if already authenticated
    if (this.authService.isAuthenticated()) {
      this.navigateToReturnUrl();
    }
  }

  /**
   * Handle form submission
   */
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched(this.loginForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Get form value before disabling
    const credentials: LoginRequest = this.loginForm.value;

    // Disable form controls during submission
    this.loginForm.disable();

    this.authService
      .login(credentials)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.navigateToReturnUrl();
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = this.extractErrorMessage(error);
          this.loginForm.enable(); // Re-enable form on error
          this.loginForm.get('password')?.setValue(''); // Clear password on error (AC #2)
        },
      });
  }

  /**
   * Extract user-friendly error message from error response
   */
  private extractErrorMessage(error: unknown): string {
    // Type guard for error object
    const isErrorObject = (
      err: unknown,
    ): err is {
      error?: { detail?: string; title?: string };
      status?: number;
      message?: string;
    } => {
      return typeof err === 'object' && err !== null;
    };

    if (!isErrorObject(error)) {
      return 'Login failed. Please try again';
    }

    // Check for ProblemDetails format
    if (error.error?.detail) {
      return error.error.detail;
    }

    if (error.error?.title) {
      return error.error.title;
    }

    // Check for HTTP error status
    if (error.status === 401) {
      return 'Invalid email or password';
    }

    if (error.status === 0) {
      return 'Network error. Please check your connection';
    }

    // Default error message
    return error.message || 'Login failed. Please try again';
  }

  /**
   * Navigate to return URL or dashboard
   */
  private navigateToReturnUrl(): void {
    // Check for return URL in query params
    const returnUrl =
      this.route.snapshot.queryParams['returnUrl'] || this.authService.getReturnUrl();

    this.router.navigateByUrl(returnUrl);
  }

  /**
   * Mark all controls as touched to show validation errors
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Get error message for email field
   */
  getEmailErrorMessage(): string {
    const emailControl = this.loginForm.get('email');
    if (!emailControl || !emailControl.touched) {
      return '';
    }

    if (emailControl.hasError('required')) {
      return 'Email is required';
    }

    if (emailControl.hasError('email')) {
      return 'Please enter a valid email address';
    }

    return '';
  }

  /**
   * Get error message for password field
   */
  getPasswordErrorMessage(): string {
    const passwordControl = this.loginForm.get('password');
    if (!passwordControl || !passwordControl.touched) {
      return '';
    }

    if (passwordControl.hasError('required')) {
      return 'Password is required';
    }

    if (passwordControl.hasError('minlength')) {
      return 'Password must be at least 6 characters';
    }

    return '';
  }
}
