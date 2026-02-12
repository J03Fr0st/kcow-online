using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacyEvaluationMapperTests
{
    private readonly LegacyEvaluationMapper _mapper;

    public LegacyEvaluationMapperTests()
    {
        _mapper = new LegacyEvaluationMapper(
            validStudentIds: new[] { 1, 2, 3 },
            validActivityIds: new[] { 10, 20, 30 });
    }

    [Fact]
    public void Map_ValidRecord_ReturnsEvaluation()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: 85,
            SpeedMetric: 7.5m,
            AccuracyMetric: 92.3m,
            Notes: "Good progress",
            OriginalCreatedAt: new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc),
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Empty(result.Warnings);
        Assert.Equal(1, result.Evaluation!.StudentId);
        Assert.Equal(10, result.Evaluation.ActivityId);
        Assert.Equal("2024-01-15", result.Evaluation.EvaluationDate);
        Assert.Equal(85, result.Evaluation.Score);
        Assert.Equal(7.5m, result.Evaluation.SpeedMetric);
        Assert.Equal(92.3m, result.Evaluation.AccuracyMetric);
        Assert.Equal("Good progress", result.Evaluation.Notes);
    }

    [Fact]
    public void Map_InvalidStudentId_ReturnsNull()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 999,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: 85,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.Null(result.Evaluation);
        Assert.Contains(result.Warnings, w => w.Contains("Student ID 999 not found"));
    }

    [Fact]
    public void Map_InvalidActivityId_ReturnsNull()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 999,
            EvaluationDate: "2024-01-15",
            Score: 85,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.Null(result.Evaluation);
        Assert.Contains(result.Warnings, w => w.Contains("Activity ID 999 not found"));
    }

    [Fact]
    public void Map_InvalidDate_ReturnsNull()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "invalid",
            Score: 85,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.Null(result.Evaluation);
        Assert.Contains(result.Warnings, w => w.Contains("Invalid evaluation date"));
    }

    [Fact]
    public void Map_ScoreOutOfRange_ClampedAndWarns()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: 150,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Equal(100, result.Evaluation!.Score);
        Assert.Contains(result.Warnings, w => w.Contains("Score 150 out of range"));
    }

    [Fact]
    public void Map_NegativeScore_ClampedToZero()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: -5,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Equal(0, result.Evaluation!.Score);
    }

    [Fact]
    public void Map_NegativeSpeedMetric_ClampedToZero()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: 50,
            SpeedMetric: -3.5m,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Equal(0m, result.Evaluation!.SpeedMetric);
        Assert.Contains(result.Warnings, w => w.Contains("Negative speed metric"));
    }

    [Fact]
    public void Map_AccuracyOutOfRange_ClampedAndWarns()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: 50,
            SpeedMetric: 5.0m,
            AccuracyMetric: 110.5m,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Equal(100m, result.Evaluation!.AccuracyMetric);
        Assert.Contains(result.Warnings, w => w.Contains("Accuracy metric 110.5 out of range"));
    }

    [Fact]
    public void Map_NullMetrics_PreservedAsNull()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-01-15",
            Score: null,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Null(result.Evaluation!.Score);
        Assert.Null(result.Evaluation.SpeedMetric);
        Assert.Null(result.Evaluation.AccuracyMetric);
    }

    [Fact]
    public void Map_PreservesHistoricalTimestamps()
    {
        var created = new DateTime(2023, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var modified = new DateTime(2023, 6, 20, 14, 30, 0, DateTimeKind.Utc);

        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2023-06-15",
            Score: 75,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: created,
            OriginalModifiedAt: modified);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Equal(created, result.Evaluation!.CreatedAt);
        Assert.Equal(modified, result.Evaluation.ModifiedAt);
    }

    [Fact]
    public void Map_AccessDateTimeFormat_ParsesCorrectly()
    {
        var record = new LegacyEvaluationImportRecord(
            StudentId: 1,
            ActivityId: 10,
            EvaluationDate: "2024-03-20T00:00:00",
            Score: 90,
            SpeedMetric: null,
            AccuracyMetric: null,
            Notes: null,
            OriginalCreatedAt: null,
            OriginalModifiedAt: null);

        var result = _mapper.Map(record);

        Assert.NotNull(result.Evaluation);
        Assert.Equal("2024-03-20", result.Evaluation!.EvaluationDate);
    }
}
