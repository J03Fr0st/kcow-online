import { Injectable, signal } from '@angular/core';
import type { Notification, NotificationConfig } from '@models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly notifications = signal<Notification[]>([]);
  private readonly defaultDuration = 5000; // 5 seconds
  private idCounter = 0;

  readonly notifications$ = this.notifications.asReadonly();

  /**
   * Show a success notification
   */
  success(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'success',
      message,
      title,
      duration,
    });
  }

  /**
   * Show an error notification
   */
  error(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'error',
      message,
      title,
      duration,
    });
  }

  /**
   * Show a warning notification
   */
  warning(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'warning',
      message,
      title,
      duration,
    });
  }

  /**
   * Show an info notification
   */
  info(message: string, title?: string, duration?: number): string {
    return this.show({
      type: 'info',
      message,
      title,
      duration,
    });
  }

  /**
   * Show a notification with custom configuration
   */
  show(config: NotificationConfig): string {
    const id = this.generateId();
    const notification: Notification = {
      id,
      type: config.type,
      message: config.message,
      title: config.title,
      duration: config.duration ?? this.defaultDuration,
      dismissible: config.dismissible ?? true,
      timestamp: new Date(),
    };

    this.notifications.update((notifications) => [...notifications, notification]);

    // Auto-dismiss if duration is set
    if (notification.duration && notification.duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, notification.duration);
    }

    return id;
  }

  /**
   * Dismiss a notification by ID
   */
  dismiss(id: string): void {
    this.notifications.update((notifications) => notifications.filter((n) => n.id !== id));
  }

  /**
   * Clear all notifications
   */
  clearAll(): void {
    this.notifications.set([]);
  }

  private generateId(): string {
    return `notification-${Date.now()}-${this.idCounter++}`;
  }
}
