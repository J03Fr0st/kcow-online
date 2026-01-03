import { Component, inject, output, input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { FamilyService, type Family, type FamilyMergePreview, type MergeFamiliesResult } from '@core/services/family.service';
import { NotificationService } from '@core/services/notification.service';
import { EventEmitter } from '@angular/core';
import { debounceTime, distinctUntilChanged } from 'rxjs';

type MergeStep = 'select' | 'choose-primary' | 'resolve-conflict' | 'preview' | 'confirm';

interface MergeState {
    primaryFamily: Family | null;
    secondaryFamily: Family | null;
    preview: FamilyMergePreview | null;
    keepPrimaryBillingContactId: number | null;
}

@Component({
    selector: 'app-merge-families-modal',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './merge-families-modal.component.html',
    styleUrls: ['./merge-families-modal.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MergeFamiliesModalComponent implements OnInit {
    private fb = inject(FormBuilder);
    private familyService = inject(FamilyService);
    private notificationService = inject(NotificationService);

    // Input: Starting family ID (optional)
    readonly startingFamilyId = input<number | null>(null);

    // Outputs for modal service integration
    readonly closeModal = new EventEmitter<MergeFamiliesResult>();
    readonly dismissModal = new EventEmitter<void>();

    // Form group
    protected searchForm!: FormGroup;
    protected primaryForm!: FormGroup;

    // Merge state
    protected currentStep: MergeStep = 'select';
    protected mergeState: MergeState = {
        primaryFamily: null,
        secondaryFamily: null,
        preview: null,
        keepPrimaryBillingContactId: null,
    };

    // Available families
    protected availableFamilies: Family[] = [];

    // Loading states
    protected isLoadingFamilies = false;
    protected isLoadingPreview = false;
    protected isExecutingMerge = false;

    // Search state
    protected filteredFamilies: Family[] = [];
    protected selectedFamilyForMerge: Family | null = null;

    ngOnInit(): void {
        this.initializeForms();
        this.loadActiveFamilies();
    }

    /**
     * Initialize forms
     */
    private initializeForms(): void {
        this.searchForm = this.fb.group({
            searchQuery: [''],
        });

        this.primaryForm = this.fb.group({
            keepPrimaryBillingContactId: [null as number | null],
        });

        // Watch search query changes
        this.searchForm.get('searchQuery')?.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged()
        ).subscribe((query) => {
            this.filterFamilies(query || '');
        });
    }

    /**
     * Load active families for selection
     */
    private loadActiveFamilies(): void {
        this.isLoadingFamilies = true;
        this.familyService.getActiveFamilies().subscribe({
            next: (families) => {
                this.availableFamilies = families.filter(f =>
                    this.startingFamilyId() ? f.id !== this.startingFamilyId() : true
                );
                this.filteredFamilies = [...this.availableFamilies];
                this.isLoadingFamilies = false;
            },
            error: () => {
                this.isLoadingFamilies = false;
            },
        });
    }

    /**
     * Filter families by search query
     */
    protected filterFamilies(query: string): void {
        const lowerQuery = query.toLowerCase();
        this.filteredFamilies = this.availableFamilies.filter(f =>
            f.familyName.toLowerCase().includes(lowerQuery)
        );
    }

    /**
     * Step 1: Select family to merge with
     */
    protected onSelectFamilyToMerge(family: Family): void {
        this.selectedFamilyForMerge = family;
    }

    /**
     * Proceed to choose primary family step
     */
    protected onProceedToChoosePrimary(): void {
        if (!this.selectedFamilyForMerge) return;

        this.mergeState.secondaryFamily = this.selectedFamilyForMerge;
        this.currentStep = 'choose-primary';
    }

    /**
     * Step 2: Choose which family to keep as primary
     */
    protected onChooseAsPrimary(family: Family): void {
        this.mergeState.primaryFamily = family;
        this.loadPreview();
    }

    /**
     * Load merge preview and determine next step
     */
    protected loadPreview(): void {
        if (!this.mergeState.primaryFamily || !this.mergeState.secondaryFamily) return;

        this.isLoadingPreview = true;

        const request = {
            primaryFamilyId: this.mergeState.primaryFamily.id,
            secondaryFamilyId: this.mergeState.secondaryFamily.id,
        };

        this.familyService.previewMerge(request).subscribe({
            next: (preview) => {
                this.mergeState.preview = preview;
                this.isLoadingPreview = false;

                // Determine next step based on conflict
                if (preview.hasPrimaryBillingContactConflict) {
                    this.currentStep = 'resolve-conflict';
                } else {
                    this.currentStep = 'preview';
                }
            },
            error: (err) => {
                this.isLoadingPreview = false;
                this.notificationService.error(
                    err.detail || 'Failed to load merge preview',
                    'Error'
                );
            },
        });
    }

    /**
     * Step 3: Resolve billing contact conflict
     */
    protected onResolveConflict(): void {
        this.mergeState.keepPrimaryBillingContactId = this.primaryForm.value.keepPrimaryBillingContactId || null;
        this.currentStep = 'preview';
    }

    /**
     * Step 4: Show preview
     */
    protected get preview(): FamilyMergePreview | null {
        return this.mergeState.preview;
    }

    /**
     * Step 5: Confirm and execute merge
     */
    protected onConfirmMerge(): void {
        if (!this.mergeState.primaryFamily || !this.mergeState.secondaryFamily) return;

        this.isExecutingMerge = true;

        const request = {
            primaryFamilyId: this.mergeState.primaryFamily.id,
            secondaryFamilyId: this.mergeState.secondaryFamily.id,
            keepPrimaryBillingContactId: this.mergeState.keepPrimaryBillingContactId || undefined,
        };

        this.familyService.mergeFamilies(request).subscribe({
            next: (result) => {
                this.isExecutingMerge = false;
                this.notificationService.success(result.summary, 'Merge Complete');
                this.closeModal.emit(result);
            },
            error: (err) => {
                this.isExecutingMerge = false;
                this.notificationService.error(
                    err.detail || 'Failed to merge families',
                    'Error'
                );
            },
        });
    }

    /**
     * Cancel the merge process
     */
    protected onCancel(): void {
        this.dismissModal.emit();
    }

    /**
     * Go back to previous step
     */
    protected onBack(): void {
        switch (this.currentStep) {
            case 'choose-primary':
                this.currentStep = 'select';
                break;
            case 'resolve-conflict':
                this.currentStep = 'choose-primary';
                break;
            case 'preview':
                this.currentStep = 'resolve-conflict';
                break;
        }
    }

    /**
     * Reset the merge process
     */
    protected onReset(): void {
        this.mergeState = {
            primaryFamily: null,
            secondaryFamily: null,
            preview: null,
            keepPrimaryBillingContactId: null,
        };
        this.selectedFamilyForMerge = null;
        this.primaryForm.reset();
        this.currentStep = 'select';
    }

    /**
     * Get display name for a family with billing contact info
     */
    protected getFamilyDisplayName(family: Family): string {
        return family.familyName;
    }

    /**
     * Check if we're starting from a specific family
     */
    protected get hasStartingFamily(): boolean {
        return this.startingFamilyId() !== null;
    }

    /**
     * Check if step is active
     */
    protected isStepActive(step: MergeStep): boolean {
        return this.currentStep === step;
    }

    /**
     * Check if step is completed
     */
    protected isStepCompleted(step: MergeStep): boolean {
        const steps: MergeStep[] = ['select', 'choose-primary', 'resolve-conflict', 'preview'];
        const currentIndex = steps.indexOf(this.currentStep);
        const stepIndex = steps.indexOf(step);
        return stepIndex < currentIndex;
    }
}
