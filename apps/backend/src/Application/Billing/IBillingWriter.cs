namespace Kcow.Application.Billing;

/// <summary>Commits a billing command and its required audit rows atomically.
/// A supplied key is scoped to actor and command kind and retained without expiry.
/// Replays return the original result; changed input with the same key is rejected.</summary>
public interface IBillingWriter
{
    Task<PaymentDto> RecordPaymentAsync(int studentId, CreatePaymentRequest request, string actor, string? key, CancellationToken cancellationToken);
    Task<InvoiceDto> RecordInvoiceAsync(int studentId, CreateInvoiceRequest request, string actor, string? key, CancellationToken cancellationToken);
}

public sealed class IdempotencyConflictException : InvalidOperationException
{
    public IdempotencyConflictException() : base("The idempotency key was already used with different input.") { }
}
