import { Component, inject, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FamilyService, type Family, type Guardian, type StudentSummary, type MergeFamiliesResult } from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import { ModalService } from '@core/services/modal.service';
import { MergeFamiliesModalComponent } from '@shared/components/merge-families-modal/merge-families-modal.component';
import { ChangeDetectionStrategy } from '@angular/core';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'app-family-detail',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './family-detail.component.html',
    styleUrls: ['./family-detail.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyDetailComponent implements OnInit {
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private familyService = inject(FamilyService);
    private notificationService = inject(NotificationService);
    private modalService = inject(ModalService);
    private destroyRef = inject(DestroyRef);

    // Data
    protected family: Family | null = null;
    protected guardians: Guardian[] = [];
    protected students: StudentSummary[] = [];

    // Loading states
    protected isLoading = true;
    protected isLoadingStudents = false;

    // Error state
    protected error: string | null = null;

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.loadFamily(+id);
        } else {
            this.error = 'Family ID not provided';
            this.isLoading = false;
        }
    }

    /**
     * Load family data
     */
    private loadFamily(id: number): void {
        this.isLoading = true;
        this.isLoadingStudents = true;
        this.error = null;

        forkJoin({
            family: this.familyService.getFamilyById(id),
            students: this.familyService.getStudentsByFamily(id)
        }).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: ({ family, students }) => {
                this.family = family;
                this.students = students;
                
                if (family && family.guardians) {
                    this.guardians = family.guardians;
                } else {
                    // Fallback if guardians not in DTO
                    this.loadGuardians(id);
                }

                this.isLoading = false;
                this.isLoadingStudents = false;
            },
            error: (err) => {
                this.error = err.detail || 'Failed to load family';
                this.isLoading = false;
                this.isLoadingStudents = false;
                this.notificationService.error(this.error || 'Unknown error', 'Error');
            },
        });
    }

    /**
     * Load guardians for this family (Fallback)
     */
    private loadGuardians(familyId: number): void {
        this.familyService.getGuardiansByFamily(familyId).pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (guardians) => {
                this.guardians = guardians;
            },
            error: (err) => {
                console.error('Failed to load guardians:', err);
            },
        });
    }
    // ... rest of the file




    /**
     * Navigate back to families list
     */
    protected goBack(): void {
        this.router.navigate(['/families']);
    }

    /**
     * Navigate to edit family
     */
    protected editFamily(): void {
        if (this.family) {
            this.router.navigate(['/families/edit', this.family.id]);
        }
    }

    /**
     * Navigate to edit family to add a new guardian
     */
    protected onAddGuardian(): void {
        if (this.family) {
            // Navigate to edit page - the family-form component handles guardian management
            this.router.navigate(['/families/edit', this.family.id]);
        }
    }

    /**
     * Get primary guardian name
     */
    protected getPrimaryGuardianName(): string {
        if (!this.guardians || this.guardians.length === 0) {
            return 'No guardians';
        }

        const primaryGuardian = this.guardians.find(g => g.isPrimaryContact);
        if (primaryGuardian) {
            return `${primaryGuardian.firstName} ${primaryGuardian.lastName}`;
        }

        if (this.family?.primaryBillingContactId) {
            const billingGuardian = this.guardians.find(g => g.id === this.family.primaryBillingContactId);
            if (billingGuardian) {
                return `${billingGuardian.firstName} ${billingGuardian.lastName}`;
            }
        }

        return `${this.guardians[0].firstName} ${this.guardians[0].lastName}`;
    }

    /**
     * Navigate to student detail
     */
    protected viewStudent(studentId: number): void {
        this.router.navigate(['/students', studentId]);
    }

    /**
     * Handle deactivation
     */
    protected async onDeactivate(): Promise<void> {
        if (!this.family) return;

        const confirmed = await this.modalService.confirm({
            title: 'Deactivate Family',
            message: `Are you sure you want to deactivate "${this.family.familyName}"?`,
            confirmText: 'Deactivate',
            cancelText: 'Cancel',
            confirmClass: 'btn-error',
            size: 'sm'
        });

        if (confirmed) {
            this.familyService.deactivateFamily(this.family.id, this.family).subscribe({
                next: () => {
                    this.notificationService.success('Family deactivated successfully');
                    this.router.navigate(['/families']);
                },
                error: (err) => {
                    this.notificationService.error(err.detail || 'Failed to deactivate family', 'Error');
                },
            });
        }
    }

    /**
     * Handle reactivation
     */
    protected async onReactivate(): Promise<void> {
        if (!this.family) return;

        const confirmed = await this.modalService.confirm({
            title: 'Reactivate Family',
            message: `Are you sure you want to reactivate "${this.family.familyName}"?`,
            confirmText: 'Reactivate',
            cancelText: 'Cancel',
            confirmClass: 'btn-success',
            size: 'sm'
        });

        if (confirmed) {
            this.familyService.reactivateFamily(this.family.id, this.family).subscribe({
                next: () => {
                    this.notificationService.success('Family reactivated successfully');
                    this.loadFamily(this.family.id); // Reload to get updated data
                },
                error: (err) => {
                    this.notificationService.error(err.detail || 'Failed to reactivate family', 'Error');
                },
            });
        }
    }

    /**
     * Open merge families modal
     */
    protected async onMergeFamilies(): Promise<void> {
        if (!this.family) return;

        const result = await this.modalService.open<MergeFamiliesResult>(
            MergeFamiliesModalComponent,
            {
                title: 'Merge Families',
                size: 'lg',
                inputs: {
                    startingFamilyId: this.family.id,
                },
            }
        );

        if (result) {
            // Merge was successful, navigate back to families list
            this.notificationService.success(`Merge completed: ${result.summary}`);
            this.router.navigate(['/families']);
        }
    }
}
