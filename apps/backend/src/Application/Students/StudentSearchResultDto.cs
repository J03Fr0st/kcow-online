namespace Kcow.Application.Students;

/// <summary>
/// DTO for student search results with minimal information for typeahead display.
/// </summary>
public class StudentSearchResultDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string SchoolName { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public string ClassGroupName { get; init; } = string.Empty;
}
