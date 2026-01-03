import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '@core/services/notification.service';
import { CardComponent } from '@shared/components/card/card.component';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule, CardComponent],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css'],
})
export class NotificationsComponent {
  private readonly notificationService = inject(NotificationService);

  customMessage = 'This is a custom notification message';
  customTitle = 'Custom Title';
  customDuration = 5000;

  showSuccess(): void {
    this.notificationService.success('Operation completed successfully!', 'Success');
  }

  showError(): void {
    this.notificationService.error('An error occurred while processing your request.', 'Error');
  }

  showWarning(): void {
    this.notificationService.warning('Please review your input before proceeding.', 'Warning');
  }

  showInfo(): void {
    this.notificationService.info('New updates are available for your application.', 'Information');
  }

  showCustom(): void {
    this.notificationService.show({
      type: 'info',
      message: this.customMessage,
      title: this.customTitle,
      duration: this.customDuration,
      dismissible: true,
    });
  }

  showMultiple(): void {
    this.notificationService.success('First notification', 'Success');
    setTimeout(() => {
      this.notificationService.info('Second notification', 'Info');
    }, 300);
    setTimeout(() => {
      this.notificationService.warning('Third notification', 'Warning');
    }, 600);
  }

  showPersistent(): void {
    this.notificationService.show({
      type: 'info',
      message: 'This notification will not auto-dismiss. Click the X to close.',
      title: 'Persistent Notification',
      duration: 0,
      dismissible: true,
    });
  }

  clearAll(): void {
    this.notificationService.clearAll();
  }
}
