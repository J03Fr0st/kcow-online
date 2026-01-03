import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  template: `
    <div class="card bg-base-100 shadow-xl" [class]="customClass">
      @if (title) {
        <div class="card-body">
          <h2 class="card-title">
            {{ title }}
            @if (badge) {
              <div class="badge badge-secondary">{{ badge }}</div>
            }
          </h2>
          @if (description) {
            <p class="text-sm opacity-70">{{ description }}</p>
          }
          <ng-content></ng-content>
          @if (hasActions) {
            <div class="card-actions justify-end mt-4">
              <ng-content select="[actions]"></ng-content>
            </div>
          }
        </div>
      } @else {
        <div class="card-body">
          <ng-content></ng-content>
        </div>
      }
    </div>
  `,
})
export class CardComponent {
  @Input() title?: string;
  @Input() description?: string;
  @Input() badge?: string;
  @Input() customClass?: string;
  @Input() hasActions = false;
}
