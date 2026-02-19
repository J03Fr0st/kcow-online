import { animate, style, transition, trigger } from '@angular/animations';
import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  type ComponentRef,
  EventEmitter,
  HostListener,
  Input,
  type OnInit,
  Output,
  type Type,
  ViewChild,
  ViewContainerRef,
} from '@angular/core';
import type { Modal } from '@models/modal.model';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
  animations: [
    trigger('backdropAnimation', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-out', style({ opacity: 1 })),
      ]),
      transition(':leave', [animate('200ms ease-in', style({ opacity: 0 }))]),
    ]),
    trigger('modalAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.9)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'scale(0.9)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalComponent implements OnInit {
  @Input() modal!: Modal;
  @Output() close = new EventEmitter<unknown>();
  @Output() dismiss = new EventEmitter<unknown>();

  @ViewChild('dynamicContent', { read: ViewContainerRef })
  dynamicContent!: ViewContainerRef;

  private componentRef?: ComponentRef<Record<string, unknown>>;

  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: KeyboardEvent): void {
    if (this.modal.config.dismissible) {
      event.preventDefault();
      this.onDismiss();
    }
  }

  ngOnInit(): void {
    // Load dynamic component if provided
    if (this.modal.component && this.dynamicContent) {
      this.loadComponent(this.modal.component);
    }
  }

  ngAfterViewInit(): void {
    // Load component after view init if not loaded yet
    if (this.modal.component && !this.componentRef && this.dynamicContent) {
      this.loadComponent(this.modal.component);
    }
  }

  ngOnDestroy(): void {
    if (this.componentRef) {
      this.componentRef.destroy();
    }
  }

  private loadComponent(component: Type<Record<string, unknown>>): void {
    if (!this.dynamicContent) return;

    this.dynamicContent.clear();
    this.componentRef = this.dynamicContent.createComponent(component);

    // Pass data to component if it has a data property
    if (this.modal.config.data && this.componentRef.instance) {
      const data = this.modal.config.data;
      if (typeof data === 'object' && data !== null) {
        Object.assign(this.componentRef.instance, data);
      }
    }

    // Subscribe to component outputs if they exist
    const instance = this.componentRef.instance;
    const closeModal = instance.closeModal;
    if (closeModal && typeof (closeModal as { subscribe?: unknown }).subscribe === 'function') {
      (closeModal as { subscribe: (fn: (result: unknown) => void) => void }).subscribe(
        (result: unknown) => {
          this.onClose(result);
        },
      );
    }
    const dismissModal = instance.dismissModal;
    if (dismissModal && typeof (dismissModal as { subscribe?: unknown }).subscribe === 'function') {
      (dismissModal as { subscribe: (fn: (reason: unknown) => void) => void }).subscribe(
        (reason: unknown) => {
          this.onDismiss(reason);
        },
      );
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (
      this.modal.config.backdropDismiss &&
      (event.target as HTMLElement).classList.contains('modal-backdrop')
    ) {
      this.onDismiss();
    }
  }

  onClose(result?: unknown): void {
    this.close.emit(result);
  }

  onDismiss(reason?: unknown): void {
    this.dismiss.emit(reason);
  }

  isConfirmationDialog(): boolean {
    const data = this.modal.config.data;
    return (
      data != null &&
      typeof data === 'object' &&
      (data as Record<string, unknown>).type === 'confirmation'
    );
  }

  isAlertDialog(): boolean {
    const data = this.modal.config.data;
    return (
      data != null && typeof data === 'object' && (data as Record<string, unknown>).type === 'alert'
    );
  }

  onConfirm(): void {
    this.onClose(true);
  }

  onCancel(): void {
    this.onClose(false);
  }

  onOk(): void {
    this.onClose();
  }
}
