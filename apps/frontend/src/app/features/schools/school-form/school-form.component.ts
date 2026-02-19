import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  type OnInit,
  signal,
} from '@angular/core';
import { FormBuilder, type FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationService } from '@core/services/notification.service';
import { type School, SchoolService } from '@core/services/school.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-school-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './school-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SchoolFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly schoolService = inject(SchoolService);
  private readonly notificationService = inject(NotificationService);
  private readonly cdr = inject(ChangeDetectorRef);

  protected form!: FormGroup;
  protected isEditMode = signal<boolean>(false);
  protected schoolId = signal<number | null>(null);
  protected isSubmitting = signal<boolean>(false);
  protected isLoading = signal<boolean>(false);
  private currentSchoolStatus = true;

  // Language options
  protected readonly languageOptions = [
    { value: 'Afr', label: 'Afrikaans' },
    { value: 'Eng', label: 'English' },
  ];

  ngOnInit(): void {
    this.form = this.fb.group({
      // Basic Info
      name: ['', [Validators.required, Validators.maxLength(200)]],
      shortName: ['', [Validators.maxLength(50)]],
      truckId: [null],

      // Pricing
      price: [null],
      feeDescription: ['', [Validators.maxLength(255)]],
      formula: [null],

      // Schedule
      visitDay: ['', [Validators.maxLength(50)]],
      visitSequence: ['', [Validators.maxLength(50)]],

      // Contact
      contactPerson: ['', [Validators.maxLength(200)]],
      contactCell: ['', [Validators.maxLength(50)]],
      phone: ['', [Validators.maxLength(50)]],
      telephone: ['', [Validators.maxLength(50)]],
      fax: ['', [Validators.maxLength(50)]],
      email: ['', [Validators.email, Validators.maxLength(255)]],
      circularsEmail: ['', [Validators.email, Validators.maxLength(255)]],

      // Address
      address: ['', [Validators.maxLength(500)]],
      address2: ['', [Validators.maxLength(50)]],

      // Headmaster
      headmaster: ['', [Validators.maxLength(50)]],
      headmasterCell: ['', [Validators.maxLength(50)]],

      // Settings
      language: ['Afr', [Validators.maxLength(50)]],
      printInvoice: [false],
      importFlag: [false],

      // Afterschool 1
      afterschool1Name: ['', [Validators.maxLength(255)]],
      afterschool1Contact: ['', [Validators.maxLength(255)]],

      // Afterschool 2
      afterschool2Name: ['', [Validators.maxLength(255)]],
      afterschool2Contact: ['', [Validators.maxLength(255)]],

      // Notes
      schedulingNotes: [''],
      moneyMessage: [''],
      safeNotes: [''],

      // Links
      webPage: ['', [Validators.maxLength(500)]],
      kcowWebPageLink: ['', [Validators.maxLength(500)]],
    });

    // Check if we're in edit mode
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.schoolId.set(Number(id));
      this.loadSchool(Number(id));
    }
  }

  private loadSchool(id: number): void {
    this.isLoading.set(true);
    this.schoolService
      .getSchoolById(id)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: (school: School) => {
          this.currentSchoolStatus = school.isActive;
          this.form.patchValue({
            // Basic Info
            name: school.name,
            shortName: school.shortName || '',
            truckId: school.truckId?.toString() || '',

            // Pricing
            price: school.price?.toString() || '',
            feeDescription: school.feeDescription || '',
            formula: school.formula?.toString() || '',

            // Schedule
            visitDay: school.visitDay || '',
            visitSequence: school.visitSequence || '',

            // Contact
            contactPerson: school.contactPerson || '',
            contactCell: school.contactCell || '',
            phone: school.phone || '',
            telephone: school.telephone || '',
            fax: school.fax || '',
            email: school.email || '',
            circularsEmail: school.circularsEmail || '',

            // Address
            address: school.address || '',
            address2: school.address2 || '',

            // Headmaster
            headmaster: school.headmaster || '',
            headmasterCell: school.headmasterCell || '',

            // Settings
            language: school.language || 'Afr',
            printInvoice: school.printInvoice,
            importFlag: school.importFlag,

            // Afterschool 1
            afterschool1Name: school.afterschool1Name || '',
            afterschool1Contact: school.afterschool1Contact || '',

            // Afterschool 2
            afterschool2Name: school.afterschool2Name || '',
            afterschool2Contact: school.afterschool2Contact || '',

            // Notes
            schedulingNotes: school.schedulingNotes || '',
            moneyMessage: school.moneyMessage || '',
            safeNotes: school.safeNotes || '',

            // Links
            webPage: school.webPage || '',
            kcowWebPageLink: school.kcowWebPageLink || '',
          });
        },
        error: (error) => {
          this.notificationService.error(error.detail || 'Failed to load school', 'Error');
          this.router.navigate(['/schools']);
        },
      });
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const formValue = this.form.value;

    const request = {
      name: formValue.name,
      shortName: formValue.shortName || undefined,
      truckId:
        formValue.truckId !== null && formValue.truckId !== ''
          ? Number(formValue.truckId)
          : undefined,
      price:
        formValue.price !== null && formValue.price !== '' ? Number(formValue.price) : undefined,
      feeDescription: formValue.feeDescription || undefined,
      formula:
        formValue.formula !== null && formValue.formula !== ''
          ? Number(formValue.formula)
          : undefined,
      visitDay: formValue.visitDay || undefined,
      visitSequence: formValue.visitSequence || undefined,
      contactPerson: formValue.contactPerson || undefined,
      contactCell: formValue.contactCell || undefined,
      phone: formValue.phone || undefined,
      telephone: formValue.telephone || undefined,
      fax: formValue.fax || undefined,
      email: formValue.email || undefined,
      circularsEmail: formValue.circularsEmail || undefined,
      address: formValue.address || undefined,
      address2: formValue.address2 || undefined,
      headmaster: formValue.headmaster || undefined,
      headmasterCell: formValue.headmasterCell || undefined,
      language: formValue.language || 'Afr',
      printInvoice: !!formValue.printInvoice,
      importFlag: !!formValue.importFlag,
      afterschool1Name: formValue.afterschool1Name || undefined,
      afterschool1Contact: formValue.afterschool1Contact || undefined,
      afterschool2Name: formValue.afterschool2Name || undefined,
      afterschool2Contact: formValue.afterschool2Contact || undefined,
      schedulingNotes: formValue.schedulingNotes || undefined,
      moneyMessage: formValue.moneyMessage || undefined,
      safeNotes: formValue.safeNotes || undefined,
      webPage: formValue.webPage || undefined,
      kcowWebPageLink: formValue.kcowWebPageLink || undefined,
    };

    if (this.isEditMode()) {
      const updateRequest = { ...request, isActive: this.currentSchoolStatus };

      this.schoolService.updateSchool(this.schoolId() ?? 0, updateRequest).subscribe({
        next: () => {
          this.notificationService.success('School updated successfully', 'Success');
          this.router.navigate(['/schools']);
        },
        error: (error) => {
          this.notificationService.error(error.detail || 'Failed to update school', 'Error');
          this.isSubmitting.set(false);
        },
      });
    } else {
      this.schoolService.createSchool(request).subscribe({
        next: () => {
          this.notificationService.success('School created successfully', 'Success');
          this.router.navigate(['/schools']);
        },
        error: (error) => {
          this.notificationService.error(error.detail || 'Failed to create school', 'Error');
          this.isSubmitting.set(false);
        },
      });
    }
  }

  protected onCancel(): void {
    this.router.navigate(['/schools']);
  }

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
    if (control.errors.email) {
      return 'Please enter a valid email address';
    }

    return '';
  }
}
