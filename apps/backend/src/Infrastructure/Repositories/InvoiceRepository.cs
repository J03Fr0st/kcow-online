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
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT id, student_id, invoice_date, amount, due_date, status,
                   description, notes, created_at
            FROM invoices
            WHERE student_id = @StudentId
            ORDER BY invoice_date DESC";
        return await connection.QueryAsync<Invoice>(new CommandDefinition(sql, new { StudentId = studentId }, cancellationToken: cancellationToken));
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT id, student_id, invoice_date, amount, due_date, status,
                   description, notes, created_at
            FROM invoices
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Invoice>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            INSERT INTO invoices (student_id, invoice_date, amount, due_date, status, description, notes, created_at)
            VALUES (@StudentId, @InvoiceDate, @Amount, @DueDate, @Status, @Description, @Notes, @CreatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(new CommandDefinition(sql, invoice, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            UPDATE invoices
            SET student_id = @StudentId, invoice_date = @InvoiceDate, amount = @Amount,
                due_date = @DueDate, status = @Status, description = @Description, notes = @Notes
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, invoice, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }
}
