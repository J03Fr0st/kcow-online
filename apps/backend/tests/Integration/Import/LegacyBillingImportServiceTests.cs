using Dapper;
using Kcow.Application.Billing;
using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Infrastructure;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Import;
using Microsoft.Extensions.DependencyInjection;

namespace Kcow.Integration.Tests.Import;

public class LegacyBillingImportServiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized;
    private static readonly object _lock = new();

    public LegacyBillingImportServiceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ImportAsync_ValidRecord_InsertsInvoiceAndPayment()
    {
        EnsureDatabaseInitialized();

        var (studentId, schoolId, testRef) = await CreateTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var uniqueDate = $"2024-11-{Random.Shared.Next(10, 28):D2}";
        var records = new List<LegacyBillingImportRecord>
        {
            new(
                StudentId: studentId,
                FamilyId: null,
                SchoolId: schoolId,
                StudentReference: testRef,
                FinancialCode: "FC-TEST-001",
                Charge: "1500",
                Deposit: "500",
                PayDate: uniqueDate,
                TshirtMoney1: null,
                TshirtMoneyDate1: null,
                TshirtMoney2: null,
                TshirtMoneyDate2: null)
        };

        var summary = await service.ImportAsync(records, null, null);

        Assert.Equal(1, summary.InvoicesImported);
        Assert.Equal(1, summary.PaymentsImported);
        Assert.Equal(1000m, summary.TotalBalanceImported); // 1500 - 500

        // Verify data was persisted
        var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoices = await invoiceRepo.GetByStudentIdAsync(studentId);
        Assert.Contains(invoices, i => i.Amount == 1500m && i.Description.Contains("FC-TEST-001"));
    }

    [Fact]
    public async Task ImportAsync_InvalidStudentId_SkipsRecord()
    {
        EnsureDatabaseInitialized();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var records = new List<LegacyBillingImportRecord>
        {
            new(
                StudentId: 999999,
                FamilyId: null,
                SchoolId: null,
                StudentReference: "INVALID-REF",
                FinancialCode: null,
                Charge: "1000",
                Deposit: "500",
                PayDate: "2024-03-15",
                TshirtMoney1: null,
                TshirtMoneyDate1: null,
                TshirtMoney2: null,
                TshirtMoneyDate2: null)
        };

        var summary = await service.ImportAsync(records, null, null);

        Assert.Equal(0, summary.InvoicesImported);
        Assert.Equal(0, summary.PaymentsImported);
        Assert.True(summary.ErrorCount > 0);
    }

    [Fact]
    public async Task ImportAsync_PreviewMode_DoesNotInsert()
    {
        EnsureDatabaseInitialized();

        var (studentId, schoolId, testRef) = await CreateTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var uniqueDate = $"2099-0{Random.Shared.Next(1, 9)}-{Random.Shared.Next(10, 28):D2}";
        var records = new List<LegacyBillingImportRecord>
        {
            new(
                StudentId: studentId,
                FamilyId: null,
                SchoolId: schoolId,
                StudentReference: testRef,
                FinancialCode: "PREVIEW",
                Charge: "9999",
                Deposit: "1111",
                PayDate: uniqueDate,
                TshirtMoney1: null,
                TshirtMoneyDate1: null,
                TshirtMoney2: null,
                TshirtMoneyDate2: null)
        };

        var summary = await service.ImportAsync(records, null, null, preview: true);

        Assert.Equal(1, summary.InvoicesImported); // Counted but not inserted
        Assert.Equal(1, summary.PaymentsImported);

        // Verify data was NOT persisted
        var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoices = await invoiceRepo.GetByStudentIdAsync(studentId);
        Assert.DoesNotContain(invoices, i => i.Description.Contains("PREVIEW"));
    }

    [Fact]
    public async Task ImportAsync_WithSchoolPriceFallback_UsesSchoolPrice()
    {
        EnsureDatabaseInitialized();

        var (studentId, schoolId, testRef) = await CreateTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var uniqueDate = $"2024-12-{Random.Shared.Next(10, 28):D2}";
        var records = new List<LegacyBillingImportRecord>
        {
            new(
                StudentId: studentId,
                FamilyId: null,
                SchoolId: schoolId,
                StudentReference: testRef,
                FinancialCode: null,
                Charge: null, // No charge - should use school price
                Deposit: "100",
                PayDate: uniqueDate,
                TshirtMoney1: null,
                TshirtMoneyDate1: null,
                TshirtMoney2: null,
                TshirtMoneyDate2: null)
        };

        var summary = await service.ImportAsync(records, null, null);

        Assert.Equal(1, summary.InvoicesImported);

        // Verify invoice was created with school price
        var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoices = await invoiceRepo.GetByStudentIdAsync(studentId);
        Assert.Contains(invoices, i => i.Description.Contains("school default rate") && i.Amount == 450m);
    }

    [Fact]
    public async Task ImportAsync_BillingSummary_ShowsCorrectBalance()
    {
        EnsureDatabaseInitialized();

        var (studentId, schoolId, testRef) = await CreateTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
        var service = CreateImportService(scope);

        var uniqueDate = $"2024-10-{Random.Shared.Next(10, 28):D2}";
        var records = new List<LegacyBillingImportRecord>
        {
            new(studentId, null, schoolId, testRef, "BAL-TEST", "2000", "1500", uniqueDate, null, null, null, null)
        };

        await service.ImportAsync(records, null, null);

        // Get billing summary
        var summary = await billingService.GetBillingSummaryAsync(studentId);

        Assert.Equal(500m, summary.CurrentBalance); // 2000 - 1500
        Assert.True(summary.TotalInvoiced >= 2000m);
        Assert.True(summary.TotalPaid >= 1500m);
    }

    private LegacyBillingImportService CreateImportService(IServiceScope scope)
    {
        return new LegacyBillingImportService(
            scope.ServiceProvider.GetRequiredService<IInvoiceRepository>(),
            scope.ServiceProvider.GetRequiredService<IPaymentRepository>(),
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>());
    }

    private async Task<(int studentId, int schoolId, string testRef)> CreateTestDataAsync()
    {
        var testSuffix = Guid.NewGuid().ToString("N")[..8];
        var testRef = $"BLG{testSuffix}";

        using var scope = _factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        using var conn = connectionFactory.Create();

        // Create a test school with a price
        var schoolId = conn.QuerySingle<int>(@"
            INSERT INTO schools (name, short_name, price, print_invoice, import_flag, is_active, created_at)
            VALUES (@Name, @ShortName, @Price, @Print, @Import, @IsActive, @CreatedAt)
            RETURNING id",
            new { Name = $"Billing Test {testSuffix}", ShortName = $"BT{testSuffix[..2]}",
                  Price = 450m, Print = false, Import = false, IsActive = true,
                  CreatedAt = DateTime.UtcNow.ToString("o") });

        // Create a test class group
        var classGroupId = conn.QuerySingle<int>(@"
            INSERT INTO class_groups (name, description, school_id, day_of_week, start_time, end_time, is_active, created_at)
            VALUES (@Name, @Description, @SchoolId, @DayOfWeek, @StartTime, @EndTime, @IsActive, @CreatedAt)
            RETURNING id",
            new { Name = $"BLG{testSuffix}", Description = "Billing Test Group", SchoolId = schoolId,
                  DayOfWeek = (int)DayOfWeek.Monday, StartTime = "08:00", EndTime = "09:00",
                  IsActive = true, CreatedAt = DateTime.UtcNow.ToString("o") });

        // Create a test student
        var studentId = conn.QuerySingle<int>(@"
            INSERT INTO students (reference, first_name, last_name, class_group_id, is_active, created_at)
            VALUES (@Reference, @FirstName, @LastName, @ClassGroupId, @IsActive, @CreatedAt)
            RETURNING id",
            new { Reference = testRef, FirstName = "Billing", LastName = "TestStudent",
                  ClassGroupId = classGroupId, IsActive = true, CreatedAt = DateTime.UtcNow.ToString("o") });

        return (studentId, schoolId, testRef);
    }

    private void EnsureDatabaseInitialized()
    {
        if (_databaseInitialized) return;

        lock (_lock)
        {
            if (_databaseInitialized) return;

            using var scope = _factory.Services.CreateScope();
            scope.ServiceProvider.InitializeDatabaseAsync().GetAwaiter().GetResult();
            _databaseInitialized = true;
        }
    }
}
