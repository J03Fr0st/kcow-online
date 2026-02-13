using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IStudentRepository.
/// Student entity has 92 fields aligned with legacy XSD schema.
/// </summary>
public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StudentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string SelectColumns = @"
        id, reference, first_name, last_name, date_of_birth, gender, language,
        account_person_name, account_person_surname, account_person_id_number,
        account_person_cellphone, account_person_office, account_person_home, account_person_email,
        relation, mother_name, mother_surname, mother_office, mother_cell, mother_home, mother_email,
        father_name, father_surname, father_office, father_cell, father_home, father_email,
        address1, address2, postal_code, school_name, school_id, class_group_code, class_group_id,
        grade, teacher, attending_kcow_at, aftercare, extra, home_time, start_classes, terms, seat,
        truck, family, sequence, financial_code, charge, deposit, pay_date,
        tshirt_code, tshirt_money_1, tshirt_money_date_1, tshirt_received_1, tshirt_rec_date_1,
        receive_note_1, tshirt_size1, tshirt_color1, tshirt_design1,
        tshirt_size2, tshirt_money_2, tshirt_money_date_2, tshirt_received_2, tshirt_rec_date_2,
        receive_note_2, tshirt_color2, tshirt_design2,
        indicator1, indicator2, general_note, print_id_card, accept_terms_cond, status,
        sms_or_email, school_close, cnt, online_entry,
        created as LegacyCreated, submitted, updated as LegacyUpdated,
        book_email, report1_given_out, account_given_out, certificate_printed,
        report2_given_out, social, activity_report_given_out, photo_url, photo_updated,
        is_active, created_at, updated_at";

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students";
        return await connection.QueryAsync<Student>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Student>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students WHERE is_active = 1";
        return await connection.QueryAsync<Student>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Student>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<Student?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students WHERE reference = @Reference";
        return await connection.QueryFirstOrDefaultAsync<Student>(new CommandDefinition(sql, new { Reference = reference }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Student>> GetBySchoolIdAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students WHERE school_id = @SchoolId";
        return await connection.QueryAsync<Student>(new CommandDefinition(sql, new { SchoolId = schoolId }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Student>> GetByClassGroupIdAsync(int classGroupId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students WHERE class_group_id = @ClassGroupId";
        return await connection.QueryAsync<Student>(new CommandDefinition(sql, new { ClassGroupId = classGroupId }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Student>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM students WHERE first_name LIKE @SearchTerm OR last_name LIKE @SearchTerm";
        return await connection.QueryAsync<Student>(new CommandDefinition(sql, new { SearchTerm = $"%{searchTerm}%" }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(Student student, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            INSERT INTO students (
                reference, first_name, last_name, date_of_birth, gender, language,
                account_person_name, account_person_surname, account_person_id_number,
                account_person_cellphone, account_person_office, account_person_home, account_person_email,
                relation, mother_name, mother_surname, mother_office, mother_cell, mother_home, mother_email,
                father_name, father_surname, father_office, father_cell, father_home, father_email,
                address1, address2, postal_code, school_name, school_id, class_group_code, class_group_id,
                grade, teacher, attending_kcow_at, aftercare, extra, home_time, start_classes, terms, seat,
                truck, family, sequence, financial_code, charge, deposit, pay_date,
                tshirt_code, tshirt_money_1, tshirt_money_date_1, tshirt_received_1, tshirt_rec_date_1,
                receive_note_1, tshirt_size1, tshirt_color1, tshirt_design1,
                tshirt_size2, tshirt_money_2, tshirt_money_date_2, tshirt_received_2, tshirt_rec_date_2,
                receive_note_2, tshirt_color2, tshirt_design2,
                indicator1, indicator2, general_note, print_id_card, accept_terms_cond, status,
                sms_or_email, school_close, cnt, online_entry,
                created, submitted, updated,
                book_email, report1_given_out, account_given_out, certificate_printed,
                report2_given_out, social, activity_report_given_out, photo_url, photo_updated,
                is_active, created_at, updated_at
            ) VALUES (
                @Reference, @FirstName, @LastName, @DateOfBirth, @Gender, @Language,
                @AccountPersonName, @AccountPersonSurname, @AccountPersonIdNumber,
                @AccountPersonCellphone, @AccountPersonOffice, @AccountPersonHome, @AccountPersonEmail,
                @Relation, @MotherName, @MotherSurname, @MotherOffice, @MotherCell, @MotherHome, @MotherEmail,
                @FatherName, @FatherSurname, @FatherOffice, @FatherCell, @FatherHome, @FatherEmail,
                @Address1, @Address2, @PostalCode, @SchoolName, @SchoolId, @ClassGroupCode, @ClassGroupId,
                @Grade, @Teacher, @AttendingKcowAt, @Aftercare, @Extra, @HomeTime, @StartClasses, @Terms, @Seat,
                @Truck, @Family, @Sequence, @FinancialCode, @Charge, @Deposit, @PayDate,
                @TshirtCode, @TshirtMoney1, @TshirtMoneyDate1, @TshirtReceived1, @TshirtRecDate1,
                @ReceiveNote1, @TshirtSize1, @TshirtColor1, @TshirtDesign1,
                @TshirtSize2, @TshirtMoney2, @TshirtMoneyDate2, @TshirtReceived2, @TshirtRecDate2,
                @ReceiveNote2, @TshirtColor2, @TshirtDesign2,
                @Indicator1, @Indicator2, @GeneralNote, @PrintIdCard, @AcceptTermsCond, @Status,
                @SmsOrEmail, @SchoolClose, @Cnt, @OnlineEntry,
                @LegacyCreated, @Submitted, @LegacyUpdated,
                @BookEmail, @Report1GivenOut, @AccountGivenOut, @CertificatePrinted,
                @Report2GivenOut, @Social, @ActivityReportGivenOut, @PhotoUrl, @PhotoUpdated,
                @IsActive, @CreatedAt, @UpdatedAt
            )
            RETURNING id";
        return await connection.QuerySingleAsync<int>(new CommandDefinition(sql, student, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            UPDATE students SET
                reference = @Reference, first_name = @FirstName, last_name = @LastName,
                date_of_birth = @DateOfBirth, gender = @Gender, language = @Language,
                account_person_name = @AccountPersonName, account_person_surname = @AccountPersonSurname,
                account_person_id_number = @AccountPersonIdNumber,
                account_person_cellphone = @AccountPersonCellphone, account_person_office = @AccountPersonOffice,
                account_person_home = @AccountPersonHome, account_person_email = @AccountPersonEmail,
                relation = @Relation, mother_name = @MotherName, mother_surname = @MotherSurname,
                mother_office = @MotherOffice, mother_cell = @MotherCell, mother_home = @MotherHome,
                mother_email = @MotherEmail,
                father_name = @FatherName, father_surname = @FatherSurname, father_office = @FatherOffice,
                father_cell = @FatherCell, father_home = @FatherHome, father_email = @FatherEmail,
                address1 = @Address1, address2 = @Address2, postal_code = @PostalCode,
                school_name = @SchoolName, school_id = @SchoolId, class_group_code = @ClassGroupCode,
                class_group_id = @ClassGroupId, grade = @Grade, teacher = @Teacher,
                attending_kcow_at = @AttendingKcowAt, aftercare = @Aftercare, extra = @Extra,
                home_time = @HomeTime, start_classes = @StartClasses, terms = @Terms, seat = @Seat,
                truck = @Truck, family = @Family, sequence = @Sequence, financial_code = @FinancialCode,
                charge = @Charge, deposit = @Deposit, pay_date = @PayDate,
                tshirt_code = @TshirtCode, tshirt_money_1 = @TshirtMoney1, tshirt_money_date_1 = @TshirtMoneyDate1,
                tshirt_received_1 = @TshirtReceived1, tshirt_rec_date_1 = @TshirtRecDate1,
                receive_note_1 = @ReceiveNote1, tshirt_size1 = @TshirtSize1, tshirt_color1 = @TshirtColor1,
                tshirt_design1 = @TshirtDesign1,
                tshirt_size2 = @TshirtSize2, tshirt_money_2 = @TshirtMoney2, tshirt_money_date_2 = @TshirtMoneyDate2,
                tshirt_received_2 = @TshirtReceived2, tshirt_rec_date_2 = @TshirtRecDate2,
                receive_note_2 = @ReceiveNote2, tshirt_color2 = @TshirtColor2, tshirt_design2 = @TshirtDesign2,
                indicator1 = @Indicator1, indicator2 = @Indicator2, general_note = @GeneralNote,
                print_id_card = @PrintIdCard, accept_terms_cond = @AcceptTermsCond, status = @Status,
                sms_or_email = @SmsOrEmail, school_close = @SchoolClose, cnt = @Cnt, online_entry = @OnlineEntry,
                created = @LegacyCreated, submitted = @Submitted, updated = @LegacyUpdated,
                book_email = @BookEmail, report1_given_out = @Report1GivenOut, account_given_out = @AccountGivenOut,
                certificate_printed = @CertificatePrinted, report2_given_out = @Report2GivenOut,
                social = @Social, activity_report_given_out = @ActivityReportGivenOut,
                photo_url = @PhotoUrl, photo_updated = @PhotoUpdated,
                is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, student, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "DELETE FROM students WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "SELECT COUNT(1) FROM students WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return count > 0;
    }

    public async Task<bool> ExistsByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "SELECT COUNT(1) FROM students WHERE reference = @Reference";
        var count = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { Reference = reference }, cancellationToken: cancellationToken));
        return count > 0;
    }
}
