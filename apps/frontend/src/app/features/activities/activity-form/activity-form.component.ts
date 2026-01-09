import { Component, inject, OnInit, input, output, ChangeDetectionStrategy, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivityService } from '@core/services/activity.service';
import type { Activity, CreateActivityRequest, UpdateActivityRequest } from '@features/activities/models/activity.model';
import { NotificationService } from '@core/services/notification.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-activity-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './activity-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivityFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly activityService = inject(ActivityService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Input: activityId is null for create, has value for edit
  readonly activityId = input<number | null>(null);

  // Outputs
  readonly submit = output<Event>();
  readonly cancel = output<void>();

  // Form state
  form!: FormGroup;
  isLoading = signal(false);
  isSaving = signal(false);
  error = signal<string | null>(null);
  iconPreview = signal<string | null>(null);

  ngOnInit(): void {
    this.initForm();

    // Load activity data if editing
    if (this.activityId()) {
      this.loadActivity(this.activityId()!);
    }
  }

  /**
   * Initialize the form
   */
  private initForm(): void {
    this.form = this.fb.group({
      code: ['', [Validators.maxLength(255)]],
      name: ['', [Validators.maxLength(255)]],
      description: [''],
      folder: ['', [Validators.maxLength(255)]],
      gradeLevel: ['', [Validators.maxLength(255)]],
      icon: [''],
    });
  }

  /**
   * Load activity data for editing
   */
  private loadActivity(id: number): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.activityService.getActivity(id).pipe(
      finalize(() => this.isLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (activity) => {
        this.form.patchValue({
          code: activity.code ?? '',
          name: activity.name ?? '',
          description: activity.description ?? '',
          folder: activity.folder ?? '',
          gradeLevel: activity.gradeLevel ?? '',
          icon: activity.icon ?? '',
        });

        // Set icon preview if icon exists
        if (activity.icon) {
          this.iconPreview.set(`data:image/png;base64,${activity.icon}`);
        }
      },
      error: (err) => {
        console.error('Error loading activity:', err);
        this.error.set('Failed to load activity. Please try again.');
        this.notificationService.error('Failed to load activity');
      },
    });
  }

  /**
   * Handle icon file selection
   */
  protected onIconSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Validate file type
      if (file.type !== 'image/png') {
        this.notificationService.error('Please select a PNG image file');
        input.value = ''; // Reset input
        return;
      }

      // Validate file size (max 2MB)
      if (file.size > 2 * 1024 * 1024) {
        this.notificationService.error('Image file must be less than 2MB');
        input.value = ''; // Reset input
        return;
      }

      const reader = new FileReader();
      reader.onload = () => {
        // reader.result is base64 data URL
        const result = reader.result as string;
        const base64 = result.split(',')[1];
        this.form.patchValue({ icon: base64 });
        this.iconPreview.set(result);
        input.value = ''; // Reset input so same file can be selected again if needed
      };
      reader.onerror = () => {
        this.notificationService.error('Failed to read image file');
        input.value = ''; // Reset input
      };
      reader.readAsDataURL(file);
    }
  }

  /**
   * Clear icon
   */
  protected clearIcon(): void {
    this.form.patchValue({ icon: '' });
    this.iconPreview.set(null);
  }

  /**
   * Submit form (create or update)
   */
  protected submitForm(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);

    const formValue = this.form.value;

    if (this.activityId()) {
      // Update existing activity
      const updateRequest: UpdateActivityRequest = {
        code: formValue.code || undefined,
        name: formValue.name || undefined,
        description: formValue.description || undefined,
        folder: formValue.folder || undefined,
        gradeLevel: formValue.gradeLevel || undefined,
        icon: formValue.icon || undefined,
      };

      this.activityService.updateActivity(this.activityId()!, updateRequest).pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (activity) => {
          this.submit.emit(new CustomEvent('submit', { detail: { mode: 'update', activity } }));
        },
        error: (err) => {
          console.error('Update error:', err);
          this.error.set(err.error?.detail || 'Failed to update activity');
          this.notificationService.error('Failed to update activity');
        },
      });
    } else {
      // Create new activity
      const createRequest: CreateActivityRequest = {
        code: formValue.code || undefined,
        name: formValue.name || undefined,
        description: formValue.description || undefined,
        folder: formValue.folder || undefined,
        gradeLevel: formValue.gradeLevel || undefined,
        icon: formValue.icon || undefined,
      };

      this.activityService.createActivity(createRequest).pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (activity) => {
          this.submit.emit(new CustomEvent('submit', { detail: { mode: 'create', activity } }));
        },
        error: (err) => {
          console.error('Create error:', err);
          this.error.set(err.error?.detail || 'Failed to create activity');
          this.notificationService.error('Failed to create activity');
        },
      });
    }
  }

  /**
   * Handle cancel action
   */
  protected onCancel(): void {
    this.cancel.emit();
  }

  /**
   * Check if field has error
   */
  protected hasError(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Get error message for field
   */
  protected getErrorMessage(fieldName: string): string {
    const field = this.form.get(fieldName);
    if (!field) return '';

    if (field.hasError('required')) {
      return 'This field is required';
    }
    if (field.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength']?.requiredLength || 0;
      return `Maximum length is ${maxLength} characters`;
    }

    return 'Invalid input';
  }

  /**
   * Mark all form controls as touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  /**
   * Get form title based on mode
   */
  protected get title(): string {
    return this.activityId() ? 'Edit Activity' : 'Add Activity';
  }

  /**
   * Get submit button text based on mode
   */
  protected get submitButtonText(): string {
    return this.isSaving() ? 'Saving...' : this.activityId() ? 'Update Activity' : 'Create Activity';
  }
}
