using Dapper;
using Kcow.Application.Billing;
using Kcow.Application.Audit;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kcow.Unit.Tests;

public class BillingTransactionTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"kcow-billing-{Guid.NewGuid():N}.db");
    private readonly SqliteConnectionFactory _factory;
    private readonly IBillingService _service;

    public BillingTransactionTests()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        _factory = new SqliteConnectionFactory($"Data Source={_path};Pooling=False");
        var migrator = new DbUpBootstrapper($"Data Source={_path};Pooling=False", NullLogger<DbUpBootstrapper>.Instance,
            Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts"));
        Assert.True(migrator.RunMigrations());
        using var db = _factory.Create();
        db.Execute("INSERT INTO students (id, reference) VALUES (1, 'ONE'), (2, 'TWO'); INSERT INTO invoices (id, student_id, invoice_date, due_date, amount, status) VALUES (10, 1, '2026-01-01', '2026-02-01', '100', 0)");
        _service = new BillingService(new InvoiceRepository(_factory), new PaymentRepository(_factory),
            new StudentRepository(_factory), new Infrastructure.Billing.BillingWriter(_factory, NullLogger<Infrastructure.Billing.BillingWriter>.Instance));
    }

    [Fact]
    public async Task PaymentCannotReferenceAnotherStudentsInvoice()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePaymentAsync(2,
            new CreatePaymentRequest { InvoiceId = 10, PaymentDate = "2026-02-10", Amount = 100, PaymentMethod = 0 }, "tester"));
        using var db = _factory.Create();
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM payments"));
    }

    [Fact]
    public async Task AuditFailureRollsBackPaymentAndReceipt()
    {
        using var db = _factory.Create();
        db.Execute("CREATE TRIGGER reject_audit BEFORE INSERT ON audit_log BEGIN SELECT RAISE(ABORT, 'audit unavailable'); END");
        await Assert.ThrowsAnyAsync<Exception>(() => _service.CreatePaymentAsync(1,
            new CreatePaymentRequest { InvoiceId = 10, PaymentDate = "2026-02-10", Amount = 100, PaymentMethod = 0 }, "tester"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM payments"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT status FROM invoices WHERE id=10"));
    }

    [Theory]
    [InlineData(40, 0)]
    [InlineData(100, 1)]
    public async Task PaymentCommitsReceiptAuditAndCorrectInvoiceStatus(int amount, int expectedStatus)
    {
        var payment = await _service.CreatePaymentAsync(1, Payment(amount), "tester");
        using var db = _factory.Create();
        Assert.Equal(amount, payment.Amount);
        Assert.Equal($"RCP-20260210-{payment.Id:D5}", payment.ReceiptNumber);
        Assert.Equal(payment.ReceiptNumber, db.QuerySingle<string>("SELECT receipt_number FROM payments"));
        Assert.Equal(expectedStatus, db.ExecuteScalar<int>("SELECT status FROM invoices WHERE id=10"));
        Assert.Equal(1 + expectedStatus, db.ExecuteScalar<int>("SELECT COUNT(*) FROM audit_log"));
    }

    [Fact]
    public async Task LostResponseReplayReturnsOriginalResultAndRejectsChangedInput()
    {
        var first = await _service.CreatePaymentAsync(1, Payment(), "tester", idempotencyKey: "same-command");
        var replay = await _service.CreatePaymentAsync(1, Payment(), "tester", idempotencyKey: "same-command");
        Assert.Equal(first.Id, replay.Id);
        Assert.Equal(first.CreatedAt, replay.CreatedAt);
        await Assert.ThrowsAsync<IdempotencyConflictException>(() => _service.CreatePaymentAsync(1, Payment(20), "tester", idempotencyKey: "same-command"));
        using var db = _factory.Create();
        Assert.Equal(1, db.ExecuteScalar<int>("SELECT COUNT(*) FROM payments"));
        Assert.Equal(1, db.ExecuteScalar<int>("SELECT COUNT(*) FROM billing_commands"));
        Assert.Equal(2, db.ExecuteScalar<int>("SELECT COUNT(*) FROM audit_log"));
    }

    [Fact]
    public async Task ConcurrentReplaysCommitOnlyOnce()
    {
        var payments = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            _service.CreatePaymentAsync(1, Payment(), "tester", idempotencyKey: "concurrent"))));
        Assert.Single(payments.Select(p => p.Id).Distinct());
        using var db = _factory.Create();
        Assert.Equal(1, db.ExecuteScalar<int>("SELECT COUNT(*) FROM payments"));
    }

    [Theory]
    [InlineData("BEFORE UPDATE ON payments")]
    [InlineData("BEFORE UPDATE ON invoices")]
    [InlineData("BEFORE INSERT ON billing_commands")]
    public async Task FailureAtAnyCommitStageRollsBackEverything(string trigger)
    {
        using var db = _factory.Create();
        db.Execute($"CREATE TRIGGER reject_stage {trigger} BEGIN SELECT RAISE(ABORT, 'injected failure'); END");
        await Assert.ThrowsAnyAsync<Exception>(() => _service.CreatePaymentAsync(1, Payment(), "tester", idempotencyKey: "retry-after-failure"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM payments"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM audit_log"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM billing_commands"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT status FROM invoices WHERE id=10"));
        db.Execute("DROP TRIGGER reject_stage");
        var result = await _service.CreatePaymentAsync(1, Payment(), "tester", idempotencyKey: "retry-after-failure");
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task InvoiceCreationIsAuditedAndReplayable()
    {
        var input = new CreateInvoiceRequest { InvoiceDate = "2026-02-01", DueDate = "2026-03-01", Amount = 40, Description = "Tuition" };
        var first = await _service.CreateInvoiceAsync(1, input, "tester", idempotencyKey: "invoice");
        var replay = await _service.CreateInvoiceAsync(1, input, "tester", idempotencyKey: "invoice");
        Assert.Equal(first.Id, replay.Id);
        Assert.Equal(40, first.Amount);
        Assert.Equal(0, first.Status);
        using var db = _factory.Create();
        Assert.Equal(2, db.ExecuteScalar<int>("SELECT COUNT(*) FROM invoices"));
        Assert.Equal(1, db.ExecuteScalar<int>("SELECT COUNT(*) FROM audit_log"));
    }

    [Fact]
    public async Task UnallocatedPaymentsRemainSupported()
    {
        var request = Payment(); request.InvoiceId = null;
        Assert.Null((await _service.CreatePaymentAsync(1, request, "tester")).InvoiceId);
    }

    [Fact]
    public async Task MissingEntitiesAndCancelledInvoiceRejectBeforeWrite()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePaymentAsync(999, Payment(), "tester"));
        var input = Payment(); input.InvoiceId = 999;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePaymentAsync(1, input, "tester"));
        using var db = _factory.Create(); db.Execute("UPDATE invoices SET status=3 WHERE id=10");
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePaymentAsync(1, Payment(), "tester"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateInvoiceAsync(999,
            new CreateInvoiceRequest { InvoiceDate = "2026-02-01", DueDate = "2026-03-01", Amount = 10 }, "tester"));
        Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM payments"));
    }

    private static CreatePaymentRequest Payment(decimal amount = 100) => new() { InvoiceId = 10, PaymentDate = "2026-02-10", Amount = amount, PaymentMethod = 0 };
    public void Dispose() => File.Delete(_path);
}
