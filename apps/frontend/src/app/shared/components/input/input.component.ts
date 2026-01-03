import { Component, Input, forwardRef, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

/**
 * Accessible input component with built-in ARIA support (Story 1.5)
 *
 * Features:
 * - Angular Forms integration via ControlValueAccessor
 * - Automatic aria-invalid and aria-describedby attributes
 * - Error state indication
 * - DaisyUI styling with dark theme support
 * - Built-in focus ring accessibility
 *
 * @example
 * <app-input
 *   inputId="email"
 *   type="email"
 *   placeholder="Enter email"
 *   [hasError]="emailControl.invalid && emailControl.touched"
 *   [(ngModel)]="email">
 * </app-input>
 */
@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true,
    },
  ],
  template: `
    <input
      [id]="inputId"
      [type]="type"
      [placeholder]="placeholder"
      [disabled]="disabled"
      [required]="required"
      [autocomplete]="autocomplete"
      [value]="value"
      (input)="onInputChange($event)"
      (blur)="onTouched()"
      class="input input-bordered w-full"
      [class.input-error]="hasError"
      [attr.aria-invalid]="hasError ? 'true' : null"
      [attr.aria-describedby]="hasError && inputId ? inputId + '-error' : null"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InputComponent implements ControlValueAccessor {
  /**
   * Unique ID for the input element
   * Used for label association and aria-describedby
   */
  @Input() inputId = '';

  /**
   * Input type (text, email, password, etc.)
   */
  @Input() type: 'text' | 'email' | 'password' | 'tel' | 'url' | 'number' = 'text';

  /**
   * Placeholder text
   */
  @Input() placeholder = '';

  /**
   * Whether the input is disabled
   */
  @Input() disabled = false;

  /**
   * Whether the input is required
   */
  @Input() required = false;

  /**
   * Autocomplete attribute value
   */
  @Input() autocomplete = '';

  /**
   * Whether the input is in an error state
   * Automatically adds aria-invalid="true"
   */
  @Input() hasError = false;

  value = '';

  // ControlValueAccessor implementation
  onChange: (value: string) => void = () => {};
  onTouched: () => void = () => {};

  constructor(private cdr: ChangeDetectorRef) {}

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.value = input.value;
    this.onChange(this.value);
  }

  writeValue(value: string): void {
    this.value = value || '';
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this.cdr.markForCheck();
  }
}
