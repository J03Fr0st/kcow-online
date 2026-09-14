import { HttpClient, type HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '@environments/environment';
import type {
  BillingSummary,
  CreateInvoiceRequest,
  CreatePaymentRequest,
  Invoice,
  Payment,
} from '@features/billing/models/billing.model';
import { catchError, type Observable, tap, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BillingService {
  private readonly http = inject(HttpClient);
  private readonly pendingCommands = new Map<string, string>();

  private commandKey(identity: string): string {
    const existing = this.pendingCommands.get(identity);
    if (existing) return existing;
    const key = crypto.randomUUID();
    this.pendingCommands.set(identity, key);
    return key;
  }

  /**
   * Get billing summary for a student.
   * @param studentId Student ID
   * @returns Observable of billing summary
   */
  getBillingSummary(studentId: number): Observable<BillingSummary> {
    return this.http
      .get<BillingSummary>(`${environment.apiUrl}/students/${studentId}/billing`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error loading billing summary for student ${studentId}:`, error);
          return throwError(() => error);
        }),
      );
  }

  /**
   * Get invoices for a student.
   * @param studentId Student ID
   * @returns Observable of invoices array
   */
  getInvoices(studentId: number): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${environment.apiUrl}/students/${studentId}/invoices`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading invoices for student ${studentId}:`, error);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Create a new invoice for a student.
   * @param studentId Student ID
   * @param request Invoice data to create
   * @returns Observable of created invoice
   */
  createInvoice(studentId: number, request: CreateInvoiceRequest): Observable<Invoice> {
    const identity = JSON.stringify(['invoices', studentId, request]);
    const key = this.commandKey(identity);
    return this.http
      .post<Invoice>(`${environment.apiUrl}/students/${studentId}/invoices`, request, { headers: { 'Idempotency-Key': key } })
      .pipe(
        tap(() => this.pendingCommands.delete(identity)),
        catchError((error: HttpErrorResponse) => {
          console.error(`Error creating invoice for student ${studentId}:`, error);
          return throwError(() => error);
        }),
      );
  }

  /**
   * Get payments for a student.
   * @param studentId Student ID
   * @returns Observable of payments array
   */
  getPayments(studentId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${environment.apiUrl}/students/${studentId}/payments`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`Error loading payments for student ${studentId}:`, error);
        return throwError(() => error);
      }),
    );
  }

  /**
   * Create a new payment for a student.
   * @param studentId Student ID
   * @param request Payment data to create
   * @returns Observable of created payment
   */
  createPayment(studentId: number, request: CreatePaymentRequest): Observable<Payment> {
    const identity = JSON.stringify(['payments', studentId, request]);
    const key = this.commandKey(identity);
    return this.http
      .post<Payment>(`${environment.apiUrl}/students/${studentId}/payments`, request, { headers: { 'Idempotency-Key': key } })
      .pipe(
        tap(() => this.pendingCommands.delete(identity)),
        catchError((error: HttpErrorResponse) => {
          console.error(`Error creating payment for student ${studentId}:`, error);
          return throwError(() => error);
        }),
      );
  }
}
