import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BillingService } from '@core/services/billing.service';
import { StudentService, type StudentSearchResult } from '@core/services/student.service';
import type {
  BillingSummary,
  Invoice,
  Payment,
  InvoiceStatus,
  PaymentMethod,
} from '@features/billing/models/billing.model';
import { InvoiceStatusValues, PaymentMethodValues } from '@features/billing/models/billing.model';

@Component({
  selector: 'app-billing-overview',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './billing-overview.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingOverviewComponent {
  private readonly billingService = inject(BillingService);
  private readonly studentService = inject(StudentService);

  // Search state
  protected readonly searchQuery = signal('');
  protected readonly searchResults = signal<StudentSearchResult[]>([]);
  protected readonly isSearching = signal(false);

  // Selected student state
  protected readonly selectedStudent = signal<StudentSearchResult | null>(null);
  protected readonly billingSummary = signal<BillingSummary | null>(null);
  protected readonly invoices = signal<Invoice[]>([]);
  protected readonly payments = signal<Payment[]>([]);
  protected readonly isLoadingBilling = signal(false);
  protected readonly error = signal<string | null>(null);

  protected onSearchInput(query: string): void {
    this.searchQuery.set(query);
    if (query.trim().length < 2) {
      this.searchResults.set([]);
      return;
    }

    this.isSearching.set(true);
    this.studentService.searchStudents(query).subscribe({
      next: (results) => {
        this.searchResults.set(results);
        this.isSearching.set(false);
      },
      error: () => {
        this.searchResults.set([]);
        this.isSearching.set(false);
      },
    });
  }

  protected selectStudent(student: StudentSearchResult): void {
    this.selectedStudent.set(student);
    this.searchResults.set([]);
    this.searchQuery.set(student.fullName);
    this.loadBillingData(student.id);
  }

  protected clearSelection(): void {
    this.selectedStudent.set(null);
    this.billingSummary.set(null);
    this.invoices.set([]);
    this.payments.set([]);
    this.searchQuery.set('');
    this.error.set(null);
  }

  private loadBillingData(studentId: number): void {
    this.isLoadingBilling.set(true);
    this.error.set(null);

    // Load summary
    this.billingService.getBillingSummary(studentId).subscribe({
      next: (summary) => this.billingSummary.set(summary),
      error: (err) => this.error.set(err.detail || 'Failed to load billing summary'),
    });

    // Load invoices
    this.billingService.getInvoices(studentId).subscribe({
      next: (invoices) => this.invoices.set(invoices),
      error: () => {}, // summary error is sufficient
    });

    // Load payments
    this.billingService.getPayments(studentId).subscribe({
      next: (payments) => {
        this.payments.set(payments);
        this.isLoadingBilling.set(false);
      },
      error: () => this.isLoadingBilling.set(false),
    });
  }

  protected formatCurrency(amount: number): string {
    return `R ${amount.toFixed(2)}`;
  }

  protected getInvoiceStatus(statusValue: number): InvoiceStatus {
    return InvoiceStatusValues[statusValue] || 'Pending';
  }

  protected getInvoiceStatusClass(statusValue: number): string {
    switch (this.getInvoiceStatus(statusValue)) {
      case 'Paid':
        return 'badge-success';
      case 'Overdue':
        return 'badge-error';
      case 'Cancelled':
        return 'badge-ghost';
      default:
        return 'badge-warning';
    }
  }

  protected getPaymentMethod(methodValue: number): PaymentMethod {
    return PaymentMethodValues[methodValue] || 'Other';
  }
}
