import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ModalService } from '@core/services/modal.service';

@Component({
  selector: 'app-simple-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="simple-modal">
      <p class="message">{{ message }}</p>

      <div class="actions" *ngIf="!nested">
        <button class="btn btn-outline" (click)="onClose()">
          Close
        </button>
        <button class="btn btn-primary" (click)="onSave()">
          Save Changes
        </button>
      </div>

      <div class="actions" *ngIf="nested">
        <button class="btn btn-secondary" (click)="openNested()">
          Open Another Modal
        </button>
        <button class="btn btn-primary" (click)="onClose()">
          Close This Modal
        </button>
      </div>
    </div>
  `,
  styles: [
    `
    .simple-modal {
      padding: 1rem 0;
    }

    .message {
      font-size: 1rem;
      color: var(--text-color);
      margin-bottom: 1.5rem;
      line-height: 1.6;
    }

    .actions {
      display: flex;
      gap: 0.75rem;
      justify-content: flex-end;
    }

    .btn {
      padding: 0.5rem 1.5rem;
      border-radius: 4px;
      border: none;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;

      &:focus {
        outline: 2px solid var(--primary-color);
        outline-offset: 2px;
      }

      &.btn-primary {
        background-color: var(--primary-color);
        color: white;

        &:hover {
          background-color: var(--primary-hover);
        }
      }

      &.btn-secondary {
        background-color: var(--secondary-color, #6c757d);
        color: white;

        &:hover {
          background-color: var(--secondary-hover, #5a6268);
        }
      }

      &.btn-outline {
        background-color: transparent;
        color: var(--text-color);
        border: 1px solid var(--border-color);

        &:hover {
          background-color: var(--hover-background);
        }
      }
    }
  `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SimpleModalComponent {
  private modalService = inject(ModalService);

  @Input() message: string = 'This is a simple modal';
  @Input() nested: boolean = false;
  @Output() closeModal = new EventEmitter<unknown>();
  @Output() dismissModal = new EventEmitter<unknown>();

  onClose(): void {
    this.closeModal.emit(null);
  }

  onSave(): void {
    this.closeModal.emit({ saved: true, timestamp: new Date() });
  }

  openNested(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Second Modal',
        size: 'sm',
        data: {
          message: 'This is the second modal! Modals can be nested.',
          nested: false,
        },
      })
      .then(() => {
        console.log('Second modal closed');
      })
      .catch(() => {
        console.log('Second modal dismissed');
      });
  }
}
