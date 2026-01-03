import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ModalService } from '@core/services/modal.service';
import { ModalComponent } from '../modal/modal.component';

@Component({
  selector: 'app-modal-container',
  standalone: true,
  imports: [CommonModule, ModalComponent],
  template: `
    @for (modal of modalService.modals$(); track modal.id) {
      <app-modal
        [modal]="modal"
        (close)="onClose(modal.id, $event)"
        (dismiss)="onDismiss(modal.id, $event)"
      />
    }
  `,
  styles: [],
})
export class ModalContainerComponent {
  constructor(public modalService: ModalService) {}

  onClose(id: string, result: any): void {
    this.modalService.close(id, result);
  }

  onDismiss(id: string, reason: any): void {
    this.modalService.dismiss(id, reason);
  }
}
