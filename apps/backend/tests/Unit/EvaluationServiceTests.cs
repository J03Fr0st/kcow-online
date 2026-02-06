using Kcow.Application.Audit;
using Kcow.Application.Evaluations;
using Kcow.Application.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class EvaluationServiceTests
{
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IAuditService _auditService;
    private readonly Infrastructure.Evaluations.EvaluationService _service;

    public EvaluationServiceTests()
    {
        _evaluationRepository = Substitute.For<IEvaluationRepository>();
        _studentRepository = Substitute.For<IStudentRepository>();
        _activityRepository = Substitute.For<IActivityRepository>();
        _auditService = Substitute.For<IAuditService>();
        _service = new Infrastructure.Evaluations.EvaluationService(
            _evaluationRepository,
            _studentRepository,
            _activityRepository,
            NullLogger<Infrastructure.Evaluations.EvaluationService>.Instance,
            _auditService);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsEvaluationDto()
    {
        // Arrange
        var request = new CreateEvaluationRequest
        {
            StudentId = 1,
            ActivityId = 2,
            EvaluationDate = "2026-02-05",
            Score = 85,
            SpeedMetric = 1.5m,
            AccuracyMetric = 0.92m,
            Notes = "Good performance"
        };

        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.ExistsAsync(2, Arg.Any<CancellationToken>())
            .Returns(true);

        _evaluationRepository.CreateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>())
            .Returns(42);

        _evaluationRepository.GetByIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(new EvaluationWithNames
            {
                Id = 42,
                StudentId = 1,
                StudentFirstName = "John",
                StudentLastName = "Doe",
                ActivityId = 2,
                ActivityName = "Math Activity",
                EvaluationDate = "2026-02-05",
                Score = 85,
                SpeedMetric = 1.5m,
                AccuracyMetric = 0.92m,
                Notes = "Good performance",
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CreateAsync(request, "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal(1, result.StudentId);
        Assert.Equal("John Doe", result.StudentName);
        Assert.Equal(2, result.ActivityId);
        Assert.Equal("Math Activity", result.ActivityName);
        Assert.Equal("2026-02-05", result.EvaluationDate);
        Assert.Equal(85, result.Score);
        Assert.Equal(1.5m, result.SpeedMetric);
        Assert.Equal(0.92m, result.AccuracyMetric);
        Assert.Equal("Good performance", result.Notes);
        await _evaluationRepository.Received(1).CreateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>());
        await _auditService.Received(1).LogChangeAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithMinimalData_ReturnsEvaluationDto()
    {
        // Arrange
        var request = new CreateEvaluationRequest
        {
            StudentId = 1,
            ActivityId = 2,
            EvaluationDate = "2026-02-05"
        };

        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.ExistsAsync(2, Arg.Any<CancellationToken>())
            .Returns(true);

        _evaluationRepository.CreateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>())
            .Returns(42);

        _evaluationRepository.GetByIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(new EvaluationWithNames
            {
                Id = 42,
                StudentId = 1,
                StudentFirstName = "Jane",
                StudentLastName = "Smith",
                ActivityId = 2,
                ActivityName = "Reading Activity",
                EvaluationDate = "2026-02-05",
                Score = null,
                SpeedMetric = null,
                AccuracyMetric = null,
                Notes = null,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CreateAsync(request, "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Null(result.Score);
        Assert.Null(result.SpeedMetric);
        Assert.Null(result.AccuracyMetric);
        Assert.Null(result.Notes);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingStudent_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateEvaluationRequest
        {
            StudentId = 999,
            ActivityId = 2,
            EvaluationDate = "2026-02-05"
        };

        _studentRepository.ExistsAsync(999, Arg.Any<CancellationToken>())
            .Returns(false);
        _activityRepository.ExistsAsync(2, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(request, "test@example.com"));
        Assert.Contains("Student with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingActivity_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateEvaluationRequest
        {
            StudentId = 1,
            ActivityId = 999,
            EvaluationDate = "2026-02-05"
        };

        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.ExistsAsync(999, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(request, "test@example.com"));
        Assert.Contains("Activity with ID 999 does not exist", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsDto()
    {
        // Arrange
        _evaluationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new EvaluationWithNames
            {
                Id = 1,
                StudentId = 10,
                StudentFirstName = "Jane",
                StudentLastName = "Smith",
                ActivityId = 5,
                ActivityName = "Science Activity",
                EvaluationDate = "2026-01-15",
                Score = 90,
                SpeedMetric = 2.0m,
                AccuracyMetric = 0.95m,
                Notes = "Excellent work",
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(90, result.Score);
        Assert.Equal("Jane Smith", result.StudentName);
        Assert.Equal("Science Activity", result.ActivityName);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _evaluationRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((EvaluationWithNames?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_ReturnsUpdatedDto()
    {
        // Arrange
        _evaluationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(
                new EvaluationWithNames
                {
                    Id = 1,
                    StudentId = 10,
                    ActivityId = 5,
                    EvaluationDate = "2026-01-15",
                    Score = 75,
                    SpeedMetric = 1.0m,
                    AccuracyMetric = 0.80m,
                    Notes = "Needs improvement",
                    CreatedAt = DateTime.UtcNow
                },
                new EvaluationWithNames
                {
                    Id = 1,
                    StudentId = 10,
                    StudentFirstName = "Jane",
                    StudentLastName = "Smith",
                    ActivityId = 5,
                    ActivityName = "Math Activity",
                    EvaluationDate = "2026-01-20",
                    Score = 85,
                    SpeedMetric = 1.5m,
                    AccuracyMetric = 0.90m,
                    Notes = "Improved",
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                });

        _evaluationRepository.UpdateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateEvaluationRequest
        {
            EvaluationDate = "2026-01-20",
            Score = 85,
            SpeedMetric = 1.5m,
            AccuracyMetric = 0.90m,
            Notes = "Improved"
        };

        // Act
        var result = await _service.UpdateAsync(1, request, "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(85, result.Score);
        Assert.Equal("Improved", result.Notes);
        Assert.NotNull(result.ModifiedAt);
        await _evaluationRepository.Received(1).UpdateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>());
        await _auditService.Received(1).LogChangesAsync(
            "Evaluation",
            1,
            Arg.Is<Dictionary<string, (string?, string?)>>(d =>
                d.ContainsKey("EvaluationDate") &&
                d.ContainsKey("Score") &&
                d.ContainsKey("SpeedMetric") &&
                d.ContainsKey("AccuracyMetric") &&
                d.ContainsKey("Notes")),
            "test@example.com",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _evaluationRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((EvaluationWithNames?)null);

        var request = new UpdateEvaluationRequest
        {
            EvaluationDate = "2026-01-20",
            Score = 85
        };

        // Act
        var result = await _service.UpdateAsync(999, request, "test@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        _evaluationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new EvaluationWithNames
            {
                Id = 1,
                StudentId = 10,
                ActivityId = 5,
                EvaluationDate = "2026-01-15",
                Score = 75,
                CreatedAt = DateTime.UtcNow
            });

        _evaluationRepository.DeleteAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.DeleteAsync(1, "test@example.com");

        // Assert
        Assert.True(result);
        await _evaluationRepository.Received(1).DeleteAsync(1, Arg.Any<CancellationToken>());
        await _auditService.Received(1).LogChangeAsync(
            "Evaluation",
            1,
            "Deleted",
            Arg.Any<string>(),
            Arg.Any<string?>(),
            "test@example.com",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
    {
        // Arrange
        _evaluationRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((EvaluationWithNames?)null);

        // Act
        var result = await _service.DeleteAsync(999, "test@example.com");

        // Assert
        Assert.False(result);
        await _evaluationRepository.DidNotReceive().DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _auditService.DidNotReceive().LogChangeAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfEvaluations()
    {
        // Arrange
        var records = new List<EvaluationWithNames>
        {
            new EvaluationWithNames
            {
                Id = 1,
                StudentId = 10,
                StudentFirstName = "John",
                StudentLastName = "Doe",
                ActivityId = 5,
                ActivityName = "Activity A",
                EvaluationDate = "2026-01-15",
                Score = 90,
                SpeedMetric = 2.0m,
                AccuracyMetric = 0.95m,
                CreatedAt = DateTime.UtcNow
            },
            new EvaluationWithNames
            {
                Id = 2,
                StudentId = 11,
                StudentFirstName = "Jane",
                StudentLastName = "Smith",
                ActivityId = 6,
                ActivityName = "Activity B",
                EvaluationDate = "2026-01-16",
                Score = 85,
                SpeedMetric = 1.8m,
                AccuracyMetric = 0.90m,
                CreatedAt = DateTime.UtcNow
            }
        };

        _evaluationRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(records);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("John Doe", result[0].StudentName);
        Assert.Equal("Jane Smith", result[1].StudentName);
    }

    [Fact]
    public async Task GetByStudentIdAsync_ReturnsStudentEvaluationHistory()
    {
        // Arrange
        var records = new List<EvaluationWithNames>
        {
            new EvaluationWithNames
            {
                Id = 1,
                StudentId = 10,
                StudentFirstName = "John",
                StudentLastName = "Doe",
                ActivityId = 5,
                ActivityName = "Math Activity",
                EvaluationDate = "2026-01-16",
                Score = 90,
                CreatedAt = DateTime.UtcNow
            },
            new EvaluationWithNames
            {
                Id = 2,
                StudentId = 10,
                StudentFirstName = "John",
                StudentLastName = "Doe",
                ActivityId = 6,
                ActivityName = "Reading Activity",
                EvaluationDate = "2026-01-15",
                Score = 85,
                CreatedAt = DateTime.UtcNow
            }
        };

        _evaluationRepository.GetByStudentIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(records);

        // Act
        var result = await _service.GetByStudentIdAsync(10);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(10, r.StudentId));
        Assert.Equal("Math Activity", result[0].ActivityName);
        Assert.Equal("Reading Activity", result[1].ActivityName);
    }

    [Fact]
    public async Task UpdateAsync_WithNoChanges_DoesNotLogAudit()
    {
        // Arrange
        var evaluationDate = "2026-01-15";
        var score = 75;
        var speedMetric = 1.0m;
        var accuracyMetric = 0.80m;
        var notes = "Same notes";

        _evaluationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(
                new EvaluationWithNames
                {
                    Id = 1,
                    StudentId = 10,
                    ActivityId = 5,
                    EvaluationDate = evaluationDate,
                    Score = score,
                    SpeedMetric = speedMetric,
                    AccuracyMetric = accuracyMetric,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                },
                new EvaluationWithNames
                {
                    Id = 1,
                    StudentId = 10,
                    StudentFirstName = "Jane",
                    StudentLastName = "Smith",
                    ActivityId = 5,
                    ActivityName = "Activity A",
                    EvaluationDate = evaluationDate,
                    Score = score,
                    SpeedMetric = speedMetric,
                    AccuracyMetric = accuracyMetric,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                });

        _evaluationRepository.UpdateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateEvaluationRequest
        {
            EvaluationDate = evaluationDate,
            Score = score,
            SpeedMetric = speedMetric,
            AccuracyMetric = accuracyMetric,
            Notes = notes
        };

        // Act
        var result = await _service.UpdateAsync(1, request, "test@example.com");

        // Assert
        Assert.NotNull(result);
        await _auditService.DidNotReceive().LogChangesAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<Dictionary<string, (string?, string?)>>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    public async Task CreateAsync_AcceptsValidScores(int score)
    {
        // Arrange
        var request = new CreateEvaluationRequest
        {
            StudentId = 1,
            ActivityId = 2,
            EvaluationDate = "2026-02-05",
            Score = score
        };

        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.ExistsAsync(2, Arg.Any<CancellationToken>())
            .Returns(true);

        _evaluationRepository.CreateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>())
            .Returns(1);

        _evaluationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new EvaluationWithNames
            {
                Id = 1,
                StudentId = 1,
                StudentFirstName = "Test",
                StudentLastName = "Student",
                ActivityId = 2,
                ActivityName = "Test Activity",
                EvaluationDate = "2026-02-05",
                Score = score,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CreateAsync(request, "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(score, result.Score);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public async Task CreateAsync_AcceptsValidAccuracyMetrics(decimal accuracy)
    {
        // Arrange
        var request = new CreateEvaluationRequest
        {
            StudentId = 1,
            ActivityId = 2,
            EvaluationDate = "2026-02-05",
            AccuracyMetric = accuracy
        };

        _studentRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);
        _activityRepository.ExistsAsync(2, Arg.Any<CancellationToken>())
            .Returns(true);

        _evaluationRepository.CreateAsync(Arg.Any<Kcow.Domain.Entities.Evaluation>(), Arg.Any<CancellationToken>())
            .Returns(1);

        _evaluationRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new EvaluationWithNames
            {
                Id = 1,
                StudentId = 1,
                StudentFirstName = "Test",
                StudentLastName = "Student",
                ActivityId = 2,
                ActivityName = "Test Activity",
                EvaluationDate = "2026-02-05",
                AccuracyMetric = accuracy,
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.CreateAsync(request, "test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accuracy, result.AccuracyMetric);
    }
}
