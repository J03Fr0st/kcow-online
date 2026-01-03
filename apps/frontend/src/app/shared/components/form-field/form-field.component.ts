import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Accessible form field wrapper component (Story 1.5)
 *
 * Provides consistent styling and accessibility features for form inputs:
 * - Proper label association
 * - Error message linking via aria-describedby
 * - Visible error states with aria-invalid
 * - DaisyUI dark theme styling
 *
 * @example
 * <app-form-field label="Email" [inputId]="email" [error]="emailError">
 *   <input id="email" type="email" class="input input-bordered w-full" />
 * </app-form-field>
 */
@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="form-control w-full">
      <label [for]="inputId" class="label">
        <span class="label-text">{{ label }}</span>
        @if (required) {
          <span class="label-text-alt text-error" aria-label="required">*</span>
        }
      </label>
      <ng-content></ng-content>
      @if (error) {
        <label class="label">
          <span [id]="inputId + '-error'" class="label-text-alt text-error" role="alert">
            {{ error }}
          </span>
        </label>
      }
      @if (hint && !error) {
        <label class="label">
          <span [id]="inputId + '-hint'" class="label-text-alt text-base-content/70">
            {{ hint }}
          </span>
        </label>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormFieldComponent {
  /**
   * Label text displayed above the input
   */
  @Input() label = '';

  /**
   * Unique ID for the input element (required for label association)
   * This should match the id attribute of the input element
   */
  @Input() inputId = '';

  /**
   * Error message to display below the input
   * When set, adds aria-describedby and role="alert" for screen readers
   */
  @Input() error = '';

  /**
   * Optional hint text displayed below the input when no error is present
   */
  @Input() hint = '';

  /**
   * Whether the field is required
   * Displays a visual asterisk indicator
   */
  @Input() required = false;
}
