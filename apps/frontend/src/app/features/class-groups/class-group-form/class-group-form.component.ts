import { Component, inject, OnInit, input, output, ChangeDetectionStrategy, signal, DestroyRef, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { Subject, of } from 'rxjs';
import { ClassGroupService } from '@core/services/class-group.service';
import { SchoolService, type School } from '@core/services/school.service';
import { TruckService, type Truck } from '@core/services/truck.service';
import type {
  ClassGroup,
  CreateClassGroupRequest,
  UpdateClassGroupRequest,
  ScheduleConflict,
  CheckConflictsRequest,
} from '@features/class-groups/models/class-group.model';
import { DAY_OF_WEEK_OPTIONS, getDayOfWeekNumber } from '@features/class-groups/models/class-group.model';
import { NotificationService } from '@core/services/notification.service';
import { finalize } from 'rxjs';
import { ConflictBannerComponent } from '../conflict-banner/conflict-banner.component';

@Component({
  selector: 'app-class-group-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ConflictBannerComponent],
  templateUrl: './class-group-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassGroupFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly classGroupService = inject(ClassGroupService);
  private readonly schoolService = inject(SchoolService);
  private readonly truckService = inject(TruckService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Input: classGroupId is null for create, has value for edit
  readonly classGroupId = input<number | null>(null);

  // Outputs
  readonly submit = output<{ mode: 'create' | 'update'; classGroup: ClassGroup }>();
  readonly cancel = output<void>();

  // Form state
  form!: FormGroup;
  isLoading = signal(false);
  isSaving = signal(false);
  error = signal<string | null>(null);
  checkingConflicts = signal(false);
  conflicts = signal<ScheduleConflict[]>([]);

  // Computed: has conflicts flag
  protected readonly hasConflicts = computed(() => this.conflicts().length > 0);

  // Subject for debounced conflict checking
  private conflictCheckTrigger = new Subject<void>();

  // Dropdown data
  schools = signal<School[]>([]);
  // Computed: trucks from service (reactive to service updates)
  protected readonly trucks = computed(() => this.truckService.trucks());

  // Day of week options
  readonly dayOfWeekOptions = DAY_OF_WEEK_OPTIONS;

  ngOnInit(): void {
    this.initForm();
    this.loadSchools();
    this.loadTrucks();
    this.setupConflictChecking();

    // Load class group data if editing
    if (this.classGroupId()) {
      this.loadClassGroup(this.classGroupId()!);
    }
  }

  /**
   * Initialize the form
   */
  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(10)]], // XSD: 10 chars max
      schoolId: [null, [Validators.required]],
      truckId: [null],
      dayOfWeek: [1, [Validators.required]], // Default to Monday (1)
      startTime: ['', [Validators.required]],
      endTime: ['', [Validators.required]],
      sequence: [1, [Validators.required, Validators.min(1)]],
      notes: ['', Validators.maxLength(255)], // XSD: 255 chars max
    });

    // Watch for value changes to trigger conflict checks
    this.form.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged((prev, curr) =>
        prev.truckId === curr.truckId &&
        prev.dayOfWeek === curr.dayOfWeek &&
        prev.startTime === curr.startTime &&
        prev.endTime === curr.endTime
      ),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.triggerConflictCheck();
    });
  }

  /**
   * Load schools for dropdown
   */
  private loadSchools(): void {
    this.schoolService.getSchools().subscribe({
      next: (schools) => this.schools.set(schools.filter((s) => s.isActive)),
      error: (err) => {
        console.error('Error loading schools:', err);
        this.notificationService.error('Failed to load schools');
      },
    });
  }

  /**
   * Load trucks for dropdown
   */
  private loadTrucks(): void {
    // Just trigger loading - trucks signal is computed from service
    this.truckService.loadTrucks();
  }

  /**
   * Setup debounced conflict checking
   */
  private setupConflictChecking(): void {
    this.conflictCheckTrigger.pipe(
      debounceTime(300),
      switchMap(() => {
        const truckId = this.form.value.truckId;
        const dayOfWeek = this.form.value.dayOfWeek;
        const startTime = this.form.value.startTime;
        const endTime = this.form.value.endTime;

        // Only check if we have all required fields and a truck is assigned
        if (!truckId || !dayOfWeek || !startTime || !endTime) {
          this.conflicts.set([]);
          return of({ hasConflicts: false, conflicts: [] });
        }

        this.checkingConflicts.set(true);

        const request: CheckConflictsRequest = {
          truckId,
          dayOfWeek,
          startTime: `${startTime}:00`,
          endTime: `${endTime}:00`,
          excludeId: this.classGroupId() ?? undefined,
        };

        return this.classGroupService.checkConflicts(request);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (response) => {
        this.conflicts.set(response.conflicts);
        this.checkingConflicts.set(false);
      },
      error: (err) => {
        console.error('Error checking conflicts:', err);
        this.conflicts.set([]);
        this.checkingConflicts.set(false);
      },
    });
  }

  /**
   * Trigger conflict check
   */
  private triggerConflictCheck(): void {
    this.conflictCheckTrigger.next();
  }

  /**
   * Load class group data for editing
   */
  private loadClassGroup(id: number): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.classGroupService.getClassGroup(id).pipe(
      finalize(() => this.isLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (classGroup) => {
        this.form.patchValue({
          name: classGroup.name,
          schoolId: classGroup.schoolId,
          truckId: classGroup.truckId || null,
          dayOfWeek: classGroup.dayOfWeek,
          startTime: classGroup.startTime.substring(0, 5), // "HH:mm:ss" -> "HH:mm"
          endTime: classGroup.endTime.substring(0, 5),
          sequence: classGroup.sequence,
          notes: classGroup.notes || '',
        });
        // Trigger conflict check after loading
        this.triggerConflictCheck();
      },
      error: (err) => {
        console.error('Error loading class group:', err);
        this.error.set('Failed to load class group. Please try again.');
        this.notificationService.error('Failed to load class group');
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

    // Validate time range
    const startTime = this.form.value.startTime;
    const endTime = this.form.value.endTime;
    if (endTime <= startTime) {
      this.error.set('End time must be after start time');
      return;
    }

    // Block save if there are conflicts
    if (this.hasConflicts()) {
      this.error.set('Please resolve scheduling conflicts before saving');
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);

    const formValue = this.form.value;

    if (this.classGroupId()) {
      // Update existing class group
      const updateRequest: UpdateClassGroupRequest = {
        name: formValue.name,
        schoolId: formValue.schoolId,
        truckId: formValue.truckId || undefined,
        dayOfWeek: formValue.dayOfWeek,
        startTime: `${formValue.startTime}:00`,
        endTime: `${formValue.endTime}:00`,
        sequence: formValue.sequence,
        notes: formValue.notes || undefined,
        isActive: true,
      };

      this.classGroupService.updateClassGroup(this.classGroupId()!, updateRequest).pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (classGroup) => {
          this.submit.emit({ mode: 'update', classGroup });
        },
        error: (err) => {
          console.error('Update error:', err);
          this.error.set(err.error?.detail || 'Failed to update class group');
          this.notificationService.error('Failed to update class group');
        },
      });
    } else {
      // Create new class group
      const createRequest: CreateClassGroupRequest = {
        name: formValue.name,
        schoolId: formValue.schoolId,
        truckId: formValue.truckId || undefined,
        dayOfWeek: formValue.dayOfWeek,
        startTime: `${formValue.startTime}:00`,
        endTime: `${formValue.endTime}:00`,
        sequence: formValue.sequence,
        notes: formValue.notes || undefined,
      };

      this.classGroupService.createClassGroup(createRequest).pipe(
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (classGroup) => {
          this.submit.emit({ mode: 'create', classGroup });
        },
        error: (err) => {
          console.error('Create error:', err);
          this.error.set(err.error?.detail || 'Failed to create class group');
          this.notificationService.error('Failed to create class group');
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
    if (field.hasError('min')) {
      return 'Value must be at least 1';
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
    return this.classGroupId() ? 'Edit Class Group' : 'Add Class Group';
  }

  /**
   * Get submit button text based on mode
   */
  protected get submitButtonText(): string {
    return this.isSaving() ? 'Saving...' : this.classGroupId() ? 'Update Class Group' : 'Create Class Group';
  }

  /**
   * Get day of week number for form value
   */
  protected getDayOfWeekNumber(dayName: string): number {
    return getDayOfWeekNumber(dayName);
  }
}
