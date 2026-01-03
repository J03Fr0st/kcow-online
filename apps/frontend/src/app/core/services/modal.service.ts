import { Injectable, signal, type Type } from '@angular/core';
import type { ConfirmationConfig, Modal, ModalConfig } from '@models/modal.model';

@Injectable({
  providedIn: 'root',
})
export class ModalService {
  private readonly modals = signal<Modal[]>([]);
  private idCounter = 0;

  readonly modals$ = this.modals.asReadonly();

  /**
   * Open a modal with a component
   */
  open<T = any>(component: Type<any>, config: ModalConfig = {}): Promise<T> {
    return new Promise((resolve, reject) => {
      const id = this.generateId();
      const modal: Modal = {
        id,
        component,
        config: {
          dismissible: true,
          backdropDismiss: true,
          showCloseButton: true,
          size: 'md',
          ...config,
        },
        resolve,
        reject,
      };

      this.modals.update((modals) => [...modals, modal]);
    });
  }

  /**
   * Open a confirmation dialog
   */
  confirm(config: ConfirmationConfig): Promise<boolean> {
    return new Promise((resolve, reject) => {
      const id = this.generateId();
      const modal: Modal = {
        id,
        config: {
          title: config.title || 'Confirm',
          dismissible: true,
          backdropDismiss: true,
          showCloseButton: true,
          size: config.size || 'sm',
          data: {
            type: 'confirmation',
            message: config.message,
            confirmText: config.confirmText || 'Confirm',
            cancelText: config.cancelText || 'Cancel',
            confirmClass: config.confirmClass || 'btn-primary',
            cancelClass: config.cancelClass || 'btn-secondary',
          },
        },
        resolve,
        reject,
      };

      this.modals.update((modals) => [...modals, modal]);
    });
  }

  /**
   * Alert dialog (info message with single OK button)
   */
  alert(message: string, title: string = 'Alert'): Promise<void> {
    return new Promise((resolve, reject) => {
      const id = this.generateId();
      const modal: Modal = {
        id,
        config: {
          title,
          dismissible: true,
          backdropDismiss: true,
          showCloseButton: true,
          size: 'sm',
          data: {
            type: 'alert',
            message,
          },
        },
        resolve,
        reject,
      };

      this.modals.update((modals) => [...modals, modal]);
    });
  }

  /**
   * Close a specific modal by ID
   */
  close(id: string, result?: any): void {
    const modal = this.modals().find((m) => m.id === id);
    if (modal) {
      modal.resolve(result);
      this.removeModal(id);
    }
  }

  /**
   * Dismiss a modal (reject the promise)
   */
  dismiss(id: string, reason?: any): void {
    const modal = this.modals().find((m) => m.id === id);
    if (modal) {
      modal.reject(reason);
      this.removeModal(id);
    }
  }

  /**
   * Close all modals
   */
  closeAll(): void {
    this.modals().forEach((modal) => {
      modal.reject('All modals closed');
    });
    this.modals.set([]);
  }

  /**
   * Get a modal by ID
   */
  getModal(id: string): Modal | undefined {
    return this.modals().find((m) => m.id === id);
  }

  private removeModal(id: string): void {
    this.modals.update((modals) => modals.filter((m) => m.id !== id));
  }

  private generateId(): string {
    return `modal-${Date.now()}-${this.idCounter++}`;
  }
}
