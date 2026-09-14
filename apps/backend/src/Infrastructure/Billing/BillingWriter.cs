using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dapper;
using Kcow.Application.Billing;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Billing;

public sealed class BillingWriter(IDbConnectionFactory connections, ILogger<BillingWriter> logger) : IBillingWriter
{
    public Task<PaymentDto> RecordPaymentAsync(int studentId, CreatePaymentRequest request, string actor, string? key, CancellationToken ct)
    {
        Validate(request, request.PaymentDate, key);
        var fingerprint = JsonSerializer.Serialize(new { studentId, request.InvoiceId, request.PaymentDate,
            Amount = request.Amount.ToString("G29", CultureInfo.InvariantCulture), request.PaymentMethod, request.Notes });
        return CommitAsync(studentId, actor, "payment", key, fingerprint, async (db, tx) =>
        {
            Invoice? invoice = null;
            if (request.InvoiceId.HasValue)
            {
                invoice = await db.QuerySingleOrDefaultAsync<Invoice>(Command("SELECT * FROM invoices WHERE id=@Id", new { Id = request.InvoiceId }, tx, ct));
                if (invoice is null) throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} does not exist");
                BillingRules.ValidateInvoiceLink(studentId, invoice);
            }
            var payment = new PaymentDto { StudentId = studentId, InvoiceId = request.InvoiceId,
                PaymentDate = request.PaymentDate, Amount = request.Amount, PaymentMethod = request.PaymentMethod,
                Notes = request.Notes, ReceiptNumber = $"uncommitted-{Guid.NewGuid():N}", CreatedAt = DateTime.UtcNow.ToString("o") };
            payment.Id = await db.QuerySingleAsync<int>(Command("""
                INSERT INTO payments(student_id,invoice_id,payment_date,amount,payment_method,receipt_number,notes,created_at)
                VALUES(@StudentId,@InvoiceId,@PaymentDate,@Amount,@PaymentMethod,@ReceiptNumber,@Notes,@CreatedAt) RETURNING id
                """, payment, tx, ct));
            payment.ReceiptNumber = $"RCP-{request.PaymentDate.Replace("-", "")}-{payment.Id:D5}";
            await db.ExecuteAsync(Command("UPDATE payments SET receipt_number=@ReceiptNumber WHERE id=@Id", payment, tx, ct));
            await AuditAsync(db, tx, "Payment", payment.Id, "Created", null, $"Receipt: {payment.ReceiptNumber}", actor, ct);
            if (invoice is not null)
            {
                var amounts = await db.QueryAsync<decimal>(Command("SELECT amount FROM payments WHERE invoice_id=@Id AND student_id=@StudentId", invoice, tx, ct));
                if (amounts.Sum() >= invoice.Amount && invoice.Status != 1)
                {
                    await db.ExecuteAsync(Command("UPDATE invoices SET status=1 WHERE id=@Id", invoice, tx, ct));
                    await AuditAsync(db, tx, "Invoice", invoice.Id, "Status", BillingRules.InvoiceStatus(invoice.Status), "Paid", actor, ct);
                }
            }
            return payment;
        }, ct);
    }

    public Task<InvoiceDto> RecordInvoiceAsync(int studentId, CreateInvoiceRequest request, string actor, string? key, CancellationToken ct)
    {
        Validate(request, request.InvoiceDate, key);
        if (!DateOnly.TryParseExact(request.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new InvalidOperationException("DueDate must be a valid ISO date");
        var fingerprint = JsonSerializer.Serialize(new { studentId, request.InvoiceDate, request.DueDate,
            Amount = request.Amount.ToString("G29", CultureInfo.InvariantCulture), request.Description, request.Notes });
        return CommitAsync(studentId, actor, "invoice", key, fingerprint, async (db, tx) =>
        {
            var invoice = new InvoiceDto { StudentId = studentId, InvoiceDate = request.InvoiceDate, DueDate = request.DueDate,
                Amount = request.Amount, Status = 0, Description = request.Description, Notes = request.Notes, CreatedAt = DateTime.UtcNow.ToString("o") };
            invoice.Id = await db.QuerySingleAsync<int>(Command("""
                INSERT INTO invoices(student_id,invoice_date,due_date,amount,status,description,notes,created_at)
                VALUES(@StudentId,@InvoiceDate,@DueDate,@Amount,@Status,@Description,@Notes,@CreatedAt) RETURNING id
                """, invoice, tx, ct));
            await AuditAsync(db, tx, "Invoice", invoice.Id, "Created", null, "Invoice created", actor, ct);
            return invoice;
        }, ct);
    }

    private async Task<T> CommitAsync<T>(int studentId, string actor, string operation, string? key, string fingerprint,
        Func<IDbConnection, IDbTransaction, Task<T>> write, CancellationToken ct)
    {
        var started = System.Diagnostics.Stopwatch.StartNew();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint)));
        using var db = await connections.CreateAsync(ct);
        // SQLite's immediate transaction serializes command lookup and all following writes.
        using var tx = db.BeginTransaction();
        var identity = new { Actor = actor, Operation = operation, Key = key };
        if (key is not null)
        {
            var existing = await db.QuerySingleOrDefaultAsync<CommandReceipt>(Command("""
                SELECT request_hash AS RequestHash, result_json AS ResultJson FROM billing_commands
                WHERE actor=@Actor AND operation=@Operation AND command_key=@Key
                """, identity, tx, ct));
            if (existing is not null)
            {
                if (existing.RequestHash != hash) throw new IdempotencyConflictException();
                logger.LogInformation("Billing {Operation} replay for actor {Actor}; elapsed {ElapsedMs} ms", operation, actor, started.ElapsedMilliseconds);
                return JsonSerializer.Deserialize<T>(existing.ResultJson)!;
            }
        }
        if (!await db.ExecuteScalarAsync<bool>(Command("SELECT EXISTS(SELECT 1 FROM students WHERE id=@StudentId)", new { StudentId = studentId }, tx, ct)))
            throw new InvalidOperationException($"Student with ID {studentId} does not exist");
        var result = await write(db, tx);
        if (key is not null)
            await db.ExecuteAsync(Command("""
                INSERT INTO billing_commands(actor,operation,command_key,request_hash,result_json)
                VALUES(@Actor,@Operation,@Key,@Hash,@Json)
                """, new { Actor = actor, Operation = operation, Key = key, Hash = hash, Json = JsonSerializer.Serialize(result) }, tx, ct));
        ct.ThrowIfCancellationRequested();
        tx.Commit();
        logger.LogInformation("Billing {Operation} committed for student {StudentId} actor {Actor}; elapsed {ElapsedMs} ms", operation, studentId, actor, started.ElapsedMilliseconds);
        return result;
    }

    private static void Validate(object request, string date, string? key)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true))
            throw new InvalidOperationException(string.Join("; ", errors.Select(e => e.ErrorMessage)));
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new InvalidOperationException("Date must be a valid ISO date");
        if (key is not null && (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key.Any(char.IsControl)))
            throw new InvalidOperationException("Idempotency-Key must contain 1–128 printable characters");
    }

    private static CommandDefinition Command(string sql, object? args, IDbTransaction tx, CancellationToken ct) => new(sql, args, tx, cancellationToken: ct);
    private static Task<int> AuditAsync(IDbConnection db, IDbTransaction tx, string entity, int id, string field, string? oldValue, string value, string actor, CancellationToken ct) =>
        db.ExecuteAsync(Command("""
            INSERT INTO audit_log(entity_type,entity_id,field,old_value,new_value,changed_by,changed_at)
            VALUES(@Entity,@Id,@Field,@OldValue,@Value,@Actor,@Now)
            """, new { Entity = entity, Id = id, Field = field, OldValue = oldValue, Value = value, Actor = actor, Now = DateTime.UtcNow.ToString("o") }, tx, ct));

    private sealed class CommandReceipt
    {
        public string RequestHash { get; set; } = "";
        public string ResultJson { get; set; } = "";
    }
}
