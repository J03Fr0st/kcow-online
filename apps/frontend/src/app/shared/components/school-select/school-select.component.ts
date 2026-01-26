import { Component, inject, model, forwardRef, OnInit, effect, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { SchoolService, type School } from '@core/services/school.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-school-select',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './school-select.component.html',
    styleUrls: ['./school-select.component.scss'],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => SchoolSelectComponent),
            multi: true,
        },
    ],
})
export class SchoolSelectComponent implements ControlValueAccessor, OnInit {
    protected schoolService = inject(SchoolService);
    private destroyRef = inject(DestroyRef);

    // Two-way binding for schoolId
    readonly schoolId = model<number | null>(null);

    // Search query
    protected searchQuery = '';

    // Filtered schools based on search
    protected filteredSchools: School[] = [];

    // Loading state
    protected isLoading = false;

    // Disabled state
    protected isDisabled = false;

    // Display selected school name
    protected displaySchoolName = '';

    // Dropdown open state
    protected isDropdownOpen = false;

    // Event emitted when "Create New School" is clicked
    readonly createNew = output<void>();

    // All schools cache
    private allSchools: School[] = [];

    // Subject for debounced search
    private searchSubject = new Subject<string>();

    // ControlValueAccessor callbacks
    private onChange: (value: number | null) => void = () => {};
    private onTouched: () => void = () => {};

    constructor() {
        // Use effect to reactively watch the schools signal
        effect(() => {
            this.allSchools = this.schoolService.schools();
            this.filterSchools();
            
            // If we have a schoolId but no display name, update it
            const currentId = this.schoolId();
            if (currentId && !this.displaySchoolName && this.allSchools.length > 0) {
                const selected = this.allSchools.find((s) => s.id === currentId);
                if (selected) {
                    this.displaySchoolName = selected.name;
                    this.searchQuery = selected.name;
                }
            }
        });
    }

    ngOnInit(): void {
        this.loadSchools();

        // Setup debounced search (300ms as per AC)
        this.searchSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntilDestroyed(this.destroyRef)
        ).subscribe((query) => {
            this.searchQuery = query;
            this.filterSchools();
        });
    }

    /**
     * Load schools from service
     */
    private loadSchools(): void {
        this.isLoading = true;
        this.schoolService.getActiveSchools().subscribe({
            next: () => {
                this.isLoading = false;
            },
            error: () => {
                this.isLoading = false;
            },
        });
    }

    /**
     * Filter schools based on search query
     */
    protected filterSchools(): void {
        if (!this.searchQuery) {
            this.filteredSchools = this.allSchools;
            return;
        }

        const query = this.searchQuery.toLowerCase();
        this.filteredSchools = this.allSchools.filter((school) =>
            school.name.toLowerCase().includes(query) ||
            (school.shortName && school.shortName.toLowerCase().includes(query))
        );
    }

    /**
     * Handle search input with debounce (300ms)
     */
    protected onSearchInput(event: Event): void {
        const input = event.target as HTMLInputElement;
        this.searchSubject.next(input.value);
    }

    /**
     * Handle input focus - open dropdown
     */
    protected onInputFocus(): void {
        this.isDropdownOpen = true;
        this.searchQuery = '';
        this.filterSchools();
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
     * Select a school
     */
    protected selectSchool(schoolId: number): void {
        this.schoolId.set(schoolId);
        const selected = this.allSchools.find((s) => s.id === schoolId);
        this.displaySchoolName = selected?.name || '';
        this.searchQuery = this.displaySchoolName; // Set search query to selected name
        this.isDropdownOpen = false;
        
        // Update Angular form control
        this.onChange(schoolId);
        this.onTouched();
    }

    /**
     * Clear selection
     */
    protected clearSelection(): void {
        this.schoolId.set(null);
        this.displaySchoolName = '';
        this.onChange(null);
        this.onTouched();
    }

    /**
     * Handle Create New School click
     */
    protected onCreateNew(): void {
        this.createNew.emit();
    }

    // ControlValueAccessor implementation
    writeValue(value: number | null): void {
        this.schoolId.set(value);
        if (value && this.allSchools.length > 0) {
            const selected = this.allSchools.find((s) => s.id === value);
            this.displaySchoolName = selected?.name || '';
            this.searchQuery = this.displaySchoolName;
        } else if (value) {
            // ID provided but schools not loaded yet - effect will handle it when loaded
            this.displaySchoolName = 'Loading...';
        } else {
            this.displaySchoolName = '';
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
