using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

public class ActivityDataMapperTests
{
    private readonly ActivityDataMapper _mapper = new();

    [Fact]
    public void Map_ValidRecord_ReturnsSuccessWithActivity()
    {
        var record = new LegacyActivityRecord(1, "PROG1", "Activity Name", "Focus", "FolderA", "Grade 1", null);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal("PROG1", result.Data.Code);
        Assert.Equal("Activity Name", result.Data.Name);
        Assert.Equal("Focus", result.Data.Description);
        Assert.Equal("FolderA", result.Data.Folder);
        Assert.Equal("Grade 1", result.Data.GradeLevel);
    }

    [Fact]
    public void Map_LongFields_TruncatesWithWarning()
    {
        var longValue = new string('A', 300);
        var record = new LegacyActivityRecord(2, longValue, "Name", null, null, null, null);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(255, result.Data!.Code!.Length);
        Assert.Contains(result.Warnings, w => w.Field == "Code");
    }

    [Fact]
    public void Map_LargeIcon_WarnsAboutSize()
    {
        var largeIcon = new string('X', 200_000);
        var record = new LegacyActivityRecord(3, "P", "N", null, null, null, largeIcon);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Contains(result.Warnings, w => w.Field == "Icon" && w.Message.Contains("Large icon"));
    }

    [Fact]
    public void Map_NonStandardIconPrefix_WarnsAboutOleWrapper()
    {
        var icon = "ABCDEF123456"; // not /9j/ or iVBOR
        var record = new LegacyActivityRecord(4, "P", "N", null, null, null, icon);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Contains(result.Warnings, w => w.Field == "Icon" && w.Message.Contains("OLE"));
    }

    [Fact]
    public void Map_NullIcon_NoWarning()
    {
        var record = new LegacyActivityRecord(5, "P", "N", null, null, null, null);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.False(result.HasWarnings);
    }

    [Fact]
    public void MapMany_MapsAllRecords()
    {
        var records = new[]
        {
            new LegacyActivityRecord(1, "A", "Act1", null, null, null, null),
            new LegacyActivityRecord(2, "B", "Act2", null, null, null, null)
        };

        var result = _mapper.MapMany(records);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }
}
