using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

public class ClassGroupDataMapperTests
{
    private readonly ClassGroupDataMapper _mapper = new();

    [Fact]
    public void Map_ValidRecord_ReturnsSuccessWithClassGroup()
    {
        var record = CreateRecord(description: "Monday Group", startTime: "08:00", endTime: "09:00");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Monday Group", result.Data!.Name);
    }

    [Fact]
    public void Map_ImportFlagFalse_ReturnsSkipped()
    {
        var record = CreateRecord(import: false);

        var result = _mapper.Map(record);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Map_MissingName_ReturnsError()
    {
        var record = CreateRecord(classGroup: "", description: null);

        var result = _mapper.Map(record);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Map_InvalidSchoolId_ReturnsError()
    {
        var validSchoolIds = new HashSet<int> { 1, 2, 3 };
        var mapper = new ClassGroupDataMapper(validSchoolIds);
        var record = CreateRecord(schoolId: 99);

        var result = mapper.Map(record);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Map_ConvertsDayIdToDayOfWeek()
    {
        var record = CreateRecord(dayId: "3", startTime: "08:00", endTime: "09:00"); // Wednesday

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(DayOfWeek.Wednesday, result.Data!.DayOfWeek);
    }

    [Fact]
    public void Map_InvalidDayId_DefaultsToMondayWithWarning()
    {
        var record = CreateRecord(dayId: "99", startTime: "08:00", endTime: "09:00");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(DayOfWeek.Monday, result.Data!.DayOfWeek);
        Assert.Contains(result.Warnings, w => w.Field == "DayId");
    }

    [Fact]
    public void Map_ParsesTimeFields()
    {
        var record = CreateRecord(startTime: "14:30", endTime: "15:45");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(new TimeOnly(14, 30), result.Data!.StartTime);
        Assert.Equal(new TimeOnly(15, 45), result.Data.EndTime);
    }

    [Fact]
    public void Map_InvalidStartTime_ReturnsError()
    {
        var record = CreateRecord(startTime: "not-a-time", endTime: "09:00");

        var result = _mapper.Map(record);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Map_EndTimeBeforeStartTime_ReturnsError()
    {
        var record = CreateRecord(startTime: "10:00", endTime: "09:00");

        var result = _mapper.Map(record);

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Map_ValidTruckId_SetsTruckId()
    {
        var validTruckIds = new HashSet<int> { 5 };
        var mapper = new ClassGroupDataMapper(new HashSet<int>(), validTruckIds);
        var record = CreateRecord(dayTruck: "5", startTime: "08:00", endTime: "09:00");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(5, result.Data!.TruckId);
    }

    [Fact]
    public void Map_InvalidTruckId_WarnsAndNullsTruck()
    {
        var validTruckIds = new HashSet<int> { 1, 2 };
        var mapper = new ClassGroupDataMapper(new HashSet<int>(), validTruckIds);
        var record = CreateRecord(dayTruck: "99", startTime: "08:00", endTime: "09:00");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Null(result.Data!.TruckId);
        Assert.Contains(result.Warnings, w => w.Field == "TruckId");
    }

    [Fact]
    public void MapMany_MapsAllRecords_AggregatesResults()
    {
        var records = new[]
        {
            CreateRecord(description: "Group A", startTime: "08:00", endTime: "09:00"),
            CreateRecord(import: false), // skipped
            CreateRecord(description: "Group C", startTime: "10:00", endTime: "11:00")
        };

        var result = _mapper.MapMany(records);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    private static LegacyClassGroupRecord CreateRecord(
        string classGroup = "GRP1",
        string? dayTruck = null,
        string? description = "Test Group",
        string? startTime = "08:00",
        string? endTime = "09:00",
        short schoolId = 1,
        string? dayId = "1",
        bool evaluate = false,
        bool import = true,
        string? sequence = "1")
    {
        return new LegacyClassGroupRecord(
            ClassGroup: classGroup,
            DayTruck: dayTruck,
            Description: description,
            EndTime: endTime,
            SchoolId: schoolId,
            DayId: dayId,
            StartTime: startTime,
            Evaluate: evaluate,
            Note: null,
            Import: import,
            Sequence: sequence,
            GroupMessage: null,
            SendCertificates: null,
            MoneyMessage: null,
            IXL: null);
    }
}
