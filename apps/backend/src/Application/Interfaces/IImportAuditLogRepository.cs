using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

public interface IImportAuditLogRepository
{
    Task<int> CreateAsync(ImportAuditLog auditLog, CancellationToken cancellationToken = default);
    Task UpdateAsync(ImportAuditLog auditLog, CancellationToken cancellationToken = default);
    Task<ImportAuditLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImportAuditLog>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);
}
