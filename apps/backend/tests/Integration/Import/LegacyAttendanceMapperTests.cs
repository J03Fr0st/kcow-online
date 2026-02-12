using Kcow.Application.Import;
using Kcow.Domain.Entities;

namespace Kcow.Integration.Tests.Import;

public class LegacyAttendanceMapperTests
{
    private readonly LegacyAttendanceMapper _mapper;

    public LegacyAttendanceMapperTests()
    {
        _mapper = new LegacyAttendanceMapper(
            validStudentIds: new[] { 1, 2, 3 },
            validClassGroupIds: new[] { 10, 20, 30 });
    }

    [Fact]
    public void Map_ValidRecord_ReturnsAttendance()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2024-01-15",
            Status: "Present",
            Notes: "On time",
            OriginalCreatedAt: new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc),
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.Empty(result.Warnings);
        Assert.Equal(1, result.Attendance!.StudentId);
        Assert.Equal(10, result.Attendance.ClassGroupId);
        Assert.Equal("2024-01-15", result.Attendance.SessionDate);
        Assert.Equal(AttendanceStatus.Present, result.Attendance.Status);
        Assert.Equal("On time", result.Attendance.Notes);
        Assert.Equal(new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc), result.Attendance.CreatedAt);
    }

    [Fact]
    public void Map_InvalidStudentId_ReturnsNull()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 999,
            ClassGroupId: 10,
            SessionDate: "2024-01-15",
            Status: "Present",
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.Null(result.Attendance);
        Assert.Contains(result.Warnings, w => w.Contains("Student ID 999 not found"));
    }

    [Fact]
    public void Map_InvalidClassGroupId_ReturnsNull()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 999,
            SessionDate: "2024-01-15",
            Status: "Present",
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.Null(result.Attendance);
        Assert.Contains(result.Warnings, w => w.Contains("Class Group ID 999 not found"));
    }

    [Fact]
    public void Map_InvalidDate_ReturnsNull()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "not-a-date",
            Status: "Present",
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.Null(result.Attendance);
        Assert.Contains(result.Warnings, w => w.Contains("Invalid session date"));
    }

    [Theory]
    [InlineData("Present", AttendanceStatus.Present)]
    [InlineData("present", AttendanceStatus.Present)]
    [InlineData("P", AttendanceStatus.Present)]
    [InlineData("p", AttendanceStatus.Present)]
    [InlineData("1", AttendanceStatus.Present)]
    [InlineData("teenwoordig", AttendanceStatus.Present)]
    [InlineData("Absent", AttendanceStatus.Absent)]
    [InlineData("absent", AttendanceStatus.Absent)]
    [InlineData("A", AttendanceStatus.Absent)]
    [InlineData("a", AttendanceStatus.Absent)]
    [InlineData("0", AttendanceStatus.Absent)]
    [InlineData("afwesig", AttendanceStatus.Absent)]
    [InlineData("Late", AttendanceStatus.Late)]
    [InlineData("late", AttendanceStatus.Late)]
    [InlineData("L", AttendanceStatus.Late)]
    [InlineData("l", AttendanceStatus.Late)]
    [InlineData("laat", AttendanceStatus.Late)]
    public void Map_StatusVariations_MapsCorrectly(string statusInput, AttendanceStatus expected)
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2024-01-15",
            Status: statusInput,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.Equal(expected, result.Attendance!.Status);
    }

    [Fact]
    public void Map_UnknownStatus_DefaultsToPresent()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2024-01-15",
            Status: "unknown-value",
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.Equal(AttendanceStatus.Present, result.Attendance!.Status);
        Assert.Contains(result.Warnings, w => w.Contains("Unknown attendance status"));
    }

    [Fact]
    public void Map_NullStatus_DefaultsToPresent()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2024-01-15",
            Status: null!,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.Equal(AttendanceStatus.Present, result.Attendance!.Status);
    }

    [Fact]
    public void Map_PreservesHistoricalTimestamps()
    {
        var created = new DateTime(2023, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var modified = new DateTime(2023, 6, 20, 14, 30, 0, DateTimeKind.Utc);

        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2023-06-15",
            Status: "Present",
            Notes: null,
            OriginalCreatedAt: created,
            OriginalModifiedAt: modified);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.Equal(created, result.Attendance!.CreatedAt);
        Assert.Equal(modified, result.Attendance.ModifiedAt);
    }

    [Fact]
    public void Map_NullTimestamps_UsesUtcNow()
    {
        var before = DateTime.UtcNow;

        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2024-01-15",
            Status: "Present",
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.True(result.Attendance!.CreatedAt >= before);
        Assert.Null(result.Attendance.ModifiedAt);
    }

    [Fact]
    public void Map_AccessDateTimeFormat_ParsesCorrectly()
    {
        var record = new LegacyAttendanceImportRecord(
            StudentId: 1,
            ClassGroupId: 10,
            SessionDate: "2024-01-15T00:00:00",
            Status: "Present",
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Attendance);
        Assert.Equal("2024-01-15", result.Attendance!.SessionDate);
    }
}
