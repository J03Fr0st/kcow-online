import { Component, inject, OnInit, DestroyRef, output, input, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { StudentService, type Student, type CreateStudentRequest, type UpdateStudentRequest, type ProblemDetails } from '@core/services/student.service';
import { SchoolSelectComponent } from '@shared/components/school-select/school-select.component';
import { ClassGroupSelectComponent } from '@shared/components/class-group-select/class-group-select.component';
import { FamilySelectComponent } from '@shared/components/family-select/family-select.component';
import { StudentAvatarComponent } from '@shared/components/student-avatar/student-avatar.component';
import { NotificationService } from '@core/services/notification.service';
import { ModalService } from '@core/services/modal.service';
import { CreateFamilyModalComponent } from '@shared/components/create-family-modal/create-family-modal.component';
import { FamilyService, type Family } from '@core/services/family.service';
import { firstValueFrom } from 'rxjs';

/**
 * Custom validator for positive integer (seat number)
 */
function positiveIntegerValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        if (!control.value) {
            return null; // Empty is valid (optional field)
        }

        const value = control.value;
        const num = Number(value);

        if (isNaN(num) || !/^[1-9]\d*$/.test(value)) {
            return { positiveInteger: true };
        }

        return null;
    };
}

@Component({
    selector: 'app-student-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        RouterLink,
        SchoolSelectComponent,
        ClassGroupSelectComponent,
        FamilySelectComponent,
        StudentAvatarComponent
    ],
    templateUrl: './student-form.component.html',
    styleUrls: ['./student-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StudentFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private studentService = inject(StudentService);
    private notificationService = inject(NotificationService);
    private destroyRef = inject(DestroyRef);
    private modalService = inject(ModalService);
    private familyService = inject(FamilyService);

    // Family state
    family = signal<Family | null>(null);

    // Track original student state
    private originalStudent: Student | null = null;
    private originalFamilyId: number | null = null;

    // Inputs
    studentId = input<number | null>(null);

    // Outputs
    saved = output<Student>();
    cancelled = output<void>();

    // Form state
    form!: FormGroup;
    isLoading = false;
    isSaving = false;
    error: ProblemDetails | null = null;
    activeTab = 'info';

    // Options
    genderOptions = [
        { value: 'Male', label: 'Male' },
        { value: 'Female', label: 'Female' },
        { value: 'Other', label: 'Other' },
    ];

    languageOptions = [
        { value: 'English', label: 'English' },
        { value: 'Afrikaans', label: 'Afrikaans' },
        { value: 'Zulu', label: 'Zulu' },
        { value: 'Xhosa', label: 'Xhosa' },
        { value: 'Other', label: 'Other' },
    ];

    ngOnInit(): void {
        this.initForm();

        // Check for ID from input or route
        const id = this.studentId() || this.route.snapshot.paramMap.get('id');
        if (id) {
            this.loadStudent(+id);
        }
    }

    /**
     * Initialize the form with all XSD fields
     */
    private initForm(): void {
        this.form = this.fb.group({
            // Required fields
            firstName: ['', [Validators.required, Validators.maxLength(100)]],
            lastName: ['', [Validators.required, Validators.maxLength(100)]],
            reference: ['', [Validators.required, Validators.maxLength(10)]],
            schoolId: [null, Validators.required],
            classGroupId: [null], // Class group linked to school
            familyId: [null],

            // Basic fields
            dateOfBirth: [''],
            gender: [''],
            language: [''],
            grade: [''],
            schoolName: [''], // Legacy XSD field - denormalized school name
            photoUrl: [''],
            status: [''],

            // Address fields
            address1: [''],
            address2: [''],
            postalCode: [''],

            // Academic & Transportation
            teacher: [''],
            classGroupCode: [''],
            attendingKcowAt: [''],
            aftercare: [''],
            truck: [''],
            seat: ['', positiveIntegerValidator()], // String to match XSD, but validated as positive integer
            homeTime: [''],
            terms: [''],
            extra: [''],

            // Financial
            financialCode: [''],
            charge: [null],
            deposit: [''],
            payDate: [''],
            familyCode: [''],
            sequence: [''],

            // T-Shirt Order 1
            tshirtCode: [''],
            tshirtSize1: [''],
            tshirtColor1: [''],
            tshirtDesign1: [''],

            // T-Shirt Order 2
            tshirtSize2: [''],
            tshirtColor2: [''],
            tshirtDesign2: [''],

            // Notes & Other
            generalNote: [''],
            indicator1: [''],
            indicator2: [''],
            printIdCard: [false],
        });

        // Setup cascading dropdown: when school changes, clear class group
        this.form.get('schoolId')?.valueChanges.subscribe((schoolId) => {
            const classGroupControl = this.form.get('classGroupId');
            if (!schoolId) {
                classGroupControl?.setValue(null);
                classGroupControl?.updateValueAndValidity();
            }
        });
    }

    /**
     * Load student data for edit mode
     */
    private loadStudent(id: number): void {
        this.isLoading = true;
        this.error = null;

        this.studentService.getStudentById(id).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (student) => {
                this.originalStudent = student;
                this.originalFamilyId = student.familyId || null;

                this.form.patchValue({
                    firstName: student.firstName,
                    lastName: student.lastName,
                    dateOfBirth: student.dateOfBirth || '',
                    gender: student.gender || '',
                    language: student.language || '',
                    grade: student.grade || '',
                    schoolName: student.schoolName || '', // Legacy XSD field
                    reference: student.reference || '',
                    photoUrl: student.photoUrl || '',
                    schoolId: student.schoolId,
                    classGroupId: student.classGroupId || null,
                    familyId: student.familyId || null,
                    status: student.status || '',
                });

                if (student.familyId) {
                    this.loadFamily(student.familyId);
                } else {
                    this.family.set(null);
                }

                this.isLoading = false;
            },
            error: (err: ProblemDetails) => {
                this.error = err;
                this.isLoading = false;
                this.notificationService.error(err.detail || 'Failed to load student');
            },
        });
    }

    /**
     * Get count of filled fields in a section (for badge display)
     */
    protected getFilledFieldCount(fieldNames: string[]): number {
        return fieldNames.filter(name => {
            const value = this.form.get(name)?.value;
            return value !== null && value !== '' && value !== undefined;
        }).length;
    }

    /**
     * Handle form submission
     */
    protected async onSubmit(): Promise<void> {
        if (this.form.invalid) {
            this.markFormGroupTouched(this.form);
            return;
        }

        const currentFamilyId = this.form.value.familyId;

        // Check if family changed in edit mode
        const isEditMode = !!(this.studentId() || this.route.snapshot.paramMap.get('id'));
        if (isEditMode && this.originalFamilyId !== currentFamilyId) {
            // Family changed - show confirmation
            const message = await this.getFamilyChangeMessage(this.originalFamilyId, currentFamilyId);
            const confirmed = await this.modalService.confirm({
                title: 'Change Family Assignment',
                message: message,
                confirmText: 'Change Family',
                cancelText: 'Cancel',
                confirmClass: 'btn-warning',
                size: 'sm'
            });

            if (!confirmed) {
                return; // User cancelled
            }
        }

        this.isSaving = true;
        this.error = null;

        const formValue = this.form.value;

        const id = this.studentId() || this.route.snapshot.paramMap.get('id');

        if (id) {
            // Update existing student
            const updateRequest: UpdateStudentRequest = {
                ...formValue,
                isActive: this.originalStudent?.isActive ?? true,
            };

            this.studentService.updateStudent(+id, updateRequest).pipe(
                takeUntilDestroyed(this.destroyRef)
            ).subscribe({
                next: (student) => {
                    this.isSaving = false;
                    this.notificationService.success('Student updated successfully');
                    this.saved.emit(student);
                },
                error: (err: ProblemDetails) => {
                    this.error = err;
                    this.isSaving = false;
                    this.notificationService.error(err.detail || 'Failed to update student');
                },
            });
        } else {
            // Create new student
            const createRequest: CreateStudentRequest = formValue;

            this.studentService.createStudent(createRequest).pipe(
                takeUntilDestroyed(this.destroyRef)
            ).subscribe({
                next: (student) => {
                    this.isSaving = false;
                    this.notificationService.success('Student created successfully');
                    this.saved.emit(student);
                },
                error: (err: ProblemDetails) => {
                    this.error = err;
                    this.isSaving = false;
                    this.notificationService.error(err.detail || 'Failed to create student');
                },
            });
        }
    }

    /**
     * Get message for family change confirmation
     */
    private async getFamilyChangeMessage(oldFamilyId: number | null, newFamilyId: number | null): Promise<string> {
        let oldName = 'No Family';
        let newName = 'No Family';

        if (oldFamilyId) {
            try {
                const oldFamily = await firstValueFrom(this.familyService.getFamilyById(oldFamilyId));
                if (oldFamily) oldName = oldFamily.familyName;
            } catch {
                oldName = 'Unknown Family';
            }
        }

        if (newFamilyId) {
            try {
                const newFamily = await firstValueFrom(this.familyService.getFamilyById(newFamilyId));
                if (newFamily) newName = newFamily.familyName;
            } catch {
                newName = 'Unknown Family';
            }
        }

        return `Are you sure you want to change the family assignment from "${oldName}" to "${newName}"?`;
    }

    /**
     * Handle cancel button
     */
    protected onCancel(): void {
        this.cancelled.emit();
    }

    /**
     * Mark all fields as touched to show validation errors
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
     * Check if a field has errors
     */
    protected hasError(fieldName: string): boolean {
        const field = this.form.get(fieldName);
        return !!(field?.invalid && (field?.touched || field?.dirty));
    }

    /**
     * Get error message for a field
     */
    protected getErrorMessage(fieldName: string): string {
        const field = this.form.get(fieldName);
        if (!field?.errors) {
            return '';
        }

        if (field.errors['required']) {
            return 'This field is required';
        }

        if (field.errors['positiveInteger']) {
            return 'Must be a positive integer';
        }

        return 'Invalid value';
    }

    /**
     * Handle create new family click from family select
     */
    protected async onCreateFamily(): Promise<void> {
        const newFamily = await this.modalService.open<Family>(CreateFamilyModalComponent, {
            title: 'Create New Family',
            size: 'lg',
        });

        if (newFamily) {
            // Set the newly created family in the form
            this.form.patchValue({ familyId: newFamily.id });
            this.family.set(newFamily);
            this.notificationService.success(`Family "${newFamily.familyName}" created and selected`);
        }
    }

    /**
     * Load family details
     */
    private loadFamily(id: number): void {
        this.familyService.getFamilyById(id).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (family) => this.family.set(family),
            error: () => this.family.set(null)
        });
    }
}
