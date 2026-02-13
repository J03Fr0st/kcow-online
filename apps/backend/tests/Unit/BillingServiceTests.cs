using Kcow.Application.Audit;
using Kcow.Application.Billing;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class BillingServiceTests
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAuditService _auditService;
    private readonly Infrastructure.Billing.BillingService _service;

    public BillingServiceTests()
    {
        _invoiceRepository = Substitute.For<IInvoiceRepository>();
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _studentRepository = Substitute.For<IStudentRepository>();
        _auditService = Substitute.For<IAuditService>();
        _service = new Infrastructure.Billing.BillingService(
            _invoiceRepository,
            _paymentRepository,
            _studentRepository,
            _auditService,
            NullLogger<Infrastructure.Billing.BillingService>.Instance);
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
    public async Task CreateInvoiceAsync_WithValidData_ReturnsInvoiceDto()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.CreateAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>()).Returns(42);

        var request = new CreateInvoiceRequest
        {
            InvoiceDate = "2026-02-01",
            Amount = 1500m,
            DueDate = "2026-03-01",
            Description = "Monthly tuition",
            Notes = "February billing"
        };

        // Act
        var result = await _service.CreateInvoiceAsync(1, request, "test-user");

        // Assert
        Assert.Equal(42, result.Id);
        Assert.Equal(1, result.StudentId);
        Assert.Equal(1500m, result.Amount);
        Assert.Equal("2026-02-01", result.InvoiceDate);
        Assert.Equal("2026-03-01", result.DueDate);
        Assert.Equal(0, result.Status); // Pending
        Assert.Equal("Monthly tuition", result.Description);
        await _invoiceRepository.Received(1).CreateAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInvoiceAsync_WithNonExistingStudent_ThrowsInvalidOperationException()
    {
        // Arrange
        _studentRepository.ExistsAsync(999, Arg.Any<CancellationToken>()).Returns(false);

        var request = new CreateInvoiceRequest
        {
            InvoiceDate = "2026-02-01",
            Amount = 1500m,
            DueDate = "2026-03-01"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateInvoiceAsync(999, request, "test-user"));
        Assert.Contains("Student with ID 999 does not exist", exception.Message);
    }

    // ---- Payment Tests ----

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

    [Fact]
    public async Task CreatePaymentAsync_WithValidData_ReturnsPaymentDto()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _paymentRepository.CreateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>()).Returns(42);
        _paymentRepository.UpdateReceiptNumberAsync(42, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var request = new CreatePaymentRequest
        {
            PaymentDate = "2026-02-10",
            Amount = 500m,
            PaymentMethod = 1, // Card
            Notes = "Card payment"
        };

        // Act
        var result = await _service.CreatePaymentAsync(1, request, "test-user");

        // Assert
        Assert.Equal(42, result.Id);
        Assert.Equal(1, result.StudentId);
        Assert.Equal(500m, result.Amount);
        Assert.Equal("2026-02-10", result.PaymentDate);
        Assert.Equal(1, result.PaymentMethod);
        Assert.Equal("RCP-20260210-00042", result.ReceiptNumber);
        await _paymentRepository.Received(1).CreateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
        await _paymentRepository.Received(1).UpdateReceiptNumberAsync(42, "RCP-20260210-00042", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePaymentAsync_WithInvoiceLink_ValidatesInvoiceExists()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.GetByIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(new Invoice { Id = 10, StudentId = 1, Amount = 1000m, Status = 0, InvoiceDate = "2026-01-01", DueDate = "2026-02-01", CreatedAt = "2026-01-01" });
        _paymentRepository.CreateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>()).Returns(42);
        _paymentRepository.UpdateReceiptNumberAsync(42, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _paymentRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Payment>
            {
                new() { Id = 42, StudentId = 1, InvoiceId = 10, Amount = 1000m, PaymentDate = "2026-02-10", ReceiptNumber = "RCP-20260210-00042", CreatedAt = "2026-02-10" }
            });
        _invoiceRepository.UpdateAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>()).Returns(true);

        var request = new CreatePaymentRequest
        {
            InvoiceId = 10,
            PaymentDate = "2026-02-10",
            Amount = 1000m,
            PaymentMethod = 2 // EFT
        };

        // Act
        var result = await _service.CreatePaymentAsync(1, request, "test-user");

        // Assert
        Assert.Equal(10, result.InvoiceId);
        // Invoice should be marked as Paid since payment >= invoice amount
        await _invoiceRepository.Received(1).UpdateAsync(
            Arg.Is<Invoice>(i => i.Status == 1), // Paid
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePaymentAsync_WithNonExistingInvoice_ThrowsInvalidOperationException()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((Invoice?)null);

        var request = new CreatePaymentRequest
        {
            InvoiceId = 999,
            PaymentDate = "2026-02-10",
            Amount = 500m,
            PaymentMethod = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreatePaymentAsync(1, request, "test-user"));
        Assert.Contains("Invoice with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task CreatePaymentAsync_WithNonExistingStudent_ThrowsInvalidOperationException()
    {
        // Arrange
        _studentRepository.ExistsAsync(999, Arg.Any<CancellationToken>()).Returns(false);

        var request = new CreatePaymentRequest
        {
            PaymentDate = "2026-02-10",
            Amount = 500m,
            PaymentMethod = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreatePaymentAsync(999, request, "test-user"));
        Assert.Contains("Student with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task CreatePaymentAsync_PartialPayment_DoesNotMarkInvoicePaid()
    {
        // Arrange
        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _invoiceRepository.GetByIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(new Invoice { Id = 10, StudentId = 1, Amount = 1000m, Status = 0, InvoiceDate = "2026-01-01", DueDate = "2026-02-01", CreatedAt = "2026-01-01" });
        _paymentRepository.CreateAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>()).Returns(42);
        _paymentRepository.UpdateReceiptNumberAsync(42, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _paymentRepository.GetByStudentIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Payment>
            {
                new() { Id = 42, StudentId = 1, InvoiceId = 10, Amount = 500m, PaymentDate = "2026-02-10", ReceiptNumber = "RCP-20260210-00042", CreatedAt = "2026-02-10" }
            });

        var request = new CreatePaymentRequest
        {
            InvoiceId = 10,
            PaymentDate = "2026-02-10",
            Amount = 500m,
            PaymentMethod = 0
        };

        // Act
        await _service.CreatePaymentAsync(1, request, "test-user");

        // Assert - Invoice should NOT be updated since partial payment
        await _invoiceRepository.DidNotReceive().UpdateAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
    }
}
