using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Invoice>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, student_id, invoice_date, amount, due_date, status,
                   description, notes, created_at
            FROM invoices
            WHERE student_id = @StudentId
            ORDER BY invoice_date DESC";
        return await connection.QueryAsync<Invoice>(sql, new { StudentId = studentId });
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, student_id, invoice_date, amount, due_date, status,
                   description, notes, created_at
            FROM invoices
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Invoice>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO invoices (student_id, invoice_date, amount, due_date, status, description, notes, created_at)
            VALUES (@StudentId, @InvoiceDate, @Amount, @DueDate, @Status, @Description, @Notes, @CreatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, invoice);
    }

    public async Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE invoices
            SET student_id = @StudentId, invoice_date = @InvoiceDate, amount = @Amount,
                due_date = @DueDate, status = @Status, description = @Description, notes = @Notes
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, invoice);
        return rowsAffected > 0;
    }
}
