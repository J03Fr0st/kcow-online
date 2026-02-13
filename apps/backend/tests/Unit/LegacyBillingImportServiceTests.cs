using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Import;
using NSubstitute;

namespace Kcow.Unit.Tests;

/// <summary>
/// Unit tests for LegacyBillingImportService.
/// Note: Tests that require database connectivity (Dapper) should be in integration tests.
/// These tests focus on the mapper integration and summary generation.
/// </summary>
public class LegacyBillingImportServiceTests
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly LegacyBillingImportService _service;

    public LegacyBillingImportServiceTests()
    {
        _invoiceRepository = Substitute.For<IInvoiceRepository>();
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _connectionFactory = Substitute.For<IDbConnectionFactory>();

        _service = new LegacyBillingImportService(
            _invoiceRepository,
            _paymentRepository,
            _connectionFactory);
    }

    [Fact]
    public void LegacyBillingImportSummary_CalculatesTotalsCorrectly()
    {
        // Arrange & Act
        var summary = new LegacyBillingImportSummary(
            InvoicesImported: 10,
            InvoicesSkipped: 2,
            PaymentsImported: 15,
            PaymentsSkipped: 3,
            ErrorCount: 1,
            TotalBalanceImported: 5000m,
            CompletedAt: DateTime.UtcNow);

        // Assert
        Assert.Equal(25, summary.TotalImported); // 10 + 15
        Assert.Equal(5, summary.TotalSkipped);   // 2 + 3
    }

    [Fact]
    public void LegacyBillingImportSummary_TracksBalanceCorrectly()
    {
        // Arrange & Act
        var summary = new LegacyBillingImportSummary(
            InvoicesImported: 5,
            InvoicesSkipped: 0,
            PaymentsImported: 5,
            PaymentsSkipped: 0,
            ErrorCount: 0,
            TotalBalanceImported: -500m, // Negative means more payments than invoices
            CompletedAt: DateTime.UtcNow);

        // Assert
        Assert.Equal(-500m, summary.TotalBalanceImported);
    }

    [Fact]
    public void LegacyBillingImportSummary_HasCorrectCompletedAt()
    {
        // Arrange
        var testTime = new DateTime(2026, 2, 13, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var summary = new LegacyBillingImportSummary(
            InvoicesImported: 1,
            InvoicesSkipped: 0,
            PaymentsImported: 1,
            PaymentsSkipped: 0,
            ErrorCount: 0,
            TotalBalanceImported: 0m,
            CompletedAt: testTime);

        // Assert
        Assert.Equal(testTime, summary.CompletedAt);
    }
}
