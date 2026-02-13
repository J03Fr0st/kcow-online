import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, type FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  readonly user = this.authService.currentUser;
  readonly activeTab = signal<'overview' | 'security'>('overview');

  profileForm: FormGroup;
  passwordForm: FormGroup;
  isSaving = signal(false);
  saveSuccess = signal(false);
  passwordError = signal('');
  passwordSuccess = signal(false);

  constructor() {
    const user = this.user();
    this.profileForm = this.fb.group({
      name: [user?.name ?? '', [Validators.required, Validators.minLength(2)]],
      email: [user?.email ?? '', [Validators.required, Validators.email]],
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    });
  }

  get userInitials(): string {
    const user = this.user();
    if (!user) return 'U';
    const parts = user.name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return user.name.substring(0, 2).toUpperCase();
  }

  switchTab(tab: 'overview' | 'security'): void {
    this.activeTab.set(tab);
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.isSaving.set(true);
    this.saveSuccess.set(false);

    // TODO: Integrate with backend API to update user profile
    setTimeout(() => {
      this.isSaving.set(false);
      this.saveSuccess.set(true);
      setTimeout(() => this.saveSuccess.set(false), 3000);
    }, 500);
  }

  changePassword(): void {
    this.passwordError.set('');
    this.passwordSuccess.set(false);

    if (this.passwordForm.invalid) return;

    const { newPassword, confirmPassword } = this.passwordForm.value;
    if (newPassword !== confirmPassword) {
      this.passwordError.set('Passwords do not match');
      return;
    }

    // TODO: Integrate with backend API to change password
    this.passwordSuccess.set(true);
    this.passwordForm.reset();
    setTimeout(() => this.passwordSuccess.set(false), 3000);
  }

  logout(): void {
    this.authService.clearSessionAndRedirect();
  }

  navigateToSettings(): void {
    this.router.navigate(['/workspace-settings']);
  }
}
