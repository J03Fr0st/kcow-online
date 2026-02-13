namespace Kcow.Application.Import;

/// <summary>
/// Input record for billing import mapping from legacy Children data.
/// Contains billing-related fields extracted from the 92-field Children record.
/// </summary>
public sealed record LegacyBillingImportRecord(
    int StudentId,
    int? FamilyId,
    int? SchoolId,
    string StudentReference,
    string? FinancialCode,
    string? Charge,
    string? Deposit,
    string? PayDate,
    string? TshirtMoney1,
    string? TshirtMoneyDate1,
    string? TshirtMoney2,
    string? TshirtMoneyDate2);

/// <summary>
/// Result of mapping a legacy billing record to Invoice and Payment entities.
/// </summary>
public sealed record LegacyBillingMappingResult(
    Kcow.Domain.Entities.Invoice? Invoice,
    Kcow.Domain.Entities.Payment? Payment,
    Kcow.Domain.Entities.Payment? TshirtPayment1,
    Kcow.Domain.Entities.Payment? TshirtPayment2,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Maps legacy billing data from Children records to Invoice and Payment entities.
/// Handles the financial fields: FinancialCode, Charge, Deposit, PayDate, and T-shirt money.
/// Falls back to school billing settings (Price) when no Charge is specified.
/// </summary>
public sealed class LegacyBillingMapper
{
    private readonly HashSet<int> _validStudentIds;
    private readonly Dictionary<string, int> _referenceToStudentId;
    private readonly Dictionary<int, decimal> _schoolPrices;
    private int _receiptCounter;

    public LegacyBillingMapper(
        IEnumerable<int> validStudentIds,
        Dictionary<string, int> referenceToStudentId,
        Dictionary<int, decimal>? schoolPrices = null,
        int startingReceiptNumber = 1)
    {
        _validStudentIds = new HashSet<int>(validStudentIds);
        _referenceToStudentId = referenceToStudentId;
        _schoolPrices = schoolPrices ?? new Dictionary<int, decimal>();
        _receiptCounter = startingReceiptNumber;
    }

    /// <summary>
    /// Maps a legacy billing record to Invoice and Payment entities.
    /// Generates invoices from Charge amounts and payments from Deposit amounts.
    /// </summary>
    public LegacyBillingMappingResult Map(LegacyBillingImportRecord record)
    {
        var warnings = new List<string>();

        // Validate student reference
        int studentId;
        if (record.StudentId > 0 && _validStudentIds.Contains(record.StudentId))
        {
            studentId = record.StudentId;
        }
        else if (!string.IsNullOrWhiteSpace(record.StudentReference) &&
                 _referenceToStudentId.TryGetValue(record.StudentReference, out var mappedId))
        {
            studentId = mappedId;
        }
        else
        {
            warnings.Add($"Student not found for billing record. Reference: {record.StudentReference}, ID: {record.StudentId}");
            return new LegacyBillingMappingResult(null, null, null, null, warnings);
        }

        // Parse and create invoice from Charge
        var invoice = CreateInvoiceFromCharge(record, studentId, warnings);

        // Parse and create payment from Deposit
        var (payment, invoiceId) = CreatePaymentFromDeposit(record, studentId, invoice?.Id, warnings);

        // Create t-shirt payments if present
        var tshirtPayment1 = CreateTshirtPayment(record.TshirtMoney1, record.TshirtMoneyDate1, studentId, 1, warnings);
        var tshirtPayment2 = CreateTshirtPayment(record.TshirtMoney2, record.TshirtMoneyDate2, studentId, 2, warnings);

        // Link payment to invoice if both exist
        if (payment != null && invoice != null && invoiceId == null)
        {
            payment.InvoiceId = invoice.Id;
        }

        return new LegacyBillingMappingResult(invoice, payment, tshirtPayment1, tshirtPayment2, warnings);
    }

    private Kcow.Domain.Entities.Invoice? CreateInvoiceFromCharge(
        LegacyBillingImportRecord record,
        int studentId,
        List<string> warnings)
    {
        decimal? amount = null;
        bool usedSchoolPrice = false;

        // Try to get amount from Charge field first
        if (!string.IsNullOrWhiteSpace(record.Charge))
        {
            amount = ParseAmount(record.Charge, warnings, "Charge");
        }

        // Fall back to school billing settings if no Charge or Charge is invalid
        if ((amount == null || amount <= 0) && record.SchoolId.HasValue)
        {
            if (_schoolPrices.TryGetValue(record.SchoolId.Value, out var schoolPrice) && schoolPrice > 0)
            {
                amount = schoolPrice;
                usedSchoolPrice = true;
            }
        }

        if (amount == null || amount <= 0)
        {
            return null;
        }

        // Use PayDate as invoice date, or fall back to Created date
        var invoiceDate = ParseDate(record.PayDate) ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Calculate due date (30 days after invoice date for historical records)
        var dueDate = CalculateDueDate(invoiceDate);

        return new Kcow.Domain.Entities.Invoice
        {
            StudentId = studentId,
            InvoiceDate = invoiceDate,
            Amount = amount.Value,
            DueDate = dueDate,
            Status = 0, // Pending - will be updated based on payments
            Description = BuildInvoiceDescription(record, usedSchoolPrice),
            Notes = string.IsNullOrWhiteSpace(record.FinancialCode)
                ? (usedSchoolPrice ? "Imported from legacy system (using school default rate)" : "Imported from legacy system")
                : $"Financial Code: {record.FinancialCode}. Imported from legacy system",
            CreatedAt = DateTime.UtcNow.ToString("o")
        };
    }

    private (Kcow.Domain.Entities.Payment? Payment, int? InvoiceId) CreatePaymentFromDeposit(
        LegacyBillingImportRecord record,
        int studentId,
        int? invoiceId,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(record.Deposit))
        {
            return (null, invoiceId);
        }

        var amount = ParseAmount(record.Deposit, warnings, "Deposit");
        if (amount == null || amount <= 0)
        {
            return (null, invoiceId);
        }

        var paymentDate = ParseDate(record.PayDate) ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
        var receiptNumber = GenerateLegacyReceiptNumber(paymentDate);

        var payment = new Kcow.Domain.Entities.Payment
        {
            StudentId = studentId,
            InvoiceId = invoiceId > 0 ? invoiceId : null, // Only set if valid
            PaymentDate = paymentDate,
            Amount = amount.Value,
            PaymentMethod = 3, // Other (legacy)
            ReceiptNumber = receiptNumber,
            Notes = "Imported from legacy system (deposit payment)",
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        return (payment, invoiceId);
    }

    private Kcow.Domain.Entities.Payment? CreateTshirtPayment(
        string? tshirtMoney,
        string? tshirtDate,
        int studentId,
        int tshirtNumber,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(tshirtMoney))
        {
            return null;
        }

        var amount = ParseAmount(tshirtMoney, warnings, $"TshirtMoney{tshirtNumber}");
        if (amount == null || amount <= 0)
        {
            return null;
        }

        var paymentDate = ParseDate(tshirtDate) ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
        var receiptNumber = GenerateLegacyReceiptNumber(paymentDate);

        return new Kcow.Domain.Entities.Payment
        {
            StudentId = studentId,
            InvoiceId = null,
            PaymentDate = paymentDate,
            Amount = amount.Value,
            PaymentMethod = 3, // Other (legacy)
            ReceiptNumber = receiptNumber,
            Notes = $"Imported from legacy system (T-shirt payment {tshirtNumber})",
            CreatedAt = DateTime.UtcNow.ToString("o")
        };
    }

    private static string BuildInvoiceDescription(LegacyBillingImportRecord record, bool usedSchoolPrice = false)
    {
        var suffix = usedSchoolPrice ? " (school default rate)" : "";
        return string.IsNullOrWhiteSpace(record.FinancialCode)
            ? $"Legacy tuition fees{suffix}"
            : $"Legacy tuition fees - Code: {record.FinancialCode}{suffix}";
    }

    private static string CalculateDueDate(string invoiceDate)
    {
        if (DateTime.TryParse(invoiceDate, out var date))
        {
            return date.AddDays(30).ToString("yyyy-MM-dd");
        }
        return DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
    }

    private string GenerateLegacyReceiptNumber(string paymentDate)
    {
        var datePart = paymentDate.Replace("-", "");
        var receiptNumber = $"RCP-LEGACY-{datePart}-{_receiptCounter:D5}";
        _receiptCounter++;
        return receiptNumber;
    }

    private static decimal? ParseAmount(string? value, List<string> warnings, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value.Trim();

        // Handle currency symbols and common formats
        cleaned = cleaned.Replace("R", "").Replace("$", "").Replace(",", "").Trim();

        if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }

        warnings.Add($"Could not parse {fieldName} amount: '{value}'");
        return null;
    }

    private static string? ParseDate(string? dateValue)
    {
        return LegacyAttendanceEvaluationXmlParser.ParseDateToIso(dateValue);
    }
}
