import { Component, inject, model, forwardRef, OnInit, effect, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { FamilyService, type Family } from '@core/services/family.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-family-select',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './family-select.component.html',
    styleUrls: ['./family-select.component.scss'],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => FamilySelectComponent),
            multi: true,
        },
    ],
})
export class FamilySelectComponent implements ControlValueAccessor, OnInit {
    protected familyService = inject(FamilyService);
    private destroyRef = inject(DestroyRef);

    // Two-way binding for familyId
    readonly familyId = model<number | null>(null);

    // Search query
    protected searchQuery = '';

    // Filtered families based on search
    protected filteredFamilies: Family[] = [];

    // Loading state
    protected isLoading = false;

    // Disabled state
    protected isDisabled = false;

    // Display selected family name
    protected displayFamilyName = '';

    // Dropdown open state
    protected isDropdownOpen = false;

    // Event emitted when "Create New Family" is clicked
    readonly createNew = output<void>();

    // Subject for debounced search
    private searchSubject = new Subject<string>();

    // ControlValueAccessor callbacks
    private onChange: (value: number | null) => void = () => {};
    private onTouched: () => void = () => {};

    constructor() {
        // We no longer rely on a full local cache of families
    }

    ngOnInit(): void {
        this.loadFamilies();

        // Setup debounced search (300ms as per AC)
        this.searchSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntilDestroyed(this.destroyRef)
        ).subscribe((query) => {
            this.searchQuery = query;
            this.loadFamilies(query);
        });
    }

    /**
     * Load families from service (server-side search)
     */
    private loadFamilies(search?: string): void {
        this.isLoading = true;
        this.familyService.getActiveFamilies(search).subscribe({
            next: (families) => {
                this.filteredFamilies = families;
                this.isLoading = false;
            },
            error: () => {
                this.isLoading = false;
                this.filteredFamilies = [];
            },
        });
    }

    /**
     * Get primary contact name for display
     */
    protected getPrimaryContactName(family: Family): string {
        if (!family.guardians || family.guardians.length === 0) {
            return 'No contact';
        }

        // Find primary guardian by isPrimaryContact flag
        const primaryGuardian = family.guardians.find(g => g.isPrimaryContact);
        if (primaryGuardian) {
            return `${primaryGuardian.firstName} ${primaryGuardian.lastName}`;
        }

        // If no primary flagged, use first guardian or primaryBillingContactId
        if (family.primaryBillingContactId) {
            const billingGuardian = family.guardians.find(g => g.id === family.primaryBillingContactId);
            if (billingGuardian) {
                return `${billingGuardian.firstName} ${billingGuardian.lastName}`;
            }
        }

        // Fallback to first guardian
        const firstGuardian = family.guardians[0];
        return `${firstGuardian.firstName} ${firstGuardian.lastName}`;
    }

    /**
     * Get student count display text
     */
    protected getStudentCountText(family: Family): string {
        const count = family.studentCount || 0;
        if (count === 0) {
            return 'No students';
        } else if (count === 1) {
            return '1 student';
        }
        return `${count} students`;
    }

    /**
     * Handle search input with debounce (300ms)
     */
    protected onSearchInput(event: Event): void {
        const input = event.target as HTMLInputElement;
        const value = input.value;
        
        // If user explicitly clears the input while an item is selected, clear the selection
        if (!value && this.familyId()) {
             this.clearSelection();
        }
        
        this.searchSubject.next(value);
    }
    
    /**
     * Handle focus - open dropdown
     */
    protected onInputFocus(): void {
        this.isDropdownOpen = true;
        // If we have no families loaded or if the search query currently matches the display name,
        // clear the query so the user sees the full list/search state.
        if (this.filteredFamilies.length === 0 || (this.displayFamilyName && this.searchQuery === this.displayFamilyName)) {
            this.searchQuery = '';
            this.loadFamilies('');
        }
    }

    /**
     * Handle input blur - close dropdown after a delay to allow clicks
     */
    protected onInputBlur(): void {
        // Delay closing to allow click events on dropdown items
        setTimeout(() => {
            this.isDropdownOpen = false;
        }, 500);
    }

    /**
     * Handle dropdown content click - prevent blur from closing
     */
    protected onDropdownClick(): void {
        // Keep dropdown open when clicking inside
    }



    /**
     * Select a family
     */
    protected selectFamily(familyId: number): void {
        this.familyId.set(familyId);
        const selected = this.filteredFamilies.find((f) => f.id === familyId);
        if (selected) {
            this.displayFamilyName = selected.familyName;
            this.searchQuery = selected.familyName; // Update search query to match selection
        }
        this.isDropdownOpen = false;
        this.onChange(familyId);
        this.onTouched();
    }

    /**
     * Clear selection (select "No Family")
     */
    protected clearSelection(): void {
        this.familyId.set(null);
        this.displayFamilyName = '';
        this.searchQuery = '';
        this.onChange(null);
        this.onTouched();
    }

    /**
     * Handle Create New Family click
     */
    protected onCreateNew(): void {
        this.createNew.emit();
    }

    // ControlValueAccessor implementation
    writeValue(value: number | null): void {
        this.familyId.set(value);
        if (value) {
            // Check if we have it in current filtered list
            const existing = this.filteredFamilies.find(f => f.id === value);
            if (existing) {
                this.displayFamilyName = existing.familyName;
                this.searchQuery = existing.familyName;
            } else {
                // Fetch specific family to get name
                this.familyService.getFamilyById(value).subscribe({
                    next: (family) => {
                        this.displayFamilyName = family.familyName;
                        this.searchQuery = family.familyName;
                        // Optionally add to filtered list so it appears selected
                        if (!this.filteredFamilies.find(f => f.id === family.id)) {
                            this.filteredFamilies = [family, ...this.filteredFamilies];
                        }
                    },
                    error: () => {
                        this.displayFamilyName = 'Unknown Family';
                    }
                });
            }
        } else {
            this.displayFamilyName = '';
            this.searchQuery = '';
        }
    }

    registerOnChange(fn: (value: number | null) => void): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void {
        this.onTouched = fn;
    }

    setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }
}
