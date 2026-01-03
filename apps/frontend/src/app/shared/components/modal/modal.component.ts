import { animate, style, transition, trigger } from '@angular/animations';
import { CommonModule } from '@angular/common';
import {
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
})
export class ModalComponent implements OnInit {
  @Input() modal!: Modal;
  @Output() close = new EventEmitter<any>();
  @Output() dismiss = new EventEmitter<any>();

  @ViewChild('dynamicContent', { read: ViewContainerRef })
  dynamicContent!: ViewContainerRef;

  private componentRef?: ComponentRef<any>;

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

  private loadComponent(component: Type<any>): void {
    if (!this.dynamicContent) return;

    this.dynamicContent.clear();
    this.componentRef = this.dynamicContent.createComponent(component);

    // Pass data to component if it has a data property
    if (this.modal.config.data && this.componentRef.instance) {
      Object.assign(this.componentRef.instance, this.modal.config.data);
    }

    // Subscribe to component outputs if they exist
    const instance = this.componentRef.instance;
    if (instance.closeModal) {
      instance.closeModal.subscribe((result: any) => {
        this.onClose(result);
      });
    }
    if (instance.dismissModal) {
      instance.dismissModal.subscribe((reason: any) => {
        this.onDismiss(reason);
      });
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

  onClose(result?: any): void {
    this.close.emit(result);
  }

  onDismiss(reason?: any): void {
    this.dismiss.emit(reason);
  }

  isConfirmationDialog(): boolean {
    return this.modal.config.data?.type === 'confirmation';
  }

  isAlertDialog(): boolean {
    return this.modal.config.data?.type === 'alert';
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
