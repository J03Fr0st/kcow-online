import { HttpClient } from '@angular/common/http';
import { DestroyRef } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { BillingService } from '@core/services/billing.service';
import { NotificationService } from '@core/services/notification.service';
import type {
  BillingSummary,
  CreateInvoiceRequest,
  Invoice,
  Payment,
} from '@features/billing/models/billing.model';
import { of, throwError } from 'rxjs';
import { FinancialTabComponent } from './financial-tab.component';

describe('FinancialTabComponent', () => {
  let component: FinancialTabComponent;
  let fixture: ComponentFixture<FinancialTabComponent>;
  let billingServiceSpy: any;
  let notificationServiceSpy: any;

  const mockBillingSummary: BillingSummary = {
    studentId: 1,
    currentBalance: 1500.0,
    totalInvoiced: 5000.0,
    totalPaid: 3500.0,
    overdueAmount: 500.0,
    lastPaymentDate: '2026-01-15',
    lastPaymentAmount: 500.0,
    outstandingInvoicesCount: 2,
  };

  const mockInvoices: Invoice[] = [
    {
      id: 1,
      studentId: 1,
      invoiceDate: '2026-01-01',
      amount: 1000.0,
      dueDate: '2026-01-15',
      status: 0, // Pending
      description: 'January tuition',
      createdAt: '2026-01-01',
    },
    {
      id: 2,
      studentId: 1,
      invoiceDate: '2025-12-01',
      amount: 2000.0,
      dueDate: '2025-12-15',
      status: 1, // Paid
      description: 'December tuition',
      createdAt: '2025-12-01',
    },
  ];

  const mockPayments: Payment[] = [
    {
      id: 1,
      studentId: 1,
      invoiceId: 2,
      paymentDate: '2026-01-15',
      amount: 500.0,
      paymentMethod: 0, // Cash
      receiptNumber: 'RCP-001234',
      notes: 'Partial payment',
      createdAt: '2026-01-15',
    },
  ];

  beforeEach(async () => {
    billingServiceSpy = {
      getBillingSummary: jest.fn(),
      getInvoices: jest.fn(),
      getPayments: jest.fn(),
      createPayment: jest.fn(),
      createInvoice: jest.fn(),
    };

    notificationServiceSpy = {
      success: jest.fn(),
      error: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [FinancialTabComponent],
      providers: [
        { provide: BillingService, useValue: billingServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: HttpClient, useValue: {} },
        { provide: DestroyRef, useValue: { destroy: () => {} } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialTabComponent);
    component = fixture.componentInstance;

    // Set the required input signal
    (component as any).studentId = () => 1;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Loading billing data', () => {
    beforeEach(() => {
      (component as any).studentId = () => 1;
    });

    it('should load billing summary on init', () => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));

      component.ngOnInit();
      fixture.detectChanges();

      expect(billingServiceSpy.getBillingSummary).toHaveBeenCalledWith(1);
      expect(component.billingSummary()).toEqual(mockBillingSummary);
    });

    it('should load invoices on init', () => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of([]));

      component.ngOnInit();
      fixture.detectChanges();

      expect(billingServiceSpy.getInvoices).toHaveBeenCalledWith(1);
      expect(component.invoices()).toEqual(mockInvoices);
    });

    it('should load payments on init', () => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of(mockPayments));

      component.ngOnInit();
      fixture.detectChanges();

      expect(billingServiceSpy.getPayments).toHaveBeenCalledWith(1);
      expect(component.payments()).toEqual(mockPayments);
    });

    it('should handle error when loading billing data', () => {
      billingServiceSpy.getBillingSummary.mockReturnValue(throwError(() => new Error('API Error')));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.error()).toBe('Failed to load billing data');
    });

    it('should handle empty arrays', () => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.invoices()).toEqual([]);
      expect(component.payments()).toEqual([]);
    });
  });

  describe('Sorting data', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of(mockPayments));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should sort invoices by date descending', () => {
      const sorted = component.sortedInvoices();
      expect(sorted.length).toBe(2);
      expect(sorted[0].id).toBe(1); // 2026-01-01
      expect(sorted[1].id).toBe(2); // 2025-12-01
    });

    it('should sort payments by date descending', () => {
      const sorted = component.sortedPayments();
      expect(sorted.length).toBe(1);
      expect(sorted[0].id).toBe(1);
    });
  });

  describe('Currency formatting', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should format positive amounts correctly', () => {
      const formatted = component.formatCurrency(1500.5);
      // en-ZA locale uses space as thousands separator and comma as decimal
      expect(formatted).toMatch(/^R\s*1[\s,]500[,.]50$/);
    });

    it('should format zero amount', () => {
      const formatted = component.formatCurrency(0);
      expect(formatted).toMatch(/^R\s*0[,.]00$/);
    });

    it('should handle null', () => {
      const formatted = component.formatCurrency(null);
      expect(formatted).toMatch(/^R\s*0[,.]00$/);
    });

    it('should handle undefined', () => {
      const formatted = component.formatCurrency(undefined);
      expect(formatted).toMatch(/^R\s*0[,.]00$/);
    });
  });

  describe('Date formatting', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should format date string correctly', () => {
      const formatted = component.formatDate('2026-01-15');
      expect(formatted).toBeTruthy();
    });

    it('should handle null date', () => {
      const formatted = component.formatDate(null);
      expect(formatted).toBe('-');
    });

    it('should handle undefined date', () => {
      const formatted = component.formatDate(undefined);
      expect(formatted).toBe('-');
    });
  });

  describe('Invoice status', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should return Pending for status 0', () => {
      expect(component.getInvoiceStatus(0)).toBe('Pending');
    });

    it('should return Paid for status 1', () => {
      expect(component.getInvoiceStatus(1)).toBe('Paid');
    });

    it('should return Overdue for status 2', () => {
      expect(component.getInvoiceStatus(2)).toBe('Overdue');
    });

    it('should return Cancelled for status 3', () => {
      expect(component.getInvoiceStatus(3)).toBe('Cancelled');
    });
  });

  describe('Status class', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should return warning for Pending', () => {
      expect(component.getStatusClass(0)).toBe('badge-warning');
    });

    it('should return success for Paid', () => {
      expect(component.getStatusClass(1)).toBe('badge-success');
    });

    it('should return error for Overdue', () => {
      expect(component.getStatusClass(2)).toBe('badge-error');
    });

    it('should return ghost for Cancelled', () => {
      expect(component.getStatusClass(3)).toBe('badge-ghost');
    });
  });

  describe('Balance class', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should return success for zero balance', () => {
      expect(component.getBalanceClass(0)).toBe('text-success');
    });

    it('should return success for negative balance (credit)', () => {
      expect(component.getBalanceClass(-100)).toBe('text-success');
    });

    it('should return error for positive balance (owed)', () => {
      expect(component.getBalanceClass(100)).toBe('text-error');
    });
  });

  describe('Invoice selection', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should select invoice', () => {
      expect(component.selectedInvoice()).toBeNull();

      component.selectInvoice(mockInvoices[0]);
      expect(component.selectedInvoice()).toEqual(mockInvoices[0]);
    });

    it('should deselect invoice', () => {
      component.selectInvoice(mockInvoices[0]);
      component.closeInvoiceDetail();
      expect(component.selectedInvoice()).toBeNull();
    });
  });

  describe('Refresh', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
    });

    it('should refresh data and show success notification', () => {
      component.refresh();

      // Just verify that the notification was called
      expect(notificationServiceSpy.success).toHaveBeenCalledWith(
        'Billing data refreshed',
        undefined,
        2000,
      );
    });
  });

  describe('Payment Form', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of(mockPayments));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should show payment form', () => {
      expect(component.showPaymentForm()).toBe(false);

      component.showAddPaymentForm();
      expect(component.showPaymentForm()).toBe(true);
    });

    it('should hide payment form on cancel', () => {
      component.showAddPaymentForm();
      component.cancelPaymentForm();
      expect(component.showPaymentForm()).toBe(false);
    });

    it('should reset payment form on cancel', () => {
      component.showAddPaymentForm();
      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: 100,
        paymentMethod: 1,
        invoiceId: 1,
        notes: 'Test',
      });
      component.cancelPaymentForm();

      expect(component.paymentForm().amount).toBeNull();
      expect(component.paymentForm().paymentMethod).toBe(0);
      expect(component.paymentForm().invoiceId).toBeNull();
      expect(component.paymentForm().notes).toBe('');
    });

    it('should update payment form field', () => {
      component.updatePaymentForm('amount', 500);
      expect(component.paymentForm().amount).toBe(500);

      component.updatePaymentForm('paymentMethod', 2);
      expect(component.paymentForm().paymentMethod).toBe(2);

      component.updatePaymentForm('invoiceId', 1);
      expect(component.paymentForm().invoiceId).toBe(1);

      component.updatePaymentForm('notes', 'Test note');
      expect(component.paymentForm().notes).toBe('Test note');
    });

    it('should validate payment form - amount required', () => {
      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: null,
        paymentMethod: 0,
        invoiceId: null,
        notes: '',
      });
      expect(component.isPaymentFormValid()).toBe(false);

      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: 0,
        paymentMethod: 0,
        invoiceId: null,
        notes: '',
      });
      expect(component.isPaymentFormValid()).toBe(false);

      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: 100,
        paymentMethod: 0,
        invoiceId: null,
        notes: '',
      });
      expect(component.isPaymentFormValid()).toBe(true);
    });

    it('should filter pending invoices for dropdown', () => {
      const pending = component.pendingInvoices();
      // mockInvoices has one Pending (status 0) and one Paid (status 1)
      expect(pending.length).toBe(1);
      expect(pending[0].status).toBe(0);
    });
  });

  describe('Submit Payment', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of(mockPayments));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should submit payment successfully', () => {
      const newPayment: Payment = {
        id: 2,
        studentId: 1,
        paymentDate: '2026-02-13',
        amount: 500,
        paymentMethod: 0,
        receiptNumber: 'RCP-002345',
        notes: 'Test payment',
        createdAt: '2026-02-13',
      };
      billingServiceSpy.createPayment.mockReturnValue(of(newPayment));

      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: 500,
        paymentMethod: 0,
        invoiceId: null,
        notes: 'Test payment',
      });
      component.submitPayment();

      expect(billingServiceSpy.createPayment).toHaveBeenCalledWith(1, {
        paymentDate: expect.any(String),
        amount: 500,
        paymentMethod: 0,
        invoiceId: undefined,
        notes: 'Test payment',
      });
      expect(notificationServiceSpy.success).toHaveBeenCalled();
      expect(component.showPaymentForm()).toBe(false);
    });

    it('should show error when submitting with invalid amount', () => {
      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: null,
        paymentMethod: 0,
        invoiceId: null,
        notes: '',
      });
      component.submitPayment();

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'Please enter a valid payment amount',
        undefined,
        5000,
      );
      expect(billingServiceSpy.createPayment).not.toHaveBeenCalled();
    });

    it('should handle payment creation error', () => {
      billingServiceSpy.createPayment.mockReturnValue(throwError(() => new Error('API Error')));

      component.paymentForm.set({
        paymentDate: '2026-02-13',
        amount: 500,
        paymentMethod: 0,
        invoiceId: null,
        notes: '',
      });
      component.submitPayment();

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'Failed to record payment',
        undefined,
        5000,
      );
      expect(component.isSavingPayment()).toBe(false);
    });
  });

  describe('Payment Method Label', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of([]));
      billingServiceSpy.getPayments.mockReturnValue(of([]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should return correct payment method label', () => {
      expect(component.getPaymentMethodLabel(0)).toBe('Cash');
      expect(component.getPaymentMethodLabel(1)).toBe('Card');
      expect(component.getPaymentMethodLabel(2)).toBe('EFT');
      expect(component.getPaymentMethodLabel(3)).toBe('Other');
    });
  });

  describe('Invoice Form', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of(mockPayments));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should show invoice form', () => {
      expect(component.showInvoiceForm()).toBe(false);

      component.showAddInvoiceForm();
      expect(component.showInvoiceForm()).toBe(true);
    });

    it('should hide invoice form on cancel', () => {
      component.showAddInvoiceForm();
      component.cancelInvoiceForm();
      expect(component.showInvoiceForm()).toBe(false);
    });

    it('should reset invoice form on cancel', () => {
      component.showAddInvoiceForm();
      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 100,
        dueDate: '2026-02-20',
        description: 'Test invoice',
        notes: 'Test notes',
      });
      component.cancelInvoiceForm();

      expect(component.invoiceForm().amount).toBeNull();
      expect(component.invoiceForm().dueDate).toBe('');
      expect(component.invoiceForm().description).toBe('');
      expect(component.invoiceForm().notes).toBe('');
    });

    it('should update invoice form field', () => {
      component.updateInvoiceForm('amount', 500);
      expect(component.invoiceForm().amount).toBe(500);

      component.updateInvoiceForm('dueDate', '2026-02-20');
      expect(component.invoiceForm().dueDate).toBe('2026-02-20');

      component.updateInvoiceForm('description', 'Test description');
      expect(component.invoiceForm().description).toBe('Test description');

      component.updateInvoiceForm('notes', 'Test note');
      expect(component.invoiceForm().notes).toBe('Test note');
    });

    it('should validate invoice form - amount required', () => {
      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: null,
        dueDate: '2026-12-20',
        description: '',
        notes: '',
      });
      expect(component.isInvoiceFormValid()).toBe(false);

      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 0,
        dueDate: '2026-12-20',
        description: '',
        notes: '',
      });
      expect(component.isInvoiceFormValid()).toBe(false);

      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 100,
        dueDate: '2026-12-20',
        description: '',
        notes: '',
      });
      expect(component.isInvoiceFormValid()).toBe(true);
    });

    it('should validate invoice form - due date required', () => {
      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 100,
        dueDate: '',
        description: '',
        notes: '',
      });
      expect(component.isInvoiceFormValid()).toBe(false);

      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 100,
        dueDate: '2026-12-20',
        description: '',
        notes: '',
      });
      expect(component.isInvoiceFormValid()).toBe(true);
    });
  });

  describe('Submit Invoice', () => {
    beforeEach(() => {
      billingServiceSpy.getBillingSummary.mockReturnValue(of(mockBillingSummary));
      billingServiceSpy.getInvoices.mockReturnValue(of(mockInvoices));
      billingServiceSpy.getPayments.mockReturnValue(of(mockPayments));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should submit invoice successfully', () => {
      const newInvoice: Invoice = {
        id: 3,
        studentId: 1,
        invoiceDate: '2026-02-13',
        amount: 500,
        dueDate: '2026-12-27',
        status: 0, // Pending
        description: 'Test invoice',
        notes: 'Test notes',
        createdAt: '2026-02-13',
      };
      billingServiceSpy.createInvoice.mockReturnValue(of(newInvoice));

      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 500,
        dueDate: '2026-12-27',
        description: 'Test invoice',
        notes: 'Test notes',
      });
      component.submitInvoice();

      expect(billingServiceSpy.createInvoice).toHaveBeenCalledWith(1, {
        invoiceDate: expect.any(String),
        amount: 500,
        dueDate: '2026-12-27',
        description: 'Test invoice',
        notes: 'Test notes',
      } as CreateInvoiceRequest);
      expect(notificationServiceSpy.success).toHaveBeenCalled();
      expect(component.showInvoiceForm()).toBe(false);
    });

    it('should show error when submitting with invalid amount', () => {
      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: null,
        dueDate: '2026-12-27',
        description: '',
        notes: '',
      });
      component.submitInvoice();

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'Please enter a valid invoice amount',
        undefined,
        5000,
      );
      expect(billingServiceSpy.createInvoice).not.toHaveBeenCalled();
    });

    it('should show error when submitting without due date', () => {
      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 500,
        dueDate: '',
        description: '',
        notes: '',
      });
      component.submitInvoice();

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'Please select a due date',
        undefined,
        5000,
      );
      expect(billingServiceSpy.createInvoice).not.toHaveBeenCalled();
    });

    it('should handle invoice creation error', () => {
      billingServiceSpy.createInvoice.mockReturnValue(throwError(() => new Error('API Error')));

      component.invoiceForm.set({
        invoiceDate: '2026-02-13',
        amount: 500,
        dueDate: '2026-12-27',
        description: '',
        notes: '',
      });
      component.submitInvoice();

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'Failed to create invoice',
        undefined,
        5000,
      );
      expect(component.isSavingInvoice()).toBe(false);
    });
  });
});
