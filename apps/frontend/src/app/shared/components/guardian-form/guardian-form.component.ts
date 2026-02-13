import { ChangeDetectionStrategy, Component, inject, input, output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    FormBuilder,
    FormGroup,
    ReactiveFormsModule,
    Validators,
    FormControl
} from '@angular/forms';
import { Guardian, CreateGuardianRequest, UpdateGuardianRequest } from '@core/services/family.service';

export interface GuardianFormData {
    firstName: string;
    lastName: string;
    relationship?: string;
    phone?: string;
    email?: string;
    isPrimaryContact: boolean;
}

@Component({
    selector: 'app-guardian-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './guardian-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GuardianFormComponent implements OnInit {
    private fb = inject(FormBuilder);

    // Inputs
    readonly guardian = input<Guardian | null | undefined>(null);
    readonly isPrimaryOnly = input<boolean>(false);

    // Outputs
    readonly guardianSubmit = output<GuardianFormData>();
    readonly cancel = output<void>();

    // Form (public for testing)
    guardianForm: FormGroup = this.fb.group({
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        relationship: [''],
        phone: [''],
        email: ['', [Validators.email]],
        isPrimaryContact: [false],
    });

    ngOnInit(): void {
        // If editing existing guardian, populate form
        const existingGuardian = this.guardian();
        if (existingGuardian) {
            this.guardianForm.patchValue({
                firstName: existingGuardian.firstName,
                lastName: existingGuardian.lastName,
                relationship: existingGuardian.relationship || '',
                phone: existingGuardian.phone || '',
                email: existingGuardian.email || '',
                isPrimaryContact: existingGuardian.isPrimaryContact,
            });
        }
    }

    /**
     * Check if a form field has errors
     */
    hasError(controlName: string, errorType?: string): boolean {
        const control = this.guardianForm.get(controlName);
        if (!control) return false;

        if (errorType) {
            return control.hasError(errorType) && (control.dirty || control.touched);
        }
        return control.invalid && (control.dirty || control.touched);
    }

    /**
     * Get error message for a control
     */
    getErrorMessage(controlName: string): string {
        const control = this.guardianForm.get(controlName);
        if (!control) return '';

        if (control.hasError('required')) {
            return 'This field is required';
        }
        if (control.hasError('email')) {
            return 'Please enter a valid email address';
        }
        return '';
    }

    /**
     * Handle form submission (called from parent component's button)
     */
    onSubmit(): void {
        if (this.guardianForm.invalid) {
            // Mark all controls as touched to show validation errors
            Object.keys(this.guardianForm.controls).forEach(key => {
                const control = this.guardianForm.get(key);
                control?.markAsTouched();
            });
            return;
        }

        this.guardianSubmit.emit(this.guardianForm.value as GuardianFormData);
    }

    /**
     * Handle cancel
     */
    onCancel(): void {
        this.cancel.emit();
    }

    /**
     * Get the form control for a field name
     */
    getControl(fieldName: string): FormControl {
        return this.guardianForm.get(fieldName) as FormControl;
    }
}
