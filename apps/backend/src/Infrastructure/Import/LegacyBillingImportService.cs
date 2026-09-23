using Dapper;
using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Globalization;

namespace Kcow.Infrastructure.Import;

/// <summary>
/// Extended import summary that separates invoice and payment counts.
/// </summary>
public sealed record LegacyBillingImportSummary(
    int InvoicesImported,
    int InvoicesSkipped,
    int PaymentsImported,
    int PaymentsSkipped,
    int ErrorCount,
    decimal TotalBalanceImported,
    DateTime CompletedAt)
{
    public int TotalImported => InvoicesImported + PaymentsImported;
    public int TotalSkipped => InvoicesSkipped + PaymentsSkipped;
}

/// <summary>
/// Service for importing legacy billing data (invoices and payments).
/// Parses billing-related fields from Children records and creates
/// Invoice and Payment entities linked to students and families.
/// </summary>
public sealed class LegacyBillingImportService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacyBillingImportService(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        IDbConnectionFactory connectionFactory)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Imports billing data (invoices and payments) from legacy Children records.
    /// Validates foreign keys and calculates balances.
    /// </summary>
    public async Task<LegacyBillingImportSummary> ImportAsync(
        IReadOnlyList<LegacyBillingImportRecord> records,
        string? auditLogPath,
        string? summaryOutputPath,
        bool preview = false,
        CancellationToken cancellationToken = default)
    {
        // Load valid student IDs and reference mappings
        var validStudentIds = await LoadStudentIdsAsync(cancellationToken);
        var referenceToStudentId = await LoadStudentReferenceMappingAsync(cancellationToken);
        var schoolPrices = await LoadSchoolPricesAsync(cancellationToken);
        var nextReceiptNumber = await LoadNextReceiptNumberAsync(cancellationToken);

        var mapper = new LegacyBillingMapper(validStudentIds, referenceToStudentId, schoolPrices, nextReceiptNumber);
        var invoices = new List<Invoice>();
        var payments = new List<Payment>();
        var invoicesSkipped = 0;
        var paymentsSkipped = 0;
        decimal totalBalance = 0;

        foreach (var record in records)
        {
            var mapping = mapper.Map(record);

            // Log warnings
            foreach (var warning in mapping.Warnings)
            {
                _auditLog.AddValidationErrors("billing-import",
                    new[] { new LegacyXmlValidationError(warning, null, null) });
            }

            // Collect invoices
            if (mapping.Invoice != null)
            {
                invoices.Add(mapping.Invoice);
                totalBalance += mapping.Invoice.Amount;
            }
            else if (!string.IsNullOrWhiteSpace(record.Charge))
            {
                invoicesSkipped++;
            }

            // Collect payments
            if (mapping.Payment != null)
            {
                payments.Add(mapping.Payment);
                totalBalance -= mapping.Payment.Amount;
            }

            if (mapping.TshirtPayment1 != null)
            {
                payments.Add(mapping.TshirtPayment1);
                totalBalance -= mapping.TshirtPayment1.Amount;
            }

            if (mapping.TshirtPayment2 != null)
            {
                payments.Add(mapping.TshirtPayment2);
                totalBalance -= mapping.TshirtPayment2.Amount;
            }

            if (mapping.Payment == null && mapping.TshirtPayment1 == null && mapping.TshirtPayment2 == null
                && !string.IsNullOrWhiteSpace(record.Deposit))
            {
                paymentsSkipped++;
            }
        }

        int invoicesCreated = 0;
        int paymentsCreated = 0;

        if (!preview)
        {
            // Import invoices first (to get IDs for linking payments)
            foreach (var invoice in invoices)
            {
                try
                {
                    var id = await _invoiceRepository.CreateAsync(invoice, cancellationToken);
                    invoice.Id = id;
                    invoicesCreated++;

                    // Update any payments that should link to this invoice
                    foreach (var payment in payments.Where(p => p.StudentId == invoice.StudentId && p.InvoiceId == null))
                    {
                        // Link payment to invoice if amounts match (deposit paying off charge)
                        // Only link the first matching payment per invoice
                        if (payment.Amount >= invoice.Amount * 0.9m) // 90% threshold for matching
                        {
                            payment.InvoiceId = id;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _auditLog.AddValidationErrors("billing-import",
                        new[] { new LegacyXmlValidationError($"Failed to create invoice: {ex.Message}", null, null) });
                }
            }

            // Import payments
            foreach (var payment in payments)
            {
                try
                {
                    var id = await _paymentRepository.CreateAsync(payment, cancellationToken);
                    payment.Id = id;
                    paymentsCreated++;
                }
                catch (Exception ex)
                {
                    _auditLog.AddValidationErrors("billing-import",
                        new[] { new LegacyXmlValidationError($"Failed to create payment: {ex.Message}", null, null) });
                }
            }

            // Update invoice statuses based on payments
            await UpdateInvoiceStatusesAsync(cancellationToken);
        }
        else
        {
            invoicesCreated = invoices.Count;
            paymentsCreated = payments.Count;
        }

        var summary = new LegacyBillingImportSummary(
            InvoicesImported: invoicesCreated,
            InvoicesSkipped: invoicesSkipped,
            PaymentsImported: paymentsCreated,
            PaymentsSkipped: paymentsSkipped,
            ErrorCount: _auditLog.Entries.Count,
            TotalBalanceImported: totalBalance,
            CompletedAt: DateTime.UtcNow);

        WriteOutputFiles(auditLogPath, summaryOutputPath, summary);
        return summary;
    }

    private async Task UpdateInvoiceStatusesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();

        // Amounts are stored as TEXT. Sum parsed decimals in .NET to preserve
        // financial precision and avoid SQLite's floating-point SUM conversion.
        const string sql = @"
            SELECT i.id AS Id, i.amount AS InvoiceAmount, p.amount AS PaymentAmount
            FROM invoices i
            LEFT JOIN payments p ON p.invoice_id = i.id
            WHERE i.status = 0";

        var rows = await connection.QueryAsync<InvoicePaymentRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        foreach (var invoiceRows in rows.GroupBy(row => row.Id))
        {
            var invoiceAmount = decimal.Parse(invoiceRows.First().InvoiceAmount, CultureInfo.InvariantCulture);
            var paidAmount = invoiceRows.Sum(row => row.PaymentAmount is null
                ? 0m
                : decimal.Parse(row.PaymentAmount, CultureInfo.InvariantCulture));

            if (paidAmount >= invoiceAmount)
            {
                // Mark as Paid
                await connection.ExecuteAsync(
                    "UPDATE invoices SET status = 1 WHERE id = @Id",
                    new { Id = invoiceRows.Key });
            }
        }
    }

    private sealed class InvoicePaymentRow
    {
        public int Id { get; set; }
        public string InvoiceAmount { get; set; } = string.Empty;
        public string? PaymentAmount { get; set; }
    }

    private void WriteOutputFiles(string? auditLogPath, string? summaryOutputPath,
        LegacyBillingImportSummary summary)
    {
        if (!string.IsNullOrWhiteSpace(auditLogPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(auditLogPath) ?? ".");
            using var writer = File.CreateText(auditLogPath);
            _auditLog.WriteTo(writer);
        }

        if (!string.IsNullOrWhiteSpace(summaryOutputPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(summaryOutputPath) ?? ".");
            var contents = RenderBillingSummary(summary);
            File.WriteAllText(summaryOutputPath, contents);
        }
    }

    private static string RenderBillingSummary(LegacyBillingImportSummary summary)
    {
        return $"""
            Legacy Billing Import Summary
            Completed: {summary.CompletedAt:O}

            Invoices:
              Imported: {summary.InvoicesImported}
              Skipped: {summary.InvoicesSkipped}

            Payments:
              Imported: {summary.PaymentsImported}
              Skipped: {summary.PaymentsSkipped}

            Total Balance Imported: {summary.TotalBalanceImported:C}
            Errors: {summary.ErrorCount}
            """;
    }

    private async Task<HashSet<int>> LoadStudentIdsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        var ids = await connection.QueryAsync<int>("SELECT id FROM students");
        return new HashSet<int>(ids);
    }

    private async Task<Dictionary<string, int>> LoadStudentReferenceMappingAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        var references = await connection.QueryAsync<(string Reference, int Id)>(
            "SELECT reference, id FROM students WHERE reference IS NOT NULL AND reference != ''");
        return references.ToDictionary(r => r.Reference, r => r.Id);
    }

    private async Task<Dictionary<int, decimal>> LoadSchoolPricesAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        var prices = await connection.QueryAsync<(int Id, decimal? Price)>(
            "SELECT id, price FROM schools WHERE price IS NOT NULL AND price > 0");
        return prices
            .Where(p => p.Price.HasValue)
            .ToDictionary(p => p.Id, p => p.Price!.Value);
    }

    private async Task<int> LoadNextReceiptNumberAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        // Legacy receipts use RCP-LEGACY-YYYYMMDD-NNNNN. Continue the sequence
        // across import runs so a repeated payment date cannot reuse a receipt.
        const string sql = "SELECT COALESCE(MAX(CAST(SUBSTR(receipt_number, 21) AS INTEGER)), 0) + 1 " +
            "FROM payments WHERE receipt_number LIKE 'RCP-LEGACY-%'";
        return await connection.QuerySingleAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
