import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, signal } from '@angular/core';

@Component({
  selector: 'app-collapsible-section',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="collapsible-section" [class.expanded]="isExpanded()">
      <button
        type="button"
        class="collapsible-header"
        (click)="toggle()"
        [attr.aria-expanded]="isExpanded()"
        [attr.aria-controls]="sectionId"
      >
        <div class="header-content">
          <svg 
            class="chevron-icon" 
            [class.rotated]="isExpanded()"
            xmlns="http://www.w3.org/2000/svg" 
            fill="none" 
            viewBox="0 0 24 24" 
            stroke-width="2" 
            stroke="currentColor"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
          </svg>
          <span class="section-title">{{ title }}</span>
          @if (badge) {
            <span class="badge badge-sm badge-ghost">{{ badge }}</span>
          }
        </div>
        @if (description) {
          <span class="section-description">{{ description }}</span>
        }
      </button>
      <div 
        [id]="sectionId" 
        class="collapsible-content"
        [class.hidden]="!isExpanded()"
      >
        <div class="content-inner">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
    .collapsible-section {
      border: 1px solid oklch(var(--b3));
      border-radius: 0.5rem;
      margin-bottom: 0.75rem;
      overflow: hidden;
      background-color: oklch(var(--b1));
    }

    .collapsible-header {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      width: 100%;
      padding: 0.75rem 1rem;
      background-color: oklch(var(--b2));
      border: none;
      cursor: pointer;
      text-align: left;
      transition: background-color 0.2s ease;

      &:hover {
        background-color: oklch(var(--b3));
      }
    }

    .header-content {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .chevron-icon {
      width: 1rem;
      height: 1rem;
      transition: transform 0.2s ease;
      color: oklch(var(--bc) / 0.6);

      &.rotated {
        transform: rotate(90deg);
      }
    }

    .section-title {
      font-weight: 600;
      font-size: 0.875rem;
      color: oklch(var(--bc));
    }

    .section-description {
      font-size: 0.75rem;
      color: oklch(var(--bc) / 0.6);
      margin-top: 0.25rem;
      margin-left: 1.5rem;
    }

    .collapsible-content {
      max-height: 2000px;
      overflow: hidden;
      transition: max-height 0.3s ease-in-out;

      &.hidden {
        max-height: 0;
      }
    }

    .content-inner {
      padding: 1rem;
      border-top: 1px solid oklch(var(--b3));
    }
  `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CollapsibleSectionComponent {
  @Input() title = 'Section';
  @Input() description?: string;
  @Input() badge?: string;
  @Input() initiallyExpanded = false;

  private static idCounter = 0;
  readonly sectionId = `collapsible-section-${++CollapsibleSectionComponent.idCounter}`;

  isExpanded = signal(false);

  ngOnInit() {
    this.isExpanded.set(this.initiallyExpanded);
  }

  toggle() {
    this.isExpanded.update((v) => !v);
  }

  expand() {
    this.isExpanded.set(true);
  }

  collapse() {
    this.isExpanded.set(false);
  }
}
