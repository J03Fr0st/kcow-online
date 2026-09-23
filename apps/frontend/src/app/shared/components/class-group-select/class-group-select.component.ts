import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  effect,
  forwardRef,
  inject,
  input,
  model,
} from '@angular/core';
import { type ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { ClassGroupService } from '@core/services/class-group.service';
import type { ClassGroup } from '@features/class-groups/models/class-group.model';

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
export class ClassGroupSelectComponent implements ControlValueAccessor {
  protected classGroupService = inject(ClassGroupService);

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
    effect(() => {
      const school = this.schoolId();
      if (school) this.classGroupService.loadClassGroups(school);
    });
    effect(() => {
      const school = this.schoolId();
      this.filteredClassGroups = school
        ? this.classGroupService.classGroups().filter((group) => group.schoolId === school)
        : [];
      this.isLoading = this.classGroupService.loading();
      const selected = this.filteredClassGroups.find((group) => group.id === this.classGroupId());
      this.displayClassName = selected?.name ?? '';
    });
  }

  /**
   * Select a class group
   */
  protected selectClassGroup(classGroupId: number): void {
    this.classGroupId.set(classGroupId);
    const selected = this.filteredClassGroups.find((cg) => cg.id === classGroupId);
    this.displayClassName = selected?.name ?? '';
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
      this.displayClassName = selected?.name ?? '';
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
