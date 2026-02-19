import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  effect,
  inject,
  input,
  Output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { type AuditLog, AuditLogService } from '@core/services/audit-log.service';

@Component({
  selector: 'app-audit-trail-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit-trail-panel.component.html',
  styleUrls: ['./audit-trail-panel.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditTrailPanelComponent {
  private readonly auditLogService = inject(AuditLogService);
  private readonly destroyRef = inject(DestroyRef);

  // Input signals
  readonly entityType = input.required<string>();
  readonly entityId = input.required<number>();
  readonly isOpen = input<boolean>(false);

  // Output to emit close event
  @Output() readonly close = new EventEmitter<void>();

  // State signals
  readonly auditLogs = signal<AuditLog[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  constructor() {
    // Watch for changes to isOpen and entityId inputs
    effect(
      () => {
        if (this.isOpen()) {
          this.loadAuditLogs();
        }
      },
      { allowSignalWrites: true },
    );
  }

  /**
   * Load audit logs for the entity
   */
  private loadAuditLogs(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.auditLogService
      .getAuditLogs(this.entityType(), this.entityId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (logs) => {
          this.auditLogs.set(Array.isArray(logs) ? logs : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load audit history');
          this.isLoading.set(false);
          console.error('Error loading audit logs:', err);
        },
      });
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    try {
      const date = new Date(dateString);
      return date.toLocaleString();
    } catch {
      return dateString;
    }
  }

  /**
   * Get display label for field names
   */
  getFieldLabel(field: string): string {
    const fieldLabels: Record<string, string> = {
      Status: 'Status',
      Notes: 'Notes',
      SessionDate: 'Session Date',
      ClassGroupId: 'Class Group',
      StudentId: 'Student',
      Amount: 'Amount',
      Description: 'Description',
      DueDate: 'Due Date',
      InvoiceDate: 'Invoice Date',
      PaymentDate: 'Payment Date',
      PaymentMethod: 'Payment Method',
      ReceiptNumber: 'Receipt Number',
      Created: 'Created',
    };
    return fieldLabels[field] || field;
  }

  /**
   * Handle close event
   */
  onClose(): void {
    this.close.emit();
  }
}
