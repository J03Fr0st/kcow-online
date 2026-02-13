import { ChangeDetectionStrategy, Component, inject, input, output, EventEmitter, OnInit, OnDestroy, signal, WritableSignal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { StudentService, type Student, type UpdateStudentRequest, type ProblemDetails } from '@core/services/student.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NotificationService } from '@core/services/notification.service';

interface StudentForm {
    firstName: string | null;
    lastName: string | null;
    dateOfBirth: string | null;
    gender: string | null;
    language: string | null;
    grade: string | null;
    generalNote: string | null;
}

@Component({
    selector: 'app-child-info-tab',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './child-info-tab.component.html',
    styleUrls: ['./child-info-tab.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChildInfoTabComponent implements OnInit, OnDestroy {
    private readonly formBuilder = inject(FormBuilder);
    private readonly studentService = inject(StudentService);
    private readonly notificationService = inject(NotificationService);
    private readonly destroyRef = inject(DestroyRef);

    // Input signal for student data
    readonly student = input.required<Student>();

    // Output event for when student is updated
    readonly updated = new EventEmitter<Student>();

    // Form state
    protected form!: FormGroup;
    protected isEditing = signal<boolean>(false);
    protected isSaving = signal<boolean>(false);
    protected error = signal<ProblemDetails | null>(null);

    // Available options
    protected readonly genderOptions = [
        { value: 'M', label: 'Male' },
        { value: 'F', label: 'Female' },
    ];

    protected readonly languageOptions = [
        { value: 'Afr', label: 'Afrikaans' },
        { value: 'Eng', label: 'English' },
    ];

    ngOnInit(): void {
        this.initializeForm();
        this.populateForm();
    }

    ngOnDestroy(): void {
        // Cleanup handled by takeUntilDestroyed in save method
    }

    /**
     * Initialize reactive form
     */
    private initializeForm(): void {
        this.form = this.formBuilder.group<StudentForm>({
            firstName: [null, [Validators.required, Validators.maxLength(50)]],
            lastName: [null, [Validators.required, Validators.maxLength(50)]],
            dateOfBirth: [null],
            gender: [null],
            language: [null],
            grade: [null, [Validators.maxLength(5)]],
            generalNote: [null, [Validators.maxLength(255)]],
        });
    }

    /**
     * Populate form with student data
     */
    private populateForm(): void {
        const s = this.student();
        this.form.patchValue({
            firstName: s.firstName || null,
            lastName: s.lastName || null,
            dateOfBirth: s.dateOfBirth || null,
            gender: s.gender || null,
            language: s.language || null,
            grade: s.grade || null,
            generalNote: s.generalNote || null,
        });
        this.form.disable();
    }

    /**
     * Enable edit mode
     */
    protected enableEdit(): void {
        this.isEditing.set(true);
        this.error.set(null);
        this.form.enable();
    }

    /**
     * Cancel edit mode
     */
    protected cancelEdit(): void {
        this.isEditing.set(false);
        this.error.set(null);
        this.populateForm();
        this.form.disable();
    }

    /**
     * Save changes
     */
    protected save(): void {
        if (this.form.invalid) {
            this.markFormGroupTouched(this.form);
            return;
        }

        this.isSaving.set(true);
        this.error.set(null);

        const formValue = this.form.value;
        const updateRequest: UpdateStudentRequest = {
            firstName: formValue.firstName || undefined,
            lastName: formValue.lastName || undefined,
            dateOfBirth: formValue.dateOfBirth || undefined,
            gender: formValue.gender || undefined,
            language: formValue.language || undefined,
            grade: formValue.grade || undefined,
            generalNote: formValue.generalNote || undefined,
        };

        this.studentService.updateStudent(this.student().id, updateRequest).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (updatedStudent) => {
                this.isSaving.set(false);
                this.isEditing.set(false);
                this.form.disable();
                this.updated.emit(updatedStudent);
                this.showSuccessMessage('Student information updated successfully');
            },
            error: (err: ProblemDetails) => {
                this.isSaving.set(false);
                this.error.set(err);
                this.showErrorMessage(err.detail || 'Failed to update student');
            },
        });
    }

    /**
     * Mark all form controls as touched to show validation errors
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
     * Check if a field has error
     */
    protected hasError(fieldName: string, errorType?: string): boolean {
        const field = this.form.get(fieldName);
        if (!field) return false;

        if (errorType) {
            return field.hasError(errorType) && field.touched;
        }
        return field.invalid && field.touched;
    }

    /**
     * Get error message for a field
     */
    protected getErrorMessage(fieldName: string): string {
        const field = this.form.get(fieldName);
        if (!field || !field.errors) return '';

        if (field.errors['required']) {
            return 'This field is required';
        }
        if (field.errors['maxlength']) {
            return `Maximum length is ${field.errors['maxlength'].requiredLength} characters`;
        }
        return 'Invalid value';
    }

    /**
     * Show success message
     */
    private showSuccessMessage(message: string): void {
        this.notificationService.success(message, undefined, 3000);
    }

    /**
     * Show error message
     */
    private showErrorMessage(message: string): void {
        this.notificationService.error(message, undefined, 5000);
    }

    /**
     * Format date for date input
     */
    protected formatDateForInput(dateString?: string): string {
        if (!dateString) return '';
        try {
            const date = new Date(dateString);
            return date.toISOString().split('T')[0];
        } catch {
            return dateString;
        }
    }
}
