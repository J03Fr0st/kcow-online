import { Component, inject, OnInit, input, output, ChangeDetectionStrategy, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TruckService, type Truck, type CreateTruckRequest, type UpdateTruckRequest, TRUCK_STATUS_OPTIONS } from '@core/services/truck.service';
import { NotificationService } from '@core/services/notification.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-truck-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './truck-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TruckFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly truckService = inject(TruckService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Input: truckId is null for create, has value for edit
  readonly truckId = input<number | null>(null);

  // Outputs
  readonly submit = output<Event>();
  readonly cancel = output<void>();

  // Form state
  form!: FormGroup;
  isLoading = signal(false);
  isSaving = signal(false);
  error = signal<string | null>(null);

  // Status options
  readonly statusOptions = TRUCK_STATUS_OPTIONS;

  ngOnInit(): void {
    this.initForm();

    // Load truck data if editing
    if (this.truckId()) {
      this.loadTruck(this.truckId()!);
    }
  }

  /**
   * Initialize the form
   */
  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      registrationNumber: ['', [Validators.required, Validators.maxLength(50)]],
      status: ['Active', Validators.required],
      notes: ['', Validators.maxLength(1000)],
    });
  }

  /**
   * Load truck data for editing
   */
  private loadTruck(id: number): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.truckService.getTruck(id).pipe(
      finalize(() => this.isLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (truck) => {
        this.form.patchValue({
          name: truck.name,
          registrationNumber: truck.registrationNumber,
          status: truck.status,
          notes: truck.notes || '',
        });
      },
      error: (err) => {
        console.error('Error loading truck:', err);
        this.error.set('Failed to load truck. Please try again.');
        this.notificationService.error('Failed to load truck');
      },
    });
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

    if (this.truckId()) {
      // Update existing truck
      const updateRequest: UpdateTruckRequest = {
        name: formValue.name,
        registrationNumber: formValue.registrationNumber,
        status: formValue.status,
        notes: formValue.notes || undefined,
      };

      this.truckService.updateTruck(this.truckId()!, updateRequest).pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (truck) => {
          this.submit.emit(new CustomEvent('submit', { detail: { mode: 'update', truck } }));
        },
        error: (err) => {
          console.error('Update error:', err);
          this.error.set(err.error?.detail || 'Failed to update truck');
          this.notificationService.error('Failed to update truck');
        },
      });
    } else {
      // Create new truck
      const createRequest: CreateTruckRequest = {
        name: formValue.name,
        registrationNumber: formValue.registrationNumber,
        status: formValue.status,
        notes: formValue.notes || undefined,
      };

      this.truckService.createTruck(createRequest).pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (truck) => {
          this.submit.emit(new CustomEvent('submit', { detail: { mode: 'create', truck } }));
        },
        error: (err) => {
          console.error('Create error:', err);
          this.error.set(err.error?.detail || 'Failed to create truck');
          this.notificationService.error('Failed to create truck');
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
    return this.truckId() ? 'Edit Truck' : 'Add Truck';
  }

  /**
   * Get submit button text based on mode
   */
  protected get submitButtonText(): string {
    return this.isSaving() ? 'Saving...' : this.truckId() ? 'Update Truck' : 'Create Truck';
  }
}
