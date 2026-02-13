using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of ISchoolRepository.
/// </summary>
public class SchoolRepository : ISchoolRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SchoolRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<School>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT id, name, short_name, school_description, truck_id, price, fee_description, formula,
                   visit_day, visit_sequence, contact_person, contact_cell, phone, telephone, fax, email,
                   circulars_email, address, address2, headmaster, headmaster_cell, is_active, language,
                   print_invoice, import_flag, afterschool1_name, afterschool1_contact, afterschool2_name,
                   afterschool2_contact, scheduling_notes, money_message, safe_notes, web_page, omsendbriewe,
                   kcow_web_page_link, created_at, updated_at
            FROM schools";
        return await connection.QueryAsync<School>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<School>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT id, name, short_name, school_description, truck_id, price, fee_description, formula,
                   visit_day, visit_sequence, contact_person, contact_cell, phone, telephone, fax, email,
                   circulars_email, address, address2, headmaster, headmaster_cell, is_active, language,
                   print_invoice, import_flag, afterschool1_name, afterschool1_contact, afterschool2_name,
                   afterschool2_contact, scheduling_notes, money_message, safe_notes, web_page, omsendbriewe,
                   kcow_web_page_link, created_at, updated_at
            FROM schools
            WHERE is_active = 1";
        return await connection.QueryAsync<School>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<School?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT id, name, short_name, school_description, truck_id, price, fee_description, formula,
                   visit_day, visit_sequence, contact_person, contact_cell, phone, telephone, fax, email,
                   circulars_email, address, address2, headmaster, headmaster_cell, is_active, language,
                   print_invoice, import_flag, afterschool1_name, afterschool1_contact, afterschool2_name,
                   afterschool2_contact, scheduling_notes, money_message, safe_notes, web_page, omsendbriewe,
                   kcow_web_page_link, created_at, updated_at
            FROM schools
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<School>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(School school, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            INSERT INTO schools (name, short_name, school_description, truck_id, price, fee_description, formula,
                   visit_day, visit_sequence, contact_person, contact_cell, phone, telephone, fax, email,
                   circulars_email, address, address2, headmaster, headmaster_cell, is_active, language,
                   print_invoice, import_flag, afterschool1_name, afterschool1_contact, afterschool2_name,
                   afterschool2_contact, scheduling_notes, money_message, safe_notes, web_page, omsendbriewe,
                   kcow_web_page_link, created_at, updated_at)
            VALUES (@Name, @ShortName, @SchoolDescription, @TruckId, @Price, @FeeDescription, @Formula,
                   @VisitDay, @VisitSequence, @ContactPerson, @ContactCell, @Phone, @Telephone, @Fax, @Email,
                   @CircularsEmail, @Address, @Address2, @Headmaster, @HeadmasterCell, @IsActive, @Language,
                   @PrintInvoice, @ImportFlag, @Afterschool1Name, @Afterschool1Contact, @Afterschool2Name,
                   @Afterschool2Contact, @SchedulingNotes, @MoneyMessage, @SafeNotes, @WebPage, @Omsendbriewe,
                   @KcowWebPageLink, @CreatedAt, @UpdatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(new CommandDefinition(sql, school, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(School school, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            UPDATE schools
            SET name = @Name,
                short_name = @ShortName,
                school_description = @SchoolDescription,
                truck_id = @TruckId,
                price = @Price,
                fee_description = @FeeDescription,
                formula = @Formula,
                visit_day = @VisitDay,
                visit_sequence = @VisitSequence,
                contact_person = @ContactPerson,
                contact_cell = @ContactCell,
                phone = @Phone,
                telephone = @Telephone,
                fax = @Fax,
                email = @Email,
                circulars_email = @CircularsEmail,
                address = @Address,
                address2 = @Address2,
                headmaster = @Headmaster,
                headmaster_cell = @HeadmasterCell,
                is_active = @IsActive,
                language = @Language,
                print_invoice = @PrintInvoice,
                import_flag = @ImportFlag,
                afterschool1_name = @Afterschool1Name,
                afterschool1_contact = @Afterschool1Contact,
                afterschool2_name = @Afterschool2Name,
                afterschool2_contact = @Afterschool2Contact,
                scheduling_notes = @SchedulingNotes,
                money_message = @MoneyMessage,
                safe_notes = @SafeNotes,
                web_page = @WebPage,
                omsendbriewe = @Omsendbriewe,
                kcow_web_page_link = @KcowWebPageLink,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, school, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "DELETE FROM schools WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "SELECT COUNT(1) FROM schools WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return count > 0;
    }
}
