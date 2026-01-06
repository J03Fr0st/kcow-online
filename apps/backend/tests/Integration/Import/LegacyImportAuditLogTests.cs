using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacyImportAuditLogTests
{
    [Fact]
    public void AddValidationErrors_StoresEntriesWithLineInfo()
    {
        var auditLog = new LegacyImportAuditLog();
        var errors = new[]
        {
            new LegacyXmlValidationError("Bad value", 12, 4),
            new LegacyXmlValidationError("Missing element", null, null)
        };

        auditLog.AddValidationErrors("School.xml", errors);

        Assert.Equal(2, auditLog.Entries.Count);
        Assert.Equal("School.xml", auditLog.Entries[0].SourceFile);
        Assert.Equal("Bad value", auditLog.Entries[0].Message);
        Assert.Equal(12, auditLog.Entries[0].LineNumber);
        Assert.Equal(4, auditLog.Entries[0].LinePosition);
        Assert.Equal("Missing element", auditLog.Entries[1].Message);
        Assert.Null(auditLog.Entries[1].LineNumber);
        Assert.Null(auditLog.Entries[1].LinePosition);
    }
}
