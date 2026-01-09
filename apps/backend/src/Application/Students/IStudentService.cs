using Kcow.Application.Common;

namespace Kcow.Application.Students;

/// <summary>
/// Service interface for student management operations.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Gets a paginated list of students with optional filtering.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="schoolId">Optional school ID filter</param>
    /// <param name="classGroupId">Optional class group ID filter</param>
    /// <param name="search">Optional search term for name or reference</param>
    /// <returns>Paged response of student list DTOs</returns>
    Task<PagedResponse<StudentListDto>> GetPagedAsync(int page, int pageSize, int? schoolId = null, int? classGroupId = null, string? search = null);

    /// <summary>
    /// Gets a student by ID with school, class group, and family details.
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student DTO or null if not found</returns>
    Task<StudentDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new student.
    /// </summary>
    /// <param name="request">Create student request</param>
    /// <returns>Created student DTO</returns>
    Task<StudentDto> CreateAsync(CreateStudentRequest request);

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="request">Update student request</param>
    /// <returns>Updated student DTO or null if not found</returns>
    Task<StudentDto?> UpdateAsync(int id, UpdateStudentRequest request);

    /// <summary>
    /// Archives (soft-deletes) a student by setting IsActive to false.
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>True if student was archived, false if not found</returns>
    Task<bool> ArchiveAsync(int id);
}
