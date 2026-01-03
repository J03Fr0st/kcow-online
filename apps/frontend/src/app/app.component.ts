import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ModalContainerComponent } from '@shared/components/modal-container/modal-container.component';
import { NotificationContainerComponent } from '@shared/components/notification-container/notification-container.component';

@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, NotificationContainerComponent, ModalContainerComponent],
  template: `
    <router-outlet />
    <app-notification-container />
    <app-modal-container />
  `,
})
export class AppComponent {}
