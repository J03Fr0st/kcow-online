using Kcow.Application.Attendance;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class AttendanceServiceTests
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly Infrastructure.Attendance.AttendanceService _service;

    public AttendanceServiceTests()
    {
        _attendanceRepository = Substitute.For<IAttendanceRepository>();
        _service = new Infrastructure.Attendance.AttendanceService(
            _attendanceRepository,
            NullLogger<Infrastructure.Attendance.AttendanceService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsAttendanceDto()
    {
        // Arrange
        var request = new CreateAttendanceRequest
        {
            StudentId = 1,
            ClassGroupId = 2,
            SessionDate = "2026-02-05",
            Status = "Present",
            Notes = "On time"
        };

        _attendanceRepository.CreateAsync(Arg.Any<Kcow.Domain.Entities.Attendance>(), Arg.Any<CancellationToken>())
            .Returns(42);

        _attendanceRepository.GetByIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(new AttendanceWithNames
            {
                Id = 42,
                StudentId = 1,
                StudentFirstName = "John",
                StudentLastName = "Doe",
                ClassGroupId = 2,
                ClassGroupName = "Group A",
                SessionDate = "2026-02-05",
                Status = (int)AttendanceStatus.Present,
                Notes = "On time",
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal(1, result.StudentId);
        Assert.Equal("John Doe", result.StudentName);
        Assert.Equal(2, result.ClassGroupId);
        Assert.Equal("Group A", result.ClassGroupName);
        Assert.Equal("2026-02-05", result.SessionDate);
        Assert.Equal("Present", result.Status);
        Assert.Equal("On time", result.Notes);
        await _attendanceRepository.Received(1).CreateAsync(Arg.Any<Kcow.Domain.Entities.Attendance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateAttendanceRequest
        {
            StudentId = 1,
            ClassGroupId = 2,
            SessionDate = "2026-02-05",
            Status = "InvalidStatus"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        Assert.Contains("Invalid attendance status", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsDto()
    {
        // Arrange
        _attendanceRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new AttendanceWithNames
            {
                Id = 1,
                StudentId = 10,
                StudentFirstName = "Jane",
                StudentLastName = "Smith",
                ClassGroupId = 5,
                ClassGroupName = "Group B",
                SessionDate = "2026-01-15",
                Status = (int)AttendanceStatus.Late,
                Notes = "Arrived late",
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Late", result.Status);
        Assert.Equal("Jane Smith", result.StudentName);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _attendanceRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((AttendanceWithNames?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_ReturnsUpdatedDto()
    {
        // Arrange
        _attendanceRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(
                new AttendanceWithNames
                {
                    Id = 1, StudentId = 10, ClassGroupId = 5,
                    SessionDate = "2026-01-15", Status = (int)AttendanceStatus.Present,
                    CreatedAt = DateTime.UtcNow
                },
                new AttendanceWithNames
                {
                    Id = 1, StudentId = 10, StudentFirstName = "Jane", StudentLastName = "Smith",
                    ClassGroupId = 5, ClassGroupName = "Group B",
                    SessionDate = "2026-01-15", Status = (int)AttendanceStatus.Absent,
                    Notes = "Corrected",
                    CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow
                });

        _attendanceRepository.UpdateAsync(Arg.Any<Kcow.Domain.Entities.Attendance>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateAttendanceRequest
        {
            Status = "Absent",
            Notes = "Corrected"
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Absent", result.Status);
        Assert.Equal("Corrected", result.Notes);
        Assert.NotNull(result.ModifiedAt);
        await _attendanceRepository.Received(1).UpdateAsync(Arg.Any<Kcow.Domain.Entities.Attendance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _attendanceRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((AttendanceWithNames?)null);

        var request = new UpdateAttendanceRequest
        {
            Status = "Present"
        };

        // Act
        var result = await _service.UpdateAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        _attendanceRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new AttendanceWithNames
            {
                Id = 1, StudentId = 10, ClassGroupId = 5,
                SessionDate = "2026-01-15", Status = (int)AttendanceStatus.Present,
                CreatedAt = DateTime.UtcNow
            });

        var request = new UpdateAttendanceRequest
        {
            Status = "BadStatus"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(1, request));
        Assert.Contains("Invalid attendance status", ex.Message);
    }

    [Fact]
    public async Task GetFilteredAsync_WithStudentFilter_ReturnsFilteredList()
    {
        // Arrange
        var records = new List<AttendanceWithNames>
        {
            new AttendanceWithNames
            {
                Id = 1, StudentId = 10, StudentFirstName = "John", StudentLastName = "Doe",
                ClassGroupId = 5, ClassGroupName = "Group A",
                SessionDate = "2026-01-15", Status = (int)AttendanceStatus.Present,
                CreatedAt = DateTime.UtcNow
            }
        };

        _attendanceRepository.GetFilteredAsync(10, null, null, null, Arg.Any<CancellationToken>())
            .Returns(records);

        // Act
        var result = await _service.GetFilteredAsync(studentId: 10);

        // Assert
        Assert.Single(result);
        Assert.Equal(10, result[0].StudentId);
        Assert.Equal("Present", result[0].Status);
    }

    [Fact]
    public async Task GetByStudentIdAsync_ReturnsStudentAttendanceHistory()
    {
        // Arrange
        var records = new List<AttendanceWithNames>
        {
            new AttendanceWithNames
            {
                Id = 1, StudentId = 10, StudentFirstName = "John", StudentLastName = "Doe",
                ClassGroupId = 5, ClassGroupName = "Group A",
                SessionDate = "2026-01-16", Status = (int)AttendanceStatus.Present,
                CreatedAt = DateTime.UtcNow
            },
            new AttendanceWithNames
            {
                Id = 2, StudentId = 10, StudentFirstName = "John", StudentLastName = "Doe",
                ClassGroupId = 5, ClassGroupName = "Group A",
                SessionDate = "2026-01-15", Status = (int)AttendanceStatus.Absent,
                CreatedAt = DateTime.UtcNow
            }
        };

        _attendanceRepository.GetByStudentIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(records);

        // Act
        var result = await _service.GetByStudentIdAsync(10);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(10, r.StudentId));
    }

    [Theory]
    [InlineData("Present")]
    [InlineData("Absent")]
    [InlineData("Late")]
    [InlineData("present")]
    [InlineData("ABSENT")]
    [InlineData("late")]
    public async Task CreateAsync_AcceptsValidStatusesCaseInsensitive(string status)
    {
        // Arrange
        var request = new CreateAttendanceRequest
        {
            StudentId = 1,
            ClassGroupId = 2,
            SessionDate = "2026-02-05",
            Status = status
        };

        _attendanceRepository.CreateAsync(Arg.Any<Kcow.Domain.Entities.Attendance>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var expectedStatus = char.ToUpper(status[0]) + status.Substring(1).ToLower();
        _attendanceRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new AttendanceWithNames
            {
                Id = 1, StudentId = 1, ClassGroupId = 2,
                SessionDate = "2026-02-05",
                Status = (int)Enum.Parse<AttendanceStatus>(status, ignoreCase: true),
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        await _attendanceRepository.Received(1).CreateAsync(Arg.Any<Kcow.Domain.Entities.Attendance>(), Arg.Any<CancellationToken>());
    }
}
