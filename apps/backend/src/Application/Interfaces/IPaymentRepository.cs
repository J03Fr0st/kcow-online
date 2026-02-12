using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<bool> UpdateReceiptNumberAsync(int id, string receiptNumber, CancellationToken cancellationToken = default);
}
