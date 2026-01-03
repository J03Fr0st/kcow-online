import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  template: `
    <div class="flex items-center justify-center" [class]="containerClass">
      <span class="loading loading-spinner" [class]="'loading-' + size + ' text-' + color"></span>
      @if (message) {
        <span class="ml-3">{{ message }}</span>
      }
    </div>
  `,
})
export class LoadingSpinnerComponent {
  @Input() size: 'xs' | 'sm' | 'md' | 'lg' = 'md';
  @Input() color = 'primary';
  @Input() message?: string;
  @Input() containerClass = 'p-4';
}
