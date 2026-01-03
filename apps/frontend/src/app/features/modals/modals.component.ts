import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ModalService } from '@core/services/modal.service';
import { NotificationService } from '@core/services/notification.service';
import { CardComponent } from '@shared/components/card/card.component';
import { FormModalComponent } from './form-modal/form-modal.component';
import { SimpleModalComponent } from './simple-modal/simple-modal.component';

@Component({
  selector: 'app-modals',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './modals.component.html',
  styleUrls: ['./modals.component.scss'],
})
export class ModalsComponent {
  private modalService = inject(ModalService);
  private notificationService = inject(NotificationService);

  openSimpleModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Simple Modal',
        size: 'md',
        data: {
          message: 'This is a simple modal with dynamic content!',
        },
      })
      .then((result) => {
        this.notificationService.success(`Modal closed with result: ${JSON.stringify(result)}`);
      })
      .catch((reason) => {
        this.notificationService.info(`Modal dismissed: ${reason || 'No reason provided'}`);
      });
  }

  openFormModal(): void {
    this.modalService
      .open(FormModalComponent, {
        title: 'User Form',
        size: 'lg',
        backdropDismiss: false,
      })
      .then((result) => {
        if (result) {
          this.notificationService.success(`Form submitted: ${result.name} (${result.email})`);
        }
      })
      .catch((_reason) => {
        this.notificationService.info('Form cancelled');
      });
  }

  openSmallModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Small Modal',
        size: 'sm',
        data: {
          message: 'This is a small modal.',
        },
      })
      .then(() => {
        this.notificationService.success('Small modal closed');
      })
      .catch(() => {});
  }

  openLargeModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Large Modal',
        size: 'lg',
        data: {
          message: 'This is a large modal with more space for content.',
        },
      })
      .then(() => {
        this.notificationService.success('Large modal closed');
      })
      .catch(() => {});
  }

  openXLModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Extra Large Modal',
        size: 'xl',
        data: {
          message: 'This is an extra large modal.',
        },
      })
      .then(() => {
        this.notificationService.success('XL modal closed');
      })
      .catch(() => {});
  }

  openFullScreenModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Full Screen Modal',
        size: 'full',
        data: {
          message: 'This modal takes up almost the entire screen.',
        },
      })
      .then(() => {
        this.notificationService.success('Full screen modal closed');
      })
      .catch(() => {});
  }

  openNoDismissModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'Cannot Dismiss',
        size: 'md',
        dismissible: false,
        backdropDismiss: false,
        showCloseButton: false,
        data: {
          message:
            'This modal cannot be dismissed with ESC or backdrop click. Use the close button in the content.',
        },
      })
      .then(() => {
        this.notificationService.success('Modal closed via button');
      })
      .catch(() => {});
  }

  async openConfirmation(): Promise<void> {
    try {
      const confirmed = await this.modalService.confirm({
        title: 'Confirm Action',
        message: 'Are you sure you want to proceed with this action?',
        confirmText: 'Yes, proceed',
        cancelText: 'Cancel',
        confirmClass: 'btn-primary',
        cancelClass: 'btn-outline',
      });

      if (confirmed) {
        this.notificationService.success('Action confirmed!');
      } else {
        this.notificationService.info('Action cancelled');
      }
    } catch (_error) {
      this.notificationService.info('Confirmation dismissed');
    }
  }

  async openDangerConfirmation(): Promise<void> {
    try {
      const confirmed = await this.modalService.confirm({
        title: 'Delete Item',
        message: 'Are you sure you want to delete this item? This action cannot be undone.',
        confirmText: 'Delete',
        cancelText: 'Cancel',
        confirmClass: 'btn-danger',
        cancelClass: 'btn-outline',
      });

      if (confirmed) {
        this.notificationService.success('Item deleted successfully!');
      } else {
        this.notificationService.info('Deletion cancelled');
      }
    } catch (_error) {
      this.notificationService.info('Confirmation dismissed');
    }
  }

  async openAlert(): Promise<void> {
    try {
      await this.modalService.alert(
        'This is an important message that requires your attention.',
        'Information',
      );
      this.notificationService.success('Alert acknowledged');
    } catch (_error) {
      this.notificationService.info('Alert dismissed');
    }
  }

  openNestedModal(): void {
    this.modalService
      .open(SimpleModalComponent, {
        title: 'First Modal',
        size: 'md',
        data: {
          message: 'This is the first modal. You can open another modal from here.',
          nested: true,
        },
      })
      .then(() => {
        this.notificationService.success('First modal closed');
      })
      .catch(() => {});
  }
}
