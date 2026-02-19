import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  type OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BillingService } from '@core/services/billing.service';
import { NotificationService } from '@core/services/notification.service';
import type {
  BillingSummary,
  CreateInvoiceRequest,
  CreatePaymentRequest,
  Invoice,
  InvoiceStatus,
  Payment,
  PaymentMethod,
} from '@features/billing/models/billing.model';
import { InvoiceStatusValues, PaymentMethodValues } from '@features/billing/models/billing.model';
import { forkJoin } from 'rxjs';
import { AuditTrailPanelComponent } from '../audit-trail-panel/audit-trail-panel.component';

interface PaymentForm {
  paymentDate: string;
  amount: number | null;
  paymentMethod: number;
  invoiceId: number | null;
  notes: string;
}

interface InvoiceForm {
  invoiceDate: string;
  amount: number | null;
  dueDate: string;
  description: string;
  notes: string;
}

@Component({
  selector: 'app-financial-tab',
  standalone: true,
  imports: [CommonModule, AuditTrailPanelComponent],
  templateUrl: './financial-tab.component.html',
  styleUrls: ['./financial-tab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FinancialTabComponent implements OnInit {
  private readonly billingService = inject(BillingService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Input signal for student ID
  readonly studentId = input.required<number>();

  // State signals
  readonly billingSummary = signal<BillingSummary | null>(null);
  readonly invoices = signal<Invoice[]>([]);
  readonly payments = signal<Payment[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  // Selected invoice for detail view
  readonly selectedInvoice = signal<Invoice | null>(null);

  // Payment form state
  readonly showPaymentForm = signal<boolean>(false);
  readonly isSavingPayment = signal<boolean>(false);
  readonly paymentForm = signal<PaymentForm>({
    paymentDate: new Date().toISOString().split('T')[0],
    amount: null,
    paymentMethod: 0, // Cash
    invoiceId: null,
    notes: '',
  });

  // Payment method options
  readonly paymentMethodOptions: { value: number; label: PaymentMethod }[] = [
    { value: 0, label: 'Cash' },
    { value: 1, label: 'Card' },
    { value: 2, label: 'EFT' },
    { value: 3, label: 'Other' },
  ];

  // Invoice form state
  readonly showInvoiceForm = signal<boolean>(false);
  readonly isSavingInvoice = signal<boolean>(false);
  readonly invoiceForm = signal<InvoiceForm>({
    invoiceDate: new Date().toISOString().split('T')[0],
    amount: null,
    dueDate: '',
    description: '',
    notes: '',
  });

  // Computed: sorted invoices by date descending
  readonly sortedInvoices = computed(() => {
    return [...this.invoices()].sort((a, b) => {
      return new Date(b.invoiceDate).getTime() - new Date(a.invoiceDate).getTime();
    });
  });

  // Computed: sorted payments by date descending
  readonly sortedPayments = computed(() => {
    return [...this.payments()].sort((a, b) => {
      return new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime();
    });
  });

  // Computed: pending invoices for dropdown
  readonly pendingInvoices = computed(() => {
    return this.invoices().filter((inv) => {
      const status = this.getInvoiceStatus(inv.status);
      return status === 'Pending' || status === 'Overdue';
    });
  });

  // Audit trail state
  readonly showAuditTrail = signal<boolean>(false);
  readonly auditEntityType = signal<string>('');
  readonly auditEntityId = signal<number>(0);

  ngOnInit(): void {
    this.loadBillingData();
  }

  /**
   * Load all billing data for the student
   */
  private loadBillingData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const studentId = this.studentId();

    forkJoin({
      summary: this.billingService.getBillingSummary(studentId),
      invoices: this.billingService.getInvoices(studentId),
      payments: this.billingService.getPayments(studentId),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ summary, invoices, payments }) => {
          this.billingSummary.set(summary);
          this.invoices.set(Array.isArray(invoices) ? invoices : []);
          this.payments.set(Array.isArray(payments) ? payments : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error loading billing data:', err);
          this.error.set('Failed to load billing data');
          this.isLoading.set(false);
        },
      });
  }

  /**
   * Format currency amount
   */
  formatCurrency(amount: number | null | undefined): string {
    if (amount === null || amount === undefined) return 'R 0.00';
    return `R ${amount.toLocaleString('en-ZA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string | null | undefined): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-ZA');
    } catch {
      return dateString;
    }
  }

  /**
   * Get invoice status label from numeric value
   */
  getInvoiceStatus(statusValue: number): InvoiceStatus {
    return InvoiceStatusValues[statusValue] || 'Pending';
  }

  /**
   * Get payment method label from numeric value
   */
  getPaymentMethodLabel(methodValue: number): PaymentMethod {
    return PaymentMethodValues[methodValue] || 'Other';
  }

  /**
   * Get status class for chip styling
   */
  getStatusClass(statusValue: number): string {
    const status = this.getInvoiceStatus(statusValue);
    switch (status) {
      case 'Paid':
        return 'badge-success';
      case 'Pending':
        return 'badge-warning';
      case 'Overdue':
        return 'badge-error';
      case 'Cancelled':
        return 'badge-ghost';
      default:
        return '';
    }
  }

  /**
   * Get balance color class based on amount
   */
  getBalanceClass(balance: number): string {
    if (balance <= 0) return 'text-success';
    if (balance > 0) return 'text-error';
    return '';
  }

  /**
   * Select invoice for detail view
   */
  selectInvoice(invoice: Invoice): void {
    this.selectedInvoice.set(invoice);
  }

  /**
   * Close invoice detail view
   */
  closeInvoiceDetail(): void {
    this.selectedInvoice.set(null);
  }

  /**
   * Show payment form
   */
  showAddPaymentForm(): void {
    this.showPaymentForm.set(true);
    this.paymentForm.set({
      paymentDate: new Date().toISOString().split('T')[0],
      amount: null,
      paymentMethod: 0,
      invoiceId: null,
      notes: '',
    });
  }

  /**
   * Cancel payment form
   */
  cancelPaymentForm(): void {
    this.showPaymentForm.set(false);
    this.paymentForm.set({
      paymentDate: new Date().toISOString().split('T')[0],
      amount: null,
      paymentMethod: 0,
      invoiceId: null,
      notes: '',
    });
  }

  /**
   * Update payment form field
   */
  updatePaymentForm(field: keyof PaymentForm, value: string | number | null): void {
    const current = this.paymentForm();
    this.paymentForm.set({ ...current, [field]: value });
  }

  /**
   * Check if payment form is valid
   */
  isPaymentFormValid(): boolean {
    const form = this.paymentForm();
    return form.amount !== null && form.amount > 0;
  }

  /**
   * Submit payment
   */
  submitPayment(): void {
    const form = this.paymentForm();

    if (!form.amount || form.amount <= 0) {
      this.notificationService.error('Please enter a valid payment amount', undefined, 5000);
      return;
    }

    this.isSavingPayment.set(true);

    const request: CreatePaymentRequest = {
      paymentDate: form.paymentDate,
      amount: form.amount,
      paymentMethod: form.paymentMethod,
      invoiceId: form.invoiceId ?? undefined,
      notes: form.notes || undefined,
    };

    this.billingService
      .createPayment(this.studentId(), request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (payment) => {
          // Add new payment to list
          this.payments.set([...this.payments(), payment]);
          // Reload billing data to update balance and invoice statuses
          this.loadBillingData();
          // Hide form
          this.cancelPaymentForm();
          this.isSavingPayment.set(false);
          this.notificationService.success(
            `Payment recorded successfully. Receipt: ${payment.receiptNumber}`,
            undefined,
            5000,
          );
        },
        error: (err) => {
          this.isSavingPayment.set(false);
          this.notificationService.error('Failed to record payment', undefined, 5000);
          console.error('Error recording payment:', err);
        },
      });
  }

  /**
   * Refresh billing data
   */
  refresh(): void {
    this.loadBillingData();
    this.notificationService.success('Billing data refreshed', undefined, 2000);
  }

  /**
   * Show invoice form
   */
  showAddInvoiceForm(): void {
    this.showInvoiceForm.set(true);
    this.invoiceForm.set({
      invoiceDate: new Date().toISOString().split('T')[0],
      amount: null,
      dueDate: '',
      description: '',
      notes: '',
    });
  }

  /**
   * Cancel invoice form
   */
  cancelInvoiceForm(): void {
    this.showInvoiceForm.set(false);
    this.invoiceForm.set({
      invoiceDate: new Date().toISOString().split('T')[0],
      amount: null,
      dueDate: '',
      description: '',
      notes: '',
    });
  }

  /**
   * Update invoice form field
   */
  updateInvoiceForm(field: keyof InvoiceForm, value: string | number | null): void {
    const current = this.invoiceForm();
    this.invoiceForm.set({ ...current, [field]: value });
  }

  /**
   * Check if invoice form is valid
   */
  isInvoiceFormValid(): boolean {
    const form = this.invoiceForm();
    if (!form.amount || form.amount <= 0 || !form.dueDate || !form.invoiceDate) return false;
    const today = new Date().toISOString().split('T')[0];
    return form.dueDate >= today;
  }

  /**
   * Submit invoice
   */
  submitInvoice(): void {
    const form = this.invoiceForm();

    if (!form.amount || form.amount <= 0) {
      this.notificationService.error('Please enter a valid invoice amount', undefined, 5000);
      return;
    }

    if (!form.dueDate) {
      this.notificationService.error('Please select a due date', undefined, 5000);
      return;
    }

    this.isSavingInvoice.set(true);

    const request: CreateInvoiceRequest = {
      invoiceDate: form.invoiceDate,
      amount: form.amount,
      dueDate: form.dueDate,
      description: form.description || undefined,
      notes: form.notes || undefined,
    };

    this.billingService
      .createInvoice(this.studentId(), request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (invoice) => {
          // Add new invoice to list
          this.invoices.set([...this.invoices(), invoice]);
          // Reload billing data to update balance
          this.loadBillingData();
          // Hide form
          this.cancelInvoiceForm();
          this.isSavingInvoice.set(false);
          this.notificationService.success(
            `Invoice created successfully. ID: #${invoice.id}`,
            undefined,
            5000,
          );
        },
        error: (err) => {
          this.isSavingInvoice.set(false);
          this.notificationService.error('Failed to create invoice', undefined, 5000);
          console.error('Error creating invoice:', err);
        },
      });
  }

  /**
   * View audit history for an invoice
   */
  viewInvoiceHistory(invoice: Invoice): void {
    this.auditEntityType.set('Invoice');
    this.auditEntityId.set(invoice.id);
    this.showAuditTrail.set(true);
  }

  /**
   * View audit history for a payment
   */
  viewPaymentHistory(payment: Payment): void {
    this.auditEntityType.set('Payment');
    this.auditEntityId.set(payment.id);
    this.showAuditTrail.set(true);
  }

  /**
   * Close audit trail panel
   */
  closeAuditTrail(): void {
    this.showAuditTrail.set(false);
    this.auditEntityType.set('');
    this.auditEntityId.set(0);
  }
}
