using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PaymentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Payment>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, student_id, invoice_id, payment_date, amount,
                   payment_method, receipt_number, notes, created_at
            FROM payments
            WHERE student_id = @StudentId
            ORDER BY payment_date DESC";
        return await connection.QueryAsync<Payment>(sql, new { StudentId = studentId });
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, student_id, invoice_id, payment_date, amount,
                   payment_method, receipt_number, notes, created_at
            FROM payments
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO payments (student_id, invoice_id, payment_date, amount, payment_method, receipt_number, notes, created_at)
            VALUES (@StudentId, @InvoiceId, @PaymentDate, @Amount, @PaymentMethod, @ReceiptNumber, @Notes, @CreatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, payment);
    }

    public async Task<bool> UpdateReceiptNumberAsync(int id, string receiptNumber, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "UPDATE payments SET receipt_number = @ReceiptNumber WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, ReceiptNumber = receiptNumber });
        return rowsAffected > 0;
    }
}
