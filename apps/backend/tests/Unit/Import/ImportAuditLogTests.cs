using Kcow.Domain.Entities;

namespace Kcow.Unit.Tests.Import;

public class ImportAuditLogTests
{
    [Fact]
    public void TotalCreated_SumsAllEntityCounts()
    {
        var log = new ImportAuditLog
        {
            SchoolsCreated = 10,
            ClassGroupsCreated = 20,
            ActivitiesCreated = 5,
            StudentsCreated = 100
        };

        Assert.Equal(135, log.TotalCreated);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var log = new ImportAuditLog();

        Assert.Equal("system", log.RunBy);
        Assert.Equal("InProgress", log.Status);
        Assert.Equal(0, log.SchoolsCreated);
        Assert.Equal(0, log.TotalFailed);
        Assert.Null(log.CompletedAt);
        Assert.Null(log.ExceptionsFilePath);
    }

    [Fact]
    public void Status_CanBeSetToVariousValues()
    {
        var log = new ImportAuditLog();

        log.Status = "Completed";
        Assert.Equal("Completed", log.Status);

        log.Status = "CompletedWithErrors";
        Assert.Equal("CompletedWithErrors", log.Status);

        log.Status = "Failed";
        Assert.Equal("Failed", log.Status);
    }
}
