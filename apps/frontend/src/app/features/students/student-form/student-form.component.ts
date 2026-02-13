import { Component, inject, OnInit, DestroyRef, output, input, ChangeDetectionStrategy, ChangeDetectorRef, signal } from '@angular/core';
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
    template: `
<div class="student-form p-4">
  @if (isLoading) {
  <div class="flex justify-center items-center py-12">
    <span class="loading loading-spinner loading-lg text-primary"></span>
  </div>
  } @else {
  <form [formGroup]="form" (ngSubmit)="onSubmit()">

    <!-- 3-Column Summary Header -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">

      <!-- Col 1: Child Information -->
      <div class="card bg-base-100 shadow-md border border-base-200">
        <div class="card-body p-4 gap-4">
          <h3 class="card-title text-sm uppercase tracking-wider text-base-content/60 border-b pb-2">Child Information
          </h3>

          <!-- Photo & Name Row -->
          <div class="flex gap-4 items-start">
            <div class="flex flex-col gap-2 items-center">
              <app-student-avatar [firstName]="form.value.firstName || 'Student'"
                [lastName]="form.value.lastName || 'Name'" [photoUrl]="form.value.photoUrl" size="lg" />
            </div>
            <div class="flex-1 grid gap-2">
              <div class="form-control w-full">
                <input type="text" class="input input-sm input-bordered font-bold text-lg" formControlName="firstName"
                  placeholder="First Name" />
                @if (hasError('firstName')) { <span class="text-error text-xs mt-1">{{ getErrorMessage('firstName')
                  }}</span> }
              </div>
              <div class="form-control w-full">
                <input type="text" class="input input-sm input-bordered font-bold text-lg" formControlName="lastName"
                  placeholder="Last Name" />
                @if (hasError('lastName')) { <span class="text-error text-xs mt-1">{{ getErrorMessage('lastName')
                  }}</span> }
              </div>
            </div>
          </div>

          <!-- Photo URL (Collapsible/Optional view?) or just small input -->
          <div class="form-control">
            <label class="label py-1"><span class="label-text text-xs">Photo URL</span></label>
            <input type="url" class="input input-xs input-bordered" formControlName="photoUrl"
              placeholder="https://..." />
          </div>

          <!-- Demographics Grid -->
          <div class="grid grid-cols-2 gap-2">
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">DOB</span></label>
              <input type="date" class="input input-xs input-bordered" formControlName="dateOfBirth" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Gender</span></label>
              <select class="select select-xs select-bordered" formControlName="gender">
                <option value="">Select</option>
                @for (opt of genderOptions; track opt.value) { <option [value]="opt.value">{{ opt.label }}</option> }
              </select>
            </div>
            <div class="form-control col-span-2">
              <label class="label py-1"><span class="label-text text-xs">Language</span></label>
              <select class="select select-xs select-bordered w-full" formControlName="language">
                <option value="">Select Language</option>
                @for (opt of languageOptions; track opt.value) { <option [value]="opt.value">{{ opt.label }}</option> }
              </select>
            </div>
          </div>
        </div>
      </div>

      <!-- Col 2: Child Financial -->
      <div class="card bg-base-100 shadow-md border border-base-200">
        <div class="card-body p-4 gap-4">
          <h3 class="card-title text-sm uppercase tracking-wider text-base-content/60 border-b pb-2">Child Financial
          </h3>

          <div class="grid grid-cols-2 gap-3">
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs font-semibold">Financial Code</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="financialCode" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs font-semibold">Family Code</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="familyCode" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs font-semibold">Charge</span></label>
              <input type="number" step="0.01" class="input input-sm input-bordered" formControlName="charge"
                placeholder="0.00" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs font-semibold">Deposit</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="deposit" />
            </div>
            <div class="form-control col-span-2">
              <label class="label py-1"><span class="label-text text-xs">Pay Date</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="payDate"
                placeholder="Payment Date Info" />
            </div>
            <div class="form-control col-span-2">
              <label class="label py-1"><span class="label-text text-xs">Sequence</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="sequence" />
            </div>
          </div>
        </div>
      </div>

      <!-- Col 3: Account / Parents -->
      <div class="card bg-base-100 shadow-md border border-base-200">
        <div class="card-body p-4 gap-4">
          <h3 class="card-title text-sm uppercase tracking-wider text-base-content/60 border-b pb-2">Account / Parents
          </h3>

          <div class="grid gap-3">
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Status</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="status"
                placeholder="Active/Inactive" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Reference</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="reference" maxlength="10" />
              @if (hasError('reference')) { <span class="text-error text-xs">{{ getErrorMessage('reference') }}</span> }
            </div>

            <!-- School Selection -->
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">School</span></label>
              <app-school-select formControlName="schoolId" />
              @if (hasError('schoolId')) { <span class="text-error text-xs">{{ getErrorMessage('schoolId') }}</span> }
            </div>

            <!-- Family Selection -->
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Family Link</span></label>
              <app-family-select formControlName="familyId" (createNew)="onCreateFamily()" />
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- Tab Navigation -->
    <div role="tablist" class="tabs tabs-bordered mb-6 flex-wrap">
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'info'" (click)="activeTab = 'info'">Child Info</a>
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'financial'" (click)="activeTab = 'financial'">Child
        Financial</a>
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'class-groups'"
        (click)="activeTab = 'class-groups'">Class Groups</a>
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'attendance'" (click)="activeTab = 'attendance'">Child
        Attendance</a>
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'cg-attendance'"
        (click)="activeTab = 'cg-attendance'">Class Groups Attendance</a>
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'evaluation'" (click)="activeTab = 'evaluation'">Child
        Evaluation</a>
      <a role="tab" class="tab" [class.tab-active]="activeTab === 'cg-evaluation'"
        (click)="activeTab = 'cg-evaluation'">Class Groups Evaluation</a>
    </div>

    <!-- Tab Contents -->
    <div class="min-h-[300px]">

      <!-- Tab: Child Info -->
      @if (activeTab === 'info') {
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 animate-fade-in">

        <!-- Address Section -->
        <div class="card bg-base-100 border border-base-200">
          <div class="card-body p-4">
            <h4 class="font-bold text-sm mb-2">Address Information</h4>
            <div class="grid gap-3">
              <input type="text" class="input input-sm input-bordered w-full" formControlName="address1"
                placeholder="Address Line 1" />
              <input type="text" class="input input-sm input-bordered w-full" formControlName="address2"
                placeholder="Address Line 2" />
              <input type="text" class="input input-sm input-bordered w-32" formControlName="postalCode"
                placeholder="Postal Code" />
            </div>
          </div>
        </div>

        <!-- Notes & Indicators -->
        <div class="card bg-base-100 border border-base-200">
          <div class="card-body p-4">
            <h4 class="font-bold text-sm mb-2">Notes & Indicators</h4>
            <textarea class="textarea textarea-bordered textarea-sm w-full mb-2" formControlName="generalNote" rows="3"
              placeholder="General Notes"></textarea>

            <div class="grid grid-cols-2 gap-2">
              <input type="text" class="input input-sm input-bordered" formControlName="indicator1"
                placeholder="Indicator 1" />
              <input type="text" class="input input-sm input-bordered" formControlName="indicator2"
                placeholder="Indicator 2" />
              <input type="text" class="input input-sm input-bordered" formControlName="extra"
                placeholder="Extra Info" />
              <label class="cursor-pointer label justify-start gap-2 border rounded px-2">
                <input type="checkbox" class="checkbox checkbox-xs" formControlName="printIdCard" />
                <span class="label-text text-xs">Print ID Card</span>
              </label>
            </div>
            <div class="form-control mt-2">
              <input type="text" class="input input-sm input-bordered" formControlName="schoolName"
                placeholder="Legacy School Name" />
            </div>
          </div>
        </div>

        <!-- T-Shirt Orders (Full Width) -->
        <div class="card bg-base-100 border border-base-200 md:col-span-2">
          <div class="card-body p-4">
            <h4 class="font-bold text-sm mb-2">T-Shirt Orders</h4>
            <div class="form-control mb-2">
              <label class="label py-0"><span class="label-text text-xs">T-Shirt Code</span></label>
              <input type="text" class="input input-sm input-bordered w-1/3" formControlName="tshirtCode" />
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- Order 1 -->
              <div class="bg-base-200/50 p-3 rounded">
                <h5 class="text-xs font-bold mb-2">Order 1</h5>
                <div class="grid grid-cols-3 gap-2">
                  <input type="text" class="input input-xs input-bordered" formControlName="tshirtSize1"
                    placeholder="Size" />
                  <input type="text" class="input input-xs input-bordered" formControlName="tshirtColor1"
                    placeholder="Color" />
                  <input type="text" class="input input-xs input-bordered" formControlName="tshirtDesign1"
                    placeholder="Design" />
                </div>
              </div>
              <!-- Order 2 -->
              <div class="bg-base-200/50 p-3 rounded">
                <h5 class="text-xs font-bold mb-2">Order 2</h5>
                <div class="grid grid-cols-3 gap-2">
                  <input type="text" class="input input-xs input-bordered" formControlName="tshirtSize2"
                    placeholder="Size" />
                  <input type="text" class="input input-xs input-bordered" formControlName="tshirtColor2"
                    placeholder="Color" />
                  <input type="text" class="input input-xs input-bordered" formControlName="tshirtDesign2"
                    placeholder="Design" />
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
      }

      <!-- Tab: Class Groups -->
      @if (activeTab === 'class-groups') {
      <div class="card bg-base-100 border border-base-200 animate-fade-in">
        <div class="card-body p-4">
          <h4 class="font-bold text-sm mb-4">Academic & Transportation Details</h4>

          <!-- Assignment Section -->
          <div class="bg-base-200/30 p-4 rounded-lg mb-4">
            <h5 class="text-xs font-bold uppercase tracking-wider text-base-content/60 mb-3">Assignment</h5>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div class="form-control">
                <label class="label py-1"><span class="label-text text-xs">Class Group</span></label>
                <app-class-group-select [schoolId]="form.value.schoolId" formControlName="classGroupId" />
              </div>
              <div class="form-control">
                <label class="label py-1"><span class="label-text text-xs">Seat Number</span></label>
                <input type="number" class="input input-sm input-bordered" formControlName="seat" placeholder="Optional" min="1" step="1" />
                @if (hasError('seat')) { <span class="text-error text-xs">{{ getErrorMessage('seat') }}</span> }
                <span class="label-text-alt text-base-content/50">Optional: Enter a positive integer</span>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Grade</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="grade" placeholder="Grade" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Teacher</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="teacher" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Class Group Code</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="classGroupCode" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Attending KCOW At</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="attendingKcowAt" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Aftercare</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="aftercare" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Terms</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="terms" />
            </div>

            <!-- Transport -->
            <div class="col-span-full divider text-xs text-base-content/50">Transportation</div>

            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Truck</span></label>
              <input type="text" class="input input-sm input-bordered" formControlName="truck" />
            </div>
            <div class="form-control">
              <label class="label py-1"><span class="label-text text-xs">Home Time</span></label>
              <input type="time" class="input input-sm input-bordered" formControlName="homeTime" />
            </div>
          </div>
        </div>
      </div>
      }

      <!-- Tab: Child Financial (Detail) -->
      @if (activeTab === 'financial') {
      <div class="alert alert-info">
        <span>Detailed financial history and transactions will appear here. See summary in header.</span>
      </div>
      }

      <!-- Other Tabs Placeholders -->
      @if (activeTab === 'attendance') {
      <div class="alert"><span>📋 Child Attendance module coming soon.</span></div>
      }
      @if (activeTab === 'cg-attendance') {
      <div class="alert"><span>📊 Class Groups Attendance module coming soon.</span></div>
      }
      @if (activeTab === 'evaluation') {
      <div class="alert"><span>📝 Child Evaluation module coming soon.</span></div>
      }
      @if (activeTab === 'cg-evaluation') {
      <div class="alert"><span>📈 Class Groups Evaluation module coming soon.</span></div>
      }

    </div>

    <!-- Family Grid (Always at bottom) -->
    <div class="mt-8">
      <div class="divider text-sm font-bold uppercase tracking-widest text-base-content/40">Family & Guardians</div>

      @if (family(); as fm) {
      <div class="card bg-base-100 border border-base-200 shadow-sm animate-fade-in">
        <div class="card-body p-4">
          <div class="flex justify-between items-center mb-4">
            <div class="flex items-center gap-2">
              <h3 class="font-bold text-lg">{{ fm.familyName }}</h3>
              <span class="badge badge-sm" [class.badge-success]="fm.isActive" [class.badge-ghost]="!fm.isActive">
                {{ fm.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>
            <a [routerLink]="['/admin/families', fm.id]" class="btn btn-ghost btn-xs">View Full Details</a>
          </div>

          <div class="overflow-x-auto">
            <table class="table table-sm">
              <thead>
                <tr class="bg-base-200/50">
                  <th>Guardian Name</th>
                  <th>Relationship</th>
                  <th>Contact Info</th>
                  <th>Role</th>
                </tr>
              </thead>
              <tbody>
                @for (guardian of fm.guardians; track guardian.id) {
                <tr class="hover">
                  <td class="font-medium">{{ guardian.firstName }} {{ guardian.lastName }}</td>
                  <td>{{ guardian.relationship || 'N/A' }}</td>
                  <td>
                    <div class="flex flex-col gap-0.5 text-xs">
                      @if(guardian.phone) { <span class="flex items-center gap-1 opacity-80">📞 {{ guardian.phone
                        }}</span> }
                      @if(guardian.email) { <span class="flex items-center gap-1 opacity-80">✉️ {{ guardian.email
                        }}</span> }
                    </div>
                  </td>
                  <td>
                    @if (guardian.isPrimaryContact) {
                    <div class="tooltip" data-tip="Primary Billing Contact">
                      <span class="badge badge-primary badge-xs">Billing</span>
                    </div>
                    }
                  </td>
                </tr>
                }
                @if (!fm.guardians?.length) {
                <tr>
                  <td colspan="4" class="text-center py-6 text-base-content/50 italic">No guardians found for this
                    family.</td>
                </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>
      } @else {
      <div
        class="alert bg-base-100 border border-dashed border-base-300 text-sm flex justify-center py-8 text-base-content/60">
        No family linked. Select a family above or create a new one to see details here.
      </div>
      }
    </div>

    <!-- Global Actions -->
    <div
      class="flex justify-end gap-3 mt-8 pt-4 border-t sticky bottom-0 bg-base-100 p-4 z-10 shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.1)]">
      <button type="button" class="btn" (click)="onCancel()" [disabled]="isSaving">Cancel</button>
      <button type="submit" class="btn btn-primary px-8" [disabled]="form.invalid || isSaving">
        @if (isSaving) { <span class="loading loading-spinner"></span> }
        Save Changes
      </button>
      @if (error && error.detail) {
      <div class="text-error text-sm self-center ml-4">{{ error.detail }}</div>
      }
    </div>

  </form>
  }
</div>
    `,
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
    private cdr = inject(ChangeDetectorRef);

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
        { value: 'M', label: 'Male' },
        { value: 'F', label: 'Female' },
        { value: 'O', label: 'Other' },
    ];

    languageOptions = [
        { value: 'Eng', label: 'English' },
        { value: 'Afr', label: 'Afrikaans' },
        { value: 'Zul', label: 'Zulu' },
        { value: 'Xho', label: 'Xhosa' },
        { value: 'Oth', label: 'Other' },
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
                this.cdr.markForCheck();
            },
            error: (err: ProblemDetails) => {
                this.error = err;
                this.isLoading = false;
                this.cdr.markForCheck();
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

        const formValue = { ...this.form.value };
        
        // Clean up empty strings for nullable backend fields
        const cleanupNullable = (val: unknown): unknown => {
            if (val === null || val === undefined) return null;
            if (typeof val === 'string' && (val.trim() === '' || val.trim() === 'null')) return null;
            return val;
        };
        
        // Apply to ALL fields in formValue to be safe
        Object.keys(formValue).forEach(key => {
            formValue[key] = cleanupNullable(formValue[key]);
        });

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
                    this.cdr.markForCheck();
                    this.notificationService.success('Student updated successfully');
                    this.saved.emit(student);
                },
                error: (err: ProblemDetails) => {
                    this.error = err;
                    this.isSaving = false;
                    this.cdr.markForCheck();
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
                    this.cdr.markForCheck();
                    this.notificationService.success('Student created successfully');
                    this.saved.emit(student);
                },
                error: (err: ProblemDetails) => {
                    this.error = err;
                    this.isSaving = false;
                    this.cdr.markForCheck();
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
