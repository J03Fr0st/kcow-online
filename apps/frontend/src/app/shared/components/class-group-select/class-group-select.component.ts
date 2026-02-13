import { ChangeDetectionStrategy, Component, inject, model, forwardRef, OnInit, effect, input, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ClassGroupService, type ClassGroup } from '@core/services/class-group.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-class-group-select',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './class-group-select.component.html',
    styleUrls: ['./class-group-select.component.scss'],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => ClassGroupSelectComponent),
            multi: true,
        },
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassGroupSelectComponent implements ControlValueAccessor, OnInit {
    protected classGroupService = inject(ClassGroupService);
    private destroyRef = inject(DestroyRef);

    // Input: School ID to filter class groups
    readonly schoolId = input<number | null>(null);

    // Two-way binding for classGroupId
    readonly classGroupId = model<number | null>(null);

    // Filtered class groups based on school
    protected filteredClassGroups: ClassGroup[] = [];

    // Loading state
    protected isLoading = false;

    // Disabled state
    protected isDisabled = false;

    // Display selected class group name
    protected displayClassName = '';

    // ControlValueAccessor callbacks
    private onChange: (value: number | null) => void = () => {};
    private onTouched: () => void = () => {};

    constructor() {
        // Use effect to reactively load class groups when schoolId changes
        effect(() => {
            const school = this.schoolId();
            this.loadClassGroups(school);
        });
    }

    ngOnInit(): void {
        // Initial load if schoolId is set
        const school = this.schoolId();
        if (school) {
            this.loadClassGroups(school);
        }
    }

    /**
     * Load class groups filtered by school
     */
    private loadClassGroups(schoolId: number | null): void {
        this.filteredClassGroups = []; // Clear previous results

        if (!schoolId) {
            return; // No school selected, don't load anything
        }

        this.isLoading = true;
        this.classGroupService.loadClassGroups(schoolId);

        this.classGroupService.classGroups.pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe({
            next: (classGroups) => {
                this.filteredClassGroups = classGroups;
                this.isLoading = false;

                // Update display name if current selection is still valid
                if (this.classGroupId()) {
                    const selected = this.filteredClassGroups.find((cg) => cg.id === this.classGroupId());
                    this.displayClassName = selected ? `${selected.name} (${selected.code})` : '';
                }
            },
            error: () => {
                this.isLoading = false;
            },
        });
    }

    /**
     * Select a class group
     */
    protected selectClassGroup(classGroupId: number): void {
        this.classGroupId.set(classGroupId);
        const selected = this.filteredClassGroups.find((cg) => cg.id === classGroupId);
        this.displayClassName = selected ? `${selected.name} (${selected.code})` : '';
        this.onChange(classGroupId);
        this.onTouched();
    }

    /**
     * Clear selection
     */
    protected clearSelection(): void {
        this.classGroupId.set(null);
        this.displayClassName = '';
        this.onChange(null);
        this.onTouched();
    }

    // ControlValueAccessor implementation
    writeValue(value: number | null): void {
        this.classGroupId.set(value);
        if (value && this.filteredClassGroups.length > 0) {
            const selected = this.filteredClassGroups.find((cg) => cg.id === value);
            this.displayClassName = selected ? `${selected.name} (${selected.code})` : '';
        } else {
            this.displayClassName = '';
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
