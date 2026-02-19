import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  inject,
  type OnInit,
} from '@angular/core';
import { FormBuilder, type FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  type CreateFamilyRequest,
  type Family,
  FamilyService,
} from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import {
  GuardianFormComponent,
  type GuardianFormData,
} from '@shared/components/guardian-form/guardian-form.component';

@Component({
  selector: 'app-create-family-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, GuardianFormComponent],
  templateUrl: './create-family-modal.component.html',
  styleUrls: ['./create-family-modal.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateFamilyModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private familyService = inject(FamilyService);
  private notificationService = inject(NotificationService);

  // Outputs for modal service integration
  readonly closeModal = new EventEmitter<Family>();
  readonly dismissModal = new EventEmitter<void>();

  // Form group
  protected form!: FormGroup;

  // Loading state
  protected isSaving = false;

  // Guardian form state
  protected showGuardianForm = true;
  protected isEditingGuardian = false;
  private guardians: GuardianFormData[] = [];
  private editingGuardianIndex: number | null = null;

  ngOnInit(): void {
    this.initializeForm();
  }

  /**
   * Initialize the family form
   */
  private initializeForm(): void {
    this.form = this.fb.group({
      familyName: ['', [Validators.required, Validators.maxLength(200)]],
      notes: [''],
    });
  }

  /**
   * Handle guardian form submission
   */
  protected onGuardianSubmit(guardianData: GuardianFormData): void {
    if (this.isEditingGuardian && this.editingGuardianIndex !== null) {
      // Update existing guardian
      this.guardians[this.editingGuardianIndex] = guardianData;
      this.isEditingGuardian = false;
      this.editingGuardianIndex = null;
    } else {
      // Add new guardian
      this.guardians.push(guardianData);
    }
    this.showGuardianForm = false;
  }

  /**
   * Handle guardian form cancel
   */
  protected onGuardianCancel(): void {
    this.showGuardianForm = false;
    this.isEditingGuardian = false;
    this.editingGuardianIndex = null;
  }

  /**
   * Show guardian form to add new guardian
   */
  protected onAddGuardian(): void {
    this.showGuardianForm = true;
  }

  /**
   * Edit an existing guardian
   */
  protected onEditGuardian(index: number): void {
    this.isEditingGuardian = true;
    this.editingGuardianIndex = index;
    this.showGuardianForm = true;
  }

  /**
   * Remove a guardian
   */
  protected onRemoveGuardian(index: number): void {
    this.guardians.splice(index, 1);
  }

  /**
   * Handle form submission
   */
  protected onSubmit(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      return;
    }

    if (this.guardians.length === 0) {
      this.notificationService.warning(
        'Please add at least one guardian to create a family',
        'Validation',
      );
      return;
    }

    this.isSaving = true;

    const familyRequest: CreateFamilyRequest = {
      familyName: this.form.value.familyName,
      notes: this.form.value.notes || undefined,
    };

    this.familyService.createFamily(familyRequest).subscribe({
      next: (newFamily) => {
        // Add guardians to the new family
        this.addGuardiansToFamily(newFamily);
      },
      error: (err) => {
        this.isSaving = false;
        this.notificationService.error(err.detail || 'Failed to create family', 'Error');
      },
    });
  }

  /**
   * Add guardians to the newly created family
   */
  private addGuardiansToFamily(family: Family): void {
    let _completedCount = 0;
    let errorCount = 0;
    const totalGuardians = this.guardians.length;

    // Use sequential execution to maintain order (implied preference)
    // or parallel if order doesn't matter. Sequential is safer for race conditions on server.

    const processNext = (index: number) => {
      if (index >= totalGuardians) {
        // All processed
        this.isSaving = false;

        if (errorCount > 0) {
          this.notificationService.warning(
            `Family created, but ${errorCount} guardian(s) failed to save.`,
            'Partial Success',
          );
        } else {
          this.notificationService.success('Family created successfully');
        }

        this.closeModal.emit(family);
        return;
      }

      const guardianData = this.guardians[index];
      const guardianRequest = {
        firstName: guardianData.firstName,
        lastName: guardianData.lastName,
        relationship: guardianData.relationship,
        phone: guardianData.phone,
        email: guardianData.email,
        isPrimaryContact: guardianData.isPrimaryContact,
      };

      this.familyService.addGuardian(family.id, guardianRequest).subscribe({
        next: () => {
          _completedCount++;
          processNext(index + 1);
        },
        error: (err) => {
          console.error('Failed to add guardian', err);
          errorCount++;
          processNext(index + 1);
        },
      });
    };

    processNext(0);
  }

  /**
   * Handle cancel button
   */
  protected onCancel(): void {
    this.dismissModal.emit();
  }

  /**
   * Mark all fields as touched to show validation errors
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      control?.markAsTouched();
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

    if (field.errors.required) {
      return 'This field is required';
    }

    if (field.errors.maxlength) {
      return `Family name cannot exceed ${field.errors.maxlength.requiredLength} characters`;
    }

    return 'Invalid value';
  }

  /**
   * Get guardian display text
   */
  protected getGuardianDisplay(guardian: GuardianFormData): string {
    return `${guardian.firstName} ${guardian.lastName} (${guardian.relationship || 'Guardian'})`;
  }

  /**
   * Check if guardian is primary contact
   */
  protected isPrimaryContact(guardian: GuardianFormData): boolean {
    return guardian.isPrimaryContact;
  }
}
