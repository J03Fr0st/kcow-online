using Kcow.Application.Import;

namespace Kcow.Unit.Tests;

public class LegacyBillingMapperTests
{
    private readonly HashSet<int> _validStudentIds = new() { 1, 2, 3 };
    private readonly Dictionary<string, int> _referenceMapping = new()
    {
        { "REF001", 1 },
        { "REF002", 2 },
        { "REF003", 3 }
    };
    private readonly Dictionary<int, decimal> _schoolPrices = new()
    {
        { 10, 500m },
        { 20, 750m },
        { 30, 1000m }
    };

    [Fact]
    public void Map_WithValidStudentId_ReturnsInvoiceAndPayment()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: "FC001",
            Charge: "1500",
            Deposit: "500",
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.NotNull(result.Payment);
        Assert.Empty(result.Warnings);

        Assert.Equal(1, result.Invoice.StudentId);
        Assert.Equal(1500m, result.Invoice.Amount);
        Assert.Equal("2026-01-15", result.Invoice.InvoiceDate);
        Assert.Equal(0, result.Invoice.Status); // Pending

        Assert.Equal(1, result.Payment.StudentId);
        Assert.Equal(500m, result.Payment.Amount);
        Assert.Equal("2026-01-15", result.Payment.PaymentDate);
        Assert.Equal(3, result.Payment.PaymentMethod); // Other (legacy)
        Assert.StartsWith("RCP-LEGACY-", result.Payment.ReceiptNumber);
    }

    [Fact]
    public void Map_WithReferenceOnly_LooksUpStudentId()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 0, // No direct ID
            FamilyId: null,
            SchoolId: null,
            StudentReference: "REF002",
            FinancialCode: null,
            Charge: "2000",
            Deposit: null,
            PayDate: "2026-02-01",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.Equal(2, result.Invoice.StudentId); // Looked up from reference
        Assert.Equal(2000m, result.Invoice.Amount);
    }

    [Fact]
    public void Map_WithInvalidStudent_ReturnsNullWithWarning()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 999, // Invalid
            FamilyId: null,
            SchoolId: null,
            StudentReference: "INVALID",
            FinancialCode: null,
            Charge: "1000",
            Deposit: "500",
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.Null(result.Invoice);
        Assert.Null(result.Payment);
        Assert.Single(result.Warnings);
        Assert.Contains("Student not found", result.Warnings[0]);
    }

    [Fact]
    public void Map_WithNoChargeButSchoolPrice_UsesSchoolPrice()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10, // School with 500 price
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: null, // No charge
            Deposit: "200",
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.Equal(500m, result.Invoice.Amount); // School price
        Assert.Contains("school default rate", result.Invoice.Description);
        Assert.Contains("school default rate", result.Invoice.Notes);
    }

    [Fact]
    public void Map_WithNoChargeAndNoSchoolPrice_ReturnsNoInvoice()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 99, // School not in prices dict
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: null,
            Deposit: "500",
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.Null(result.Invoice);
        Assert.NotNull(result.Payment);
        Assert.Equal(500m, result.Payment.Amount);
    }

    [Fact]
    public void Map_WithInvalidChargeButValidSchoolPrice_UsesSchoolPrice()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 20, // School with 750 price
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "invalid", // Invalid charge
            Deposit: null,
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.Equal(750m, result.Invoice.Amount); // School price as fallback
        Assert.Contains(result.Warnings, w => w.Contains("Could not parse Charge"));
    }

    [Fact]
    public void Map_WithNoDeposit_ReturnsNoPayment()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "1500",
            Deposit: null, // No deposit
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.Null(result.Payment);
        Assert.Equal(1500m, result.Invoice.Amount);
    }

    [Fact]
    public void Map_WithTshirtPayments_ReturnsAllPayments()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "1500",
            Deposit: "500",
            PayDate: "2026-01-15",
            TshirtMoney1: "100",
            TshirtMoneyDate1: "2026-01-20",
            TshirtMoney2: "150",
            TshirtMoneyDate2: "2026-02-01");

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Payment); // Deposit payment
        Assert.NotNull(result.TshirtPayment1);
        Assert.NotNull(result.TshirtPayment2);

        Assert.Equal(500m, result.Payment.Amount);
        Assert.Equal(100m, result.TshirtPayment1.Amount);
        Assert.Equal("2026-01-20", result.TshirtPayment1.PaymentDate);
        Assert.Contains("T-shirt payment 1", result.TshirtPayment1.Notes);

        Assert.Equal(150m, result.TshirtPayment2.Amount);
        Assert.Equal("2026-02-01", result.TshirtPayment2.PaymentDate);
        Assert.Contains("T-shirt payment 2", result.TshirtPayment2.Notes);
    }

    [Fact]
    public void Map_WithCurrencySymbol_ParsesAmountCorrectly()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "R1500.00", // South African Rand format
            Deposit: "R500",
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.NotNull(result.Payment);
        Assert.Equal(1500m, result.Invoice.Amount);
        Assert.Equal(500m, result.Payment.Amount);
    }

    [Fact]
    public void Map_WithInvalidAmount_ReturnsNullAndWarning()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: null,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "invalid",
            Deposit: "500",
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.Null(result.Invoice);
        Assert.NotNull(result.Payment);
        Assert.Contains(result.Warnings, w => w.Contains("Could not parse Charge"));
    }

    [Fact]
    public void Map_WithFinancialCode_IncludesInNotes()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: "FC-2026-001",
            Charge: "1500",
            Deposit: null,
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.Contains("FC-2026-001", result.Invoice.Notes);
        Assert.Contains("FC-2026-001", result.Invoice.Description);
    }

    [Fact]
    public void Map_WithNoPayDate_UsesCurrentDate()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "1500",
            Deposit: "500",
            PayDate: null, // No date
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.NotNull(result.Payment);
        // Should have a valid date format
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result.Invoice.InvoiceDate);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result.Payment.PaymentDate);
    }

    [Fact]
    public void Map_GeneratesUniqueReceiptNumbers()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var records = new[]
        {
            new LegacyBillingImportRecord(1, null, 10, "REF001", null, "1000", "500", "2026-01-15", null, null, null, null),
            new LegacyBillingImportRecord(2, null, 20, "REF002", null, "1000", "500", "2026-01-15", null, null, null, null),
            new LegacyBillingImportRecord(3, null, 30, "REF003", null, "1000", "500", "2026-01-15", null, null, null, null)
        };

        // Act
        var results = records.Select(r => mapper.Map(r)).ToList();
        var receiptNumbers = results
            .Where(r => r.Payment != null)
            .Select(r => r.Payment!.ReceiptNumber)
            .ToList();

        // Assert
        Assert.Equal(3, receiptNumbers.Count);
        Assert.Equal(3, receiptNumbers.Distinct().Count()); // All unique
    }

    [Fact]
    public void Map_WithZeroOrNegativeAmount_ReturnsNull()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: null,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "0", // Zero amount
            Deposit: "-100", // Negative amount
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.Null(result.Invoice); // Zero amount = no invoice
        Assert.Null(result.Payment); // Negative amount = no payment
    }

    [Fact]
    public void Map_CalculatesDueDate30DaysAfterInvoiceDate()
    {
        // Arrange
        var mapper = new LegacyBillingMapper(_validStudentIds, _referenceMapping, _schoolPrices);
        var record = new LegacyBillingImportRecord(
            StudentId: 1,
            FamilyId: null,
            SchoolId: 10,
            StudentReference: "REF001",
            FinancialCode: null,
            Charge: "1500",
            Deposit: null,
            PayDate: "2026-01-15",
            TshirtMoney1: null,
            TshirtMoneyDate1: null,
            TshirtMoney2: null,
            TshirtMoneyDate2: null);

        // Act
        var result = mapper.Map(record);

        // Assert
        Assert.NotNull(result.Invoice);
        Assert.Equal("2026-01-15", result.Invoice.InvoiceDate);
        Assert.Equal("2026-02-14", result.Invoice.DueDate); // 30 days later
    }
}
