/**
 * Invoice status enum matching backend InvoiceStatus enum
 */
export type InvoiceStatus = 'Pending' | 'Paid' | 'Overdue' | 'Cancelled';

/**
 * Invoice status numeric values from backend
 */
export const InvoiceStatusValues: Record<number, InvoiceStatus> = {
  0: 'Pending',
  1: 'Paid',
  2: 'Overdue',
  3: 'Cancelled',
};

/**
 * Payment method enum matching backend PaymentMethod enum
 */
export type PaymentMethod = 'Cash' | 'Card' | 'Transfer' | 'Other';

/**
 * Payment method numeric values from backend
 */
export const PaymentMethodValues: Record<number, PaymentMethod> = {
  0: 'Cash',
  1: 'Card',
  2: 'Transfer',
  3: 'Other',
};

/**
 * Billing summary returned from /api/students/{studentId}/billing
 */
export interface BillingSummary {
  studentId: number;
  currentBalance: number;
  totalInvoiced: number;
  totalPaid: number;
  overdueAmount: number;
  lastPaymentDate: string | null;
  lastPaymentAmount: number | null;
  outstandingInvoicesCount: number;
}

/**
 * Invoice returned from /api/students/{studentId}/invoices
 */
export interface Invoice {
  id: number;
  studentId: number;
  invoiceDate: string;
  amount: number;
  dueDate: string;
  status: number;
  description?: string;
  notes?: string;
  createdAt: string;
}

/**
 * Payment returned from /api/students/{studentId}/payments
 */
export interface Payment {
  id: number;
  studentId: number;
  invoiceId?: number;
  paymentDate: string;
  amount: number;
  paymentMethod: number;
  receiptNumber: string;
  notes?: string;
  createdAt: string;
}

/**
 * Request to create a new invoice
 */
export interface CreateInvoiceRequest {
  invoiceDate: string;
  amount: number;
  dueDate: string;
  description?: string;
  notes?: string;
}

/**
 * Request to create a new payment
 */
export interface CreatePaymentRequest {
  invoiceId?: number;
  paymentDate: string;
  amount: number;
  paymentMethod: number;
  notes?: string;
}
