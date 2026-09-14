import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { BillingService } from './billing.service';

describe('Billing command identity', () => {
  it('retains a command key after a lost response and retires it after success', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(BillingService);
    const backend = TestBed.inject(HttpTestingController);
    const input = { amount: 100, paymentDate: '2026-02-10', paymentMethod: 0 };
    service.createPayment(1, input).subscribe({ error: () => undefined });
    const first = backend.expectOne(r => r.url.endsWith('/payments'));
    const key = first.request.headers.get('Idempotency-Key');
    expect(key).toBeTruthy();
    first.error(new ProgressEvent('error'));
    service.createPayment(1, { ...input }).subscribe();
    const retry = backend.expectOne(r => r.url.endsWith('/payments'));
    expect(retry.request.headers.get('Idempotency-Key')).toBe(key);
    retry.flush({ id: 42 });
    service.createPayment(1, input).subscribe();
    const next = backend.expectOne(r => r.url.endsWith('/payments'));
    expect(next.request.headers.get('Idempotency-Key')).not.toBe(key);
    next.flush({ id: 43 });
    backend.verify();
  });
});
