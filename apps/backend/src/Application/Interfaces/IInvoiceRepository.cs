using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
