using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for Student entity operations.
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Gets all students.
    /// </summary>
    Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active students.
    /// </summary>
    Task<IEnumerable<Student>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a student by ID.
    /// </summary>
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a student by reference number.
    /// </summary>
    Task<Student?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets students by school ID.
    /// </summary>
    Task<IEnumerable<Student>> GetBySchoolIdAsync(int schoolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets students by class group ID.
    /// </summary>
    Task<IEnumerable<Student>> GetByClassGroupIdAsync(int classGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches students by name (first or last).
    /// </summary>
    Task<IEnumerable<Student>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new student.
    /// </summary>
    Task<int> CreateAsync(Student student, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    Task<bool> UpdateAsync(Student student, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a student by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a student exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a student exists by reference number.
    /// </summary>
    Task<bool> ExistsByReferenceAsync(string reference, CancellationToken cancellationToken = default);
}
