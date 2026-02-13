import { ChangeDetectionStrategy, Component, inject, input, output, EventEmitter, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FamilyService, type Family, type Guardian, type StudentSummary, type CreateFamilyRequest, type UpdateFamilyRequest, type CreateGuardianRequest } from '@core/services/family.service';
import { StudentService, type ProblemDetails } from '@core/services/student.service';
import { NotificationService } from '@core/services/notification.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-family-section',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './family-section.component.html',
    styleUrls: ['./family-section.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilySectionComponent implements OnInit {
    private readonly formBuilder = inject(FormBuilder);
    private readonly familyService = inject(FamilyService);
    private readonly studentService = inject(StudentService);
    private readonly router = inject(Router);
    private readonly notificationService = inject(NotificationService);

    // Input signal for student data
    readonly student = input.required<{ id: number; familyId?: number | null; familyName?: string | null }>();

    // Output event for when student family is updated
    readonly studentUpdated = new EventEmitter<void>();

    // Component state
    protected family = signal<Family | null>(null);
    protected guardians = signal<Guardian[]>([]);
    protected siblings = signal<StudentSummary[]>([]);
    protected isLoading = signal<boolean>(false);
    protected error = signal<ProblemDetails | null>(null);
    protected isEditingFamily = signal<boolean>(false);
    protected isAddingGuardian = signal<boolean>(false);
    protected isEditingGuardian = signal<number | null>(null); // guardianId being edited

    // Forms
    protected familyForm!: FormGroup;
    protected guardianForm!: FormGroup;

    ngOnInit(): void {
        this.initializeForms();
        this.loadFamilyData();
    }

    /**
     * Initialize reactive forms
     */
    private initializeForms(): void {
        this.familyForm = this.formBuilder.group({
            familyName: ['', [Validators.required, Validators.maxLength(50)]],
            primaryBillingContactId: [null],
            notes: ['', [Validators.maxLength(500)]],
        });

        this.guardianForm = this.formBuilder.group({
            firstName: ['', [Validators.required, Validators.maxLength(50)]],
            lastName: ['', [Validators.required, Validators.maxLength(50)]],
            relationship: ['', [Validators.maxLength(20)]],
            phone: ['', [Validators.maxLength(20)]],
            email: ['', [Validators.email, Validators.maxLength(100)]],
            isPrimaryContact: [false],
        });
    }

    /**
     * Load family data for the student
     */
    private loadFamilyData(): void {
        const s = this.student();
        if (!s.familyId) {
            this.family.set(null);
            this.guardians.set([]);
            this.siblings.set([]);
            return;
        }

        this.isLoading.set(true);
        this.error.set(null);

        this.familyService.getFamilyById(s.familyId).pipe(
            takeUntilDestroyed(this)
        ).subscribe({
            next: (family) => {
                this.family.set(family);
                this.guardians.set(family.guardians || []);
                this.loadSiblings(family.id);
                this.populateFamilyForm(family);
            },
            error: (err: ProblemDetails) => {
                this.error.set(err);
                this.isLoading.set(false);
            },
            complete: () => {
                this.isLoading.set(false);
            }
        });
    }

    /**
     * Load siblings (other students in the same family)
     */
    private loadSiblings(familyId: number): void {
        this.familyService.getStudentsByFamily(familyId).pipe(
            takeUntilDestroyed(this)
        ).subscribe({
            next: (students) => {
                // Filter out the current student
                this.siblings.set(students.filter(s => s.id !== this.student().id));
            },
            error: (err: ProblemDetails) => {
                console.error('Failed to load siblings:', err);
                this.siblings.set([]);
            }
        });
    }

    /**
     * Populate family form with existing data
     */
    private populateFamilyForm(family: Family): void {
        this.familyForm.patchValue({
            familyName: family.familyName,
            primaryBillingContactId: family.primaryBillingContactId,
            notes: family.notes || '',
        });
    }

    /**
     * Enable edit mode for family
     */
    protected editFamily(): void {
        this.isEditingFamily.set(true);
        this.isAddingGuardian.set(false);
        this.isEditingGuardian.set(null);
        this.familyForm.enable();
    }

    /**
     * Cancel family edit
     */
    protected cancelFamilyEdit(): void {
        this.isEditingFamily.set(false);
        this.error.set(null);
        const f = this.family();
        if (f) {
            this.populateFamilyForm(f);
        }
        this.familyForm.disable();
    }

    /**
     * Save family changes
     */
    protected saveFamily(): void {
        if (this.familyForm.invalid) {
            this.markFormGroupTouched(this.familyForm);
            return;
        }

        const f = this.family();
        if (!f) return;

        this.isLoading.set(true);
        this.error.set(null);

        const formValue = this.familyForm.value;
        const updateRequest: UpdateFamilyRequest = {
            familyName: formValue.familyName,
            primaryBillingContactId: formValue.primaryBillingContactId || undefined,
            notes: formValue.notes || undefined,
            isActive: f.isActive,
        };

        this.familyService.updateFamily(f.id, updateRequest).pipe(
            takeUntilDestroyed(this)
        ).subscribe({
            next: (updatedFamily) => {
                this.family.set(updatedFamily);
                this.isEditingFamily.set(false);
                this.familyForm.disable();
                this.studentUpdated.emit();
                this.showSuccessMessage('Family updated successfully');
            },
            error: (err: ProblemDetails) => {
                this.error.set(err);
                this.showErrorMessage(err.detail || 'Failed to update family');
            },
            complete: () => {
                this.isLoading.set(false);
            }
        });
    }

    /**
     * Add new family
     */
    protected addNewFamily(): void {
        this.familyForm.reset({
            familyName: '',
            primaryBillingContactId: null,
            notes: '',
        });
        this.isEditingFamily.set(true);
        this.familyForm.enable();
    }

    /**
     * Show add guardian form
     */
    protected showAddGuardian(): void {
        this.guardianForm.reset({
            firstName: '',
            lastName: '',
            relationship: '',
            phone: '',
            email: '',
            isPrimaryContact: false,
        });
        this.isAddingGuardian.set(true);
        this.isEditingGuardian.set(null);
        this.isEditingFamily.set(false);
    }

    /**
     * Edit existing guardian
     */
    protected editGuardian(guardian: Guardian): void {
        this.guardianForm.patchValue({
            firstName: guardian.firstName,
            lastName: guardian.lastName,
            relationship: guardian.relationship || '',
            phone: guardian.phone || '',
            email: guardian.email || '',
            isPrimaryContact: guardian.isPrimaryContact,
        });
        this.isEditingGuardian.set(guardian.id);
        this.isAddingGuardian.set(false);
        this.isEditingFamily.set(false);
    }

    /**
     * Cancel guardian edit
     */
    protected cancelGuardianEdit(): void {
        this.isAddingGuardian.set(false);
        this.isEditingGuardian.set(null);
        this.guardianForm.reset();
    }

    /**
     * Save guardian (create or update)
     */
    protected saveGuardian(): void {
        if (this.guardianForm.invalid) {
            this.markFormGroupTouched(this.guardianForm);
            return;
        }

        const f = this.family();
        if (!f) {
            this.showErrorMessage('Please create a family first');
            return;
        }

        this.isLoading.set(true);
        this.error.set(null);

        const formValue = this.guardianForm.value;

        if (this.isEditingGuardian()) {
            // Update existing guardian
            const guardianId = this.isEditingGuardian()!;
            this.familyService.updateGuardian(f.id, guardianId, formValue).pipe(
                takeUntilDestroyed(this)
            ).subscribe({
                next: (updatedGuardian) => {
                    this.guardians.update(guardians =>
                        guardians.map(g => g.id === guardianId ? updatedGuardian : g)
                    );
                    this.cancelGuardianEdit();
                    this.showSuccessMessage('Guardian updated successfully');
                },
                error: (err: ProblemDetails) => {
                    this.error.set(err);
                    this.showErrorMessage(err.detail || 'Failed to update guardian');
                },
                complete: () => {
                    this.isLoading.set(false);
                }
            });
        } else {
            // Create new guardian
            this.familyService.addGuardian(f.id, formValue).pipe(
                takeUntilDestroyed(this)
            ).subscribe({
                next: (newGuardian) => {
                    this.guardians.update(guardians => [...guardians, newGuardian]);
                    this.cancelGuardianEdit();
                    this.showSuccessMessage('Guardian added successfully');
                },
                error: (err: ProblemDetails) => {
                    this.error.set(err);
                    this.showErrorMessage(err.detail || 'Failed to add guardian');
                },
                complete: () => {
                    this.isLoading.set(false);
                }
            });
        }
    }

    /**
     * Delete guardian
     */
    protected deleteGuardian(guardianId: number): void {
        if (!confirm('Are you sure you want to remove this guardian?')) return;

        const f = this.family();
        if (!f) return;

        this.isLoading.set(true);
        this.error.set(null);

        this.familyService.deleteGuardian(f.id, guardianId).pipe(
            takeUntilDestroyed(this)
        ).subscribe({
            next: () => {
                this.guardians.update(guardians => guardians.filter(g => g.id !== guardianId));
                this.showSuccessMessage('Guardian removed successfully');
            },
            error: (err: ProblemDetails) => {
                this.error.set(err);
                this.showErrorMessage(err.detail || 'Failed to remove guardian');
            },
            complete: () => {
                this.isLoading.set(false);
            }
        });
    }

    /**
     * Navigate to sibling profile
     */
    protected viewSibling(sibling: StudentSummary): void {
        this.router.navigate(['/students', sibling.id]);
    }

    /**
     * Check if guardian has error
     */
    protected guardianHasError(fieldName: string, errorType?: string): boolean {
        const field = this.guardianForm.get(fieldName);
        if (!field) return false;

        if (errorType) {
            return field.hasError(errorType) && field.touched;
        }
        return field.invalid && field.touched;
    }

    /**
     * Get guardian error message
     */
    protected getGuardianErrorMessage(fieldName: string): string {
        const field = this.guardianForm.get(fieldName);
        if (!field || !field.errors) return '';

        if (field.errors['required']) {
            return 'This field is required';
        }
        if (field.errors['email']) {
            return 'Invalid email format';
        }
        if (field.errors['maxlength']) {
            return `Maximum length is ${field.errors['maxlength'].requiredLength} characters`;
        }
        return 'Invalid value';
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
}
