using Kcow.Application.Billing;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class BillingServiceTests
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly BillingService _service;

    public BillingServiceTests()
    {
        _invoiceRepository = Substitute.For<IInvoiceRepository>();
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _studentRepository = Substitute.For<IStudentRepository>();
        _service = new BillingService(
            _invoiceRepository,
            _paymentRepository,
            _studentRepository,
            Substitute.For<IBillingWriter>());
    }

    // ---- Billing Summary Tests ----

    [Fact]
    public async Task GetBillingSummaryAsync_WithValidStudent_ReturnsSummary()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice>
            {
                new() { Id = 1, StudentId = 1, Amount = 1000m, Status = 0, InvoiceDate = "2026-01-01", DueDate = "2026-02-01", CreatedAt = "2026-01-01" },
                new() { Id = 2, StudentId = 1, Amount = 2000m, Status = 2, InvoiceDate = "2025-12-01", DueDate = "2025-12-31", CreatedAt = "2025-12-01" }
            });
        _paymentRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Payment>
            {
                new() { Id = 1, StudentId = 1, Amount = 500m, PaymentDate = "2026-01-15", ReceiptNumber = "RCP-20260115-00001", CreatedAt = "2026-01-15" }
            });

        // Act
        var result = await _service.GetBillingSummaryAsync(1);

        // Assert
        Assert.Equal(1, result.StudentId);
        Assert.Equal(2500m, result.CurrentBalance); // 3000 - 500
        Assert.Equal(3000m, result.TotalInvoiced);
        Assert.Equal(500m, result.TotalPaid);
        Assert.Equal(2000m, result.OverdueAmount);
        Assert.Equal("2026-01-15", result.LastPaymentDate);
        Assert.Equal(500m, result.LastPaymentAmount);
        Assert.Equal(2, result.OutstandingInvoicesCount); // 1 pending + 1 overdue
    }

    [Fact]
    public async Task GetBillingSummaryAsync_WithNoInvoicesOrPayments_ReturnsZeroSummary()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice>());
        _paymentRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Payment>());

        // Act
        var result = await _service.GetBillingSummaryAsync(1);

        // Assert
        Assert.Equal(0m, result.CurrentBalance);
        Assert.Equal(0m, result.TotalInvoiced);
        Assert.Equal(0m, result.TotalPaid);
        Assert.Equal(0m, result.OverdueAmount);
        Assert.Null(result.LastPaymentDate);
        Assert.Null(result.LastPaymentAmount);
        Assert.Equal(0, result.OutstandingInvoicesCount);
    }

    [Fact]
    public async Task GetBillingSummaryAsync_WithNonExistingStudent_ThrowsInvalidOperationException()
    {
        // Arrange
        _studentRepository.ExistsAsync(999, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetBillingSummaryAsync(999));
        Assert.Contains("Student with ID 999 does not exist", exception.Message);
    }

    // ---- Invoice Tests ----

    [Fact]
    public async Task GetInvoicesByStudentIdAsync_ReturnsInvoiceList()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Invoice>
            {
                new() { Id = 1, StudentId = 1, Amount = 1000m, Status = 0, InvoiceDate = "2026-01-01", DueDate = "2026-02-01", Description = "Tuition", CreatedAt = "2026-01-01" },
                new() { Id = 2, StudentId = 1, Amount = 500m, Status = 1, InvoiceDate = "2025-12-01", DueDate = "2025-12-31", CreatedAt = "2025-12-01" }
            });

        // Act
        var result = await _service.GetInvoicesByStudentIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1000m, result[0].Amount);
        Assert.Equal("Tuition", result[0].Description);
        Assert.Equal(0, result[0].Status);
    }

    [Fact]
    public async Task GetPaymentsByStudentIdAsync_ReturnsPaymentList()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _paymentRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Payment>
            {
                new() { Id = 1, StudentId = 1, Amount = 500m, PaymentDate = "2026-01-15", PaymentMethod = 0, ReceiptNumber = "RCP-20260115-00001", CreatedAt = "2026-01-15" },
                new() { Id = 2, StudentId = 1, Amount = 750m, PaymentDate = "2026-02-01", PaymentMethod = 2, ReceiptNumber = "RCP-20260201-00002", InvoiceId = 1, CreatedAt = "2026-02-01" }
            });

        // Act
        var result = await _service.GetPaymentsByStudentIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(500m, result[0].Amount);
        Assert.Equal(0, result[0].PaymentMethod); // Cash
        Assert.StartsWith("RCP-", result[0].ReceiptNumber);
    }
}
