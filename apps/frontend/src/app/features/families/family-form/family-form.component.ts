import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, type OnInit, signal } from '@angular/core';
import { FormBuilder, type FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  type CreateGuardianRequest,
  type Family,
  FamilyService,
  type Guardian,
} from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import {
  GuardianFormComponent,
  type GuardianFormData,
} from '@shared/components/guardian-form/guardian-form.component';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-family-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, GuardianFormComponent],
  templateUrl: './family-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly familyService = inject(FamilyService);
  private readonly notificationService = inject(NotificationService);

  protected form!: FormGroup;
  protected isEditMode = signal<boolean>(false);
  protected familyId = signal<number | null>(null);
  protected isSubmitting = signal<boolean>(false);
  protected isLoading = signal<boolean>(false);
  protected currentFamilyStatus = true;

  // Guardian management state
  protected guardians = signal<Guardian[]>([]);
  protected showGuardianForm = signal<boolean>(false);
  protected isEditingGuardian = signal<boolean>(false);
  protected editingGuardianIndex = signal<number>(-1);
  protected isAddingFirstGuardian = signal<boolean>(false);

  ngOnInit(): void {
    this.form = this.fb.group({
      familyName: ['', [Validators.required, Validators.maxLength(200)]],
      notes: ['', [Validators.maxLength(1000)]],
    });

    // Check if we're in edit mode
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.familyId.set(Number(id));
      this.loadFamily(Number(id));
    } else {
      // New family - show initial guardian form
      this.isAddingFirstGuardian.set(true);
      this.showGuardianForm.set(true);
    }
  }

  private loadFamily(id: number): void {
    this.isLoading.set(true);
    this.familyService
      .getFamilyById(id)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        }),
      )
      .subscribe({
        next: (family: Family) => {
          this.currentFamilyStatus = family.isActive;
          this.guardians.set(family.guardians || []);
          this.form.patchValue({
            familyName: family.familyName,
            notes: family.notes || '',
          });
        },
        error: (error) => {
          this.notificationService.error(error.detail || 'Failed to load family', 'Error');
          this.router.navigate(['/families']);
        },
      });
  }

  /**
   * Handle guardian form submission
   */
  protected onGuardianSubmit(data: GuardianFormData): void {
    // If this guardian is set as primary, unset all others first
    if (data.isPrimaryContact) {
      this.guardians.update((guardians) =>
        guardians.map((g) => ({ ...g, isPrimaryContact: false })),
      );
    }

    if (this.isEditingGuardian()) {
      // Update existing guardian in list
      const index = this.editingGuardianIndex();
      const existingGuardian = this.guardians()[index];
      const updatedGuardian: Guardian = {
        ...existingGuardian,
        firstName: data.firstName,
        lastName: data.lastName,
        relationship: data.relationship,
        phone: data.phone,
        email: data.email,
        isPrimaryContact: data.isPrimaryContact,
      };
      this.guardians.update((guardians) => {
        const updated = [...guardians];
        updated[index] = updatedGuardian;
        return updated;
      });
    } else {
      // Add new guardian to list
      const newGuardian: Guardian = {
        id: 0, // Temporary ID, will be replaced by server
        familyId: 0, // Will be set when family is created
        firstName: data.firstName,
        lastName: data.lastName,
        relationship: data.relationship,
        phone: data.phone,
        email: data.email,
        isPrimaryContact: data.isPrimaryContact,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
      this.guardians.update((guardians) => [...guardians, newGuardian]);
    }

    // Reset guardian form state
    this.cancelGuardianForm();
  }

  /**
   * Add a new guardian
   */
  protected onAddGuardian(): void {
    this.isEditingGuardian.set(false);
    this.editingGuardianIndex.set(-1);
    this.showGuardianForm.set(true);
    this.isAddingFirstGuardian.set(false);
  }

  /**
   * Edit an existing guardian
   */
  protected onEditGuardian(index: number): void {
    this.isEditingGuardian.set(true);
    this.editingGuardianIndex.set(index);
    this.showGuardianForm.set(true);
  }

  /**
   * Remove a guardian from the list
   */
  protected onRemoveGuardian(index: number): void {
    this.guardians.update((guardians) => guardians.filter((_, i) => i !== index));
  }

  /**
   * Cancel guardian form
   */
  protected cancelGuardianForm(): void {
    this.showGuardianForm.set(false);
    this.isEditingGuardian.set(false);
    this.editingGuardianIndex.set(-1);
    this.isAddingFirstGuardian.set(false);
  }

  /**
   * Check if guardian form is visible
   */
  protected isGuardianFormVisible(): boolean {
    return this.showGuardianForm();
  }

  /**
   * Get the guardian being edited (if any)
   */
  protected getEditingGuardian(): Guardian | null {
    if (this.isEditingGuardian() && this.editingGuardianIndex() >= 0) {
      return this.guardians()[this.editingGuardianIndex()] || null;
    }
    return null;
  }

  /**
   * Submit the family form
   */
  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const formValue = this.form.value;
    const guardiansList = this.guardians();

    const request = {
      familyName: formValue.familyName,
      notes: formValue.notes || undefined,
    };

    if (this.isEditMode()) {
      // Update existing family
      const updateRequest = {
        ...request,
        primaryBillingContactId: this.getPrimaryBillingContactId(),
        isActive: this.currentFamilyStatus,
      };

      this.familyService.updateFamily(this.familyId() ?? 0, updateRequest).subscribe({
        next: () => {
          this.notificationService.success('Family updated successfully', 'Success');
          this.router.navigate(['/families']);
        },
        error: (error) => {
          this.notificationService.error(error.detail || 'Failed to update family', 'Error');
          this.isSubmitting.set(false);
        },
      });
    } else {
      // Create new family with guardian
      if (guardiansList.length === 0) {
        this.notificationService.warning(
          'Please add at least one guardian to create a family',
          'Validation',
        );
        this.isSubmitting.set(false);
        return;
      }

      this.familyService.createFamily(request).subscribe({
        next: (family: Family) => {
          // Add the first guardian to the newly created family
          const firstGuardian = guardiansList[0];
          const guardianRequest: CreateGuardianRequest = {
            firstName: firstGuardian.firstName,
            lastName: firstGuardian.lastName,
            relationship: firstGuardian.relationship,
            phone: firstGuardian.phone,
            email: firstGuardian.email,
            isPrimaryContact: firstGuardian.isPrimaryContact,
          };

          this.familyService.addGuardian(family.id, guardianRequest).subscribe({
            next: () => {
              this.notificationService.success(
                'Family and guardian created successfully',
                'Success',
              );
              this.router.navigate(['/families']);
            },
            error: (error) => {
              this.notificationService.error(
                `Family created but failed to add guardian: ${error.detail || 'Unknown error'}`,
                'Partial Success',
              );
              this.router.navigate(['/families']);
            },
          });
        },
        error: (error) => {
          this.notificationService.error(error.detail || 'Failed to create family', 'Error');
          this.isSubmitting.set(false);
        },
      });
    }
  }

  /**
   * Get the primary billing contact ID from guardians
   */
  private getPrimaryBillingContactId(): number | undefined {
    const primaryGuardian = this.guardians().find((g) => g.isPrimaryContact);
    return primaryGuardian?.id;
  }

  /**
   * Get error message for a form control
   */
  protected getErrorMessage(controlName: string): string {
    const control = this.form.get(controlName);
    if (!control || !control.touched || !control.errors) {
      return '';
    }

    const label =
      controlName.charAt(0).toUpperCase() + controlName.slice(1).replace(/([A-Z])/g, ' $1');

    if (control.errors.required) {
      return `${label} is required`;
    }
    if (control.errors.maxlength) {
      return `${label} cannot exceed ${control.errors.maxlength.requiredLength} characters`;
    }

    return '';
  }

  /**
   * Cancel and navigate back
   */
  protected onCancel(): void {
    this.router.navigate(['/families']);
  }
}
