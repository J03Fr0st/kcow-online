using System.Data;
using Dapper;
using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Import;

/// <summary>
/// Orchestrates the full legacy data import pipeline.
/// Parses XML files, maps to domain entities, and inserts/updates in the database
/// using a shared connection + transaction per entity type.
/// Supports re-run with conflict detection via legacy_id matching.
/// </summary>
public sealed class ImportExecutionService : IImportExecutionService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILegacyParser _parser;

    public ImportExecutionService(IDbConnectionFactory connectionFactory, ILegacyParser parser)
    {
        _connectionFactory = connectionFactory;
        _parser = parser;
    }

    public Task<ImportExecutionResult> ExecuteAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(inputPath, ConflictResolutionMode.FailOnConflict, cancellationToken);
    }

    public async Task<ImportExecutionResult> ExecuteAsync(string inputPath, ConflictResolutionMode conflictMode, CancellationToken cancellationToken = default)
    {
        var result = new ImportExecutionResult
        {
            InputPath = inputPath,
            ExecutedAt = DateTime.UtcNow,
            ConflictMode = conflictMode
        };

        using var connection = await _connectionFactory.CreateAsync(cancellationToken);

        // Import in dependency order: Schools → ClassGroups → Activities → Students
        result.Schools = await ImportSchoolsAsync(connection, inputPath, conflictMode, result.Exceptions);
        result.ClassGroups = await ImportClassGroupsAsync(connection, inputPath, conflictMode, result.Exceptions);
        result.Activities = await ImportActivitiesAsync(connection, inputPath, conflictMode, result.Exceptions);
        result.Students = await ImportStudentsAsync(connection, inputPath, conflictMode, result.Exceptions);

        return result;
    }

    private async Task<EntityImportResult> ImportSchoolsAsync(
        IDbConnection connection, string inputPath, ConflictResolutionMode conflictMode, List<ImportException> exceptions)
    {
        var counts = new EntityImportResult();
        var folder = Path.Combine(inputPath, "1_School");
        var xmlPath = Path.Combine(folder, "School.xml");
        var xsdPath = Path.Combine(folder, "School.xsd");

        if (!File.Exists(xmlPath) || !File.Exists(xsdPath))
            return counts;

        var parseResult = _parser.ParseSchools(xmlPath, xsdPath);
        AddParseExceptions(parseResult, "School", exceptions);

        var mapper = new SchoolDataMapper();
        var mapResult = mapper.MapMany(parseResult.Records);
        AddMappingExceptions(mapResult, "School", exceptions, ref counts);

        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var school in mapResult.Data)
            {
                try
                {
                    var existing = await FindByLegacyIdAsync<School>(connection, transaction, "schools", school.LegacyId);
                    if (existing is not null)
                    {
                        switch (conflictMode)
                        {
                            case ConflictResolutionMode.FailOnConflict:
                                throw new InvalidOperationException(
                                    $"School with legacy_id '{school.LegacyId}' already exists (id={existing.Id}).");
                            case ConflictResolutionMode.SkipExisting:
                                counts.Skipped++;
                                continue;
                            case ConflictResolutionMode.Update:
                                school.Id = existing.Id;
                                school.CreatedAt = existing.CreatedAt;
                                school.UpdatedAt = DateTime.UtcNow;
                                await UpdateSchoolAsync(connection, transaction, school);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertSchoolAsync(connection, transaction, school);
                    counts.Imported++;
                }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("School", school.LegacyId ?? school.Id.ToString(), "_insert", ex.Message));
                }
            }
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    private async Task<EntityImportResult> ImportClassGroupsAsync(
        IDbConnection connection, string inputPath, ConflictResolutionMode conflictMode, List<ImportException> exceptions)
    {
        var counts = new EntityImportResult();
        var folder = Path.Combine(inputPath, "2_Class_Group");
        var xmlPath = Path.Combine(folder, "Class Group.xml");
        var xsdPath = Path.Combine(folder, "Class Group.xsd");

        if (!File.Exists(xmlPath) || !File.Exists(xsdPath))
            return counts;

        var parseResult = _parser.ParseClassGroups(xmlPath, xsdPath);
        AddParseExceptions(parseResult, "ClassGroup", exceptions);

        var mapper = new ClassGroupDataMapper();
        var mapResult = mapper.MapMany(parseResult.Records);
        AddMappingExceptions(mapResult, "ClassGroup", exceptions, ref counts);

        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var classGroup in mapResult.Data)
            {
                try
                {
                    var existing = await FindByLegacyIdAsync<ClassGroup>(connection, transaction, "class_groups", classGroup.LegacyId);
                    if (existing is not null)
                    {
                        switch (conflictMode)
                        {
                            case ConflictResolutionMode.FailOnConflict:
                                throw new InvalidOperationException(
                                    $"ClassGroup with legacy_id '{classGroup.LegacyId}' already exists (id={existing.Id}).");
                            case ConflictResolutionMode.SkipExisting:
                                counts.Skipped++;
                                continue;
                            case ConflictResolutionMode.Update:
                                classGroup.Id = existing.Id;
                                classGroup.CreatedAt = existing.CreatedAt;
                                classGroup.UpdatedAt = DateTime.UtcNow;
                                await UpdateClassGroupAsync(connection, transaction, classGroup);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertClassGroupAsync(connection, transaction, classGroup);
                    counts.Imported++;
                }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("ClassGroup", classGroup.LegacyId ?? classGroup.Name, "_insert", ex.Message));
                }
            }
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    private async Task<EntityImportResult> ImportActivitiesAsync(
        IDbConnection connection, string inputPath, ConflictResolutionMode conflictMode, List<ImportException> exceptions)
    {
        var counts = new EntityImportResult();
        var folder = Path.Combine(inputPath, "3_Activity");
        var xmlPath = Path.Combine(folder, "Activity.xml");
        var xsdPath = Path.Combine(folder, "Activity.xsd");

        if (!File.Exists(xmlPath) || !File.Exists(xsdPath))
            return counts;

        var parseResult = _parser.ParseActivities(xmlPath, xsdPath);
        AddParseExceptions(parseResult, "Activity", exceptions);

        var mapper = new ActivityDataMapper();
        var mapResult = mapper.MapMany(parseResult.Records);
        AddMappingExceptions(mapResult, "Activity", exceptions, ref counts);

        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var activity in mapResult.Data)
            {
                try
                {
                    var existing = await FindByLegacyIdAsync<Activity>(connection, transaction, "activities", activity.LegacyId);
                    if (existing is not null)
                    {
                        switch (conflictMode)
                        {
                            case ConflictResolutionMode.FailOnConflict:
                                throw new InvalidOperationException(
                                    $"Activity with legacy_id '{activity.LegacyId}' already exists (id={existing.Id}).");
                            case ConflictResolutionMode.SkipExisting:
                                counts.Skipped++;
                                continue;
                            case ConflictResolutionMode.Update:
                                activity.Id = existing.Id;
                                activity.CreatedAt = existing.CreatedAt;
                                activity.UpdatedAt = DateTime.UtcNow;
                                await UpdateActivityAsync(connection, transaction, activity);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertActivityAsync(connection, transaction, activity);
                    counts.Imported++;
                }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("Activity", activity.LegacyId ?? activity.Id.ToString(), "_insert", ex.Message));
                }
            }
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    private async Task<EntityImportResult> ImportStudentsAsync(
        IDbConnection connection, string inputPath, ConflictResolutionMode conflictMode, List<ImportException> exceptions)
    {
        var counts = new EntityImportResult();
        var folder = Path.Combine(inputPath, "4_Children");
        var xmlPath = Path.Combine(folder, "Children.xml");
        var xsdPath = Path.Combine(folder, "Children.xsd");

        if (!File.Exists(xmlPath) || !File.Exists(xsdPath))
            return counts;

        var parseResult = _parser.ParseChildren(xmlPath, xsdPath);
        AddParseExceptions(parseResult, "Student", exceptions);

        var mapper = new StudentDataMapper();
        var mapResult = mapper.MapMany(parseResult.Records);
        AddMappingExceptions(mapResult, "Student", exceptions, ref counts);

        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var data in mapResult.Data)
            {
                try
                {
                    var existing = await FindByLegacyIdAsync<Student>(connection, transaction, "students", data.Student.LegacyId);
                    if (existing is not null)
                    {
                        switch (conflictMode)
                        {
                            case ConflictResolutionMode.FailOnConflict:
                                throw new InvalidOperationException(
                                    $"Student with legacy_id '{data.Student.LegacyId}' already exists (id={existing.Id}).");
                            case ConflictResolutionMode.SkipExisting:
                                counts.Skipped++;
                                continue;
                            case ConflictResolutionMode.Update:
                                data.Student.Id = existing.Id;
                                data.Student.CreatedAt = existing.CreatedAt;
                                data.Student.UpdatedAt = DateTime.UtcNow;
                                await UpdateStudentAsync(connection, transaction, data.Student);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertStudentAsync(connection, transaction, data.Student);
                    counts.Imported++;
                }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("Student", data.Student.LegacyId ?? data.Student.Reference ?? "", "_insert", ex.Message));
                }
            }
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    // Conflict detection: find existing record by legacy_id

    private static async Task<T?> FindByLegacyIdAsync<T>(
        IDbConnection connection, IDbTransaction transaction, string tableName, string? legacyId)
    {
        if (string.IsNullOrEmpty(legacyId)) return default;
        var sql = $"SELECT * FROM {tableName} WHERE legacy_id = @LegacyId LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<T>(sql, new { LegacyId = legacyId }, transaction);
    }

    private static void AddParseExceptions<T>(ParseResult<T> result, string entityType, List<ImportException> exceptions)
    {
        foreach (var error in result.Errors)
        {
            exceptions.Add(new ImportException(entityType, "", "_parse", error.Message));
        }
    }

    private static void AddMappingExceptions<T>(MappingResult<List<T>> result, string entityType,
        List<ImportException> exceptions, ref EntityImportResult counts)
    {
        foreach (var error in result.Errors)
        {
            counts.Failed++;
            exceptions.Add(new ImportException(entityType, "", error.Field, error.Message));
        }

        // Count skipped items from warnings that indicate skips
        foreach (var warning in result.Warnings.Where(w => w.Field == "_skip"))
        {
            counts.Skipped++;
        }
    }

    // INSERT methods

    private static async Task InsertSchoolAsync(IDbConnection connection, IDbTransaction transaction, School school)
    {
        const string sql = @"
            INSERT INTO schools (name, short_name, truck_id, price, fee_description, formula,
                   visit_day, visit_sequence, contact_person, contact_cell, telephone, fax, email,
                   circulars_email, address, address2, headmaster, headmaster_cell, is_active, language,
                   print_invoice, import_flag, afterschool1_name, afterschool1_contact, afterschool2_name,
                   afterschool2_contact, money_message, safe_notes, web_page,
                   kcow_web_page_link, legacy_id, created_at)
            VALUES (@Name, @ShortName, @TruckId, @Price, @FeeDescription, @Formula,
                   @VisitDay, @VisitSequence, @ContactPerson, @ContactCell, @Telephone, @Fax, @Email,
                   @CircularsEmail, @Address, @Address2, @Headmaster, @HeadmasterCell, @IsActive, @Language,
                   @PrintInvoice, @ImportFlag, @Afterschool1Name, @Afterschool1Contact, @Afterschool2Name,
                   @Afterschool2Contact, @MoneyMessage, @SafeNotes, @WebPage,
                   @KcowWebPageLink, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(sql, school, transaction);
    }

    private static async Task InsertClassGroupAsync(IDbConnection connection, IDbTransaction transaction, ClassGroup cg)
    {
        const string sql = @"
            INSERT INTO class_groups (name, day_truck, description, school_id, truck_id, day_of_week,
                   start_time, end_time, sequence, evaluate, notes, import_flag, group_message,
                   send_certificates, money_message, ixl, is_active, legacy_id, created_at)
            VALUES (@Name, @DayTruck, @Description, @SchoolId, @TruckId, @DayOfWeek,
                   @StartTime, @EndTime, @Sequence, @Evaluate, @Notes, @ImportFlag, @GroupMessage,
                   @SendCertificates, @MoneyMessage, @Ixl, @IsActive, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(sql, cg, transaction);
    }

    private static async Task InsertActivityAsync(IDbConnection connection, IDbTransaction transaction, Activity activity)
    {
        const string sql = @"
            INSERT INTO activities (code, name, description, folder, grade_level, icon, is_active, legacy_id, created_at)
            VALUES (@Code, @Name, @Description, @Folder, @GradeLevel, @Icon, @IsActive, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(sql, activity, transaction);
    }

    private static async Task InsertStudentAsync(IDbConnection connection, IDbTransaction transaction, Student student)
    {
        const string sql = @"
            INSERT INTO students (reference, first_name, last_name, date_of_birth, gender, language,
                   account_person_name, account_person_surname, account_person_id_number,
                   account_person_cellphone, account_person_office, account_person_home,
                   account_person_email, relation,
                   mother_name, mother_surname, mother_office, mother_cell, mother_home, mother_email,
                   father_name, father_surname, father_office, father_cell, father_home, father_email,
                   address1, address2, postal_code, school_name, school_id, class_group_code, class_group_id,
                   grade, teacher, attending_kcow_at, aftercare, extra, home_time, start_classes,
                   terms, seat, truck, family, sequence, financial_code, charge, deposit, pay_date,
                   is_active, legacy_id, created_at)
            VALUES (@Reference, @FirstName, @LastName, @DateOfBirth, @Gender, @Language,
                   @AccountPersonName, @AccountPersonSurname, @AccountPersonIdNumber,
                   @AccountPersonCellphone, @AccountPersonOffice, @AccountPersonHome,
                   @AccountPersonEmail, @Relation,
                   @MotherName, @MotherSurname, @MotherOffice, @MotherCell, @MotherHome, @MotherEmail,
                   @FatherName, @FatherSurname, @FatherOffice, @FatherCell, @FatherHome, @FatherEmail,
                   @Address1, @Address2, @PostalCode, @SchoolName, @SchoolId, @ClassGroupCode, @ClassGroupId,
                   @Grade, @Teacher, @AttendingKcowAt, @Aftercare, @Extra, @HomeTime, @StartClasses,
                   @Terms, @Seat, @Truck, @Family, @Sequence, @FinancialCode, @Charge, @Deposit, @PayDate,
                   @IsActive, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(sql, student, transaction);
    }

    // UPDATE methods

    private static async Task UpdateSchoolAsync(IDbConnection connection, IDbTransaction transaction, School school)
    {
        const string sql = @"
            UPDATE schools SET name = @Name, short_name = @ShortName, truck_id = @TruckId,
                   price = @Price, fee_description = @FeeDescription, formula = @Formula,
                   visit_day = @VisitDay, visit_sequence = @VisitSequence, contact_person = @ContactPerson,
                   contact_cell = @ContactCell, telephone = @Telephone, fax = @Fax, email = @Email,
                   circulars_email = @CircularsEmail, address = @Address, address2 = @Address2,
                   headmaster = @Headmaster, headmaster_cell = @HeadmasterCell, is_active = @IsActive,
                   language = @Language, print_invoice = @PrintInvoice, import_flag = @ImportFlag,
                   afterschool1_name = @Afterschool1Name, afterschool1_contact = @Afterschool1Contact,
                   afterschool2_name = @Afterschool2Name, afterschool2_contact = @Afterschool2Contact,
                   money_message = @MoneyMessage, safe_notes = @SafeNotes, web_page = @WebPage,
                   kcow_web_page_link = @KcowWebPageLink, updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, school, transaction);
    }

    private static async Task UpdateClassGroupAsync(IDbConnection connection, IDbTransaction transaction, ClassGroup cg)
    {
        const string sql = @"
            UPDATE class_groups SET name = @Name, day_truck = @DayTruck, description = @Description,
                   school_id = @SchoolId, truck_id = @TruckId, day_of_week = @DayOfWeek,
                   start_time = @StartTime, end_time = @EndTime, sequence = @Sequence,
                   evaluate = @Evaluate, notes = @Notes, import_flag = @ImportFlag,
                   group_message = @GroupMessage, send_certificates = @SendCertificates,
                   money_message = @MoneyMessage, ixl = @Ixl, is_active = @IsActive,
                   updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, cg, transaction);
    }

    private static async Task UpdateActivityAsync(IDbConnection connection, IDbTransaction transaction, Activity activity)
    {
        const string sql = @"
            UPDATE activities SET code = @Code, name = @Name, description = @Description,
                   folder = @Folder, grade_level = @GradeLevel, icon = @Icon,
                   is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, activity, transaction);
    }

    private static async Task UpdateStudentAsync(IDbConnection connection, IDbTransaction transaction, Student student)
    {
        const string sql = @"
            UPDATE students SET reference = @Reference, first_name = @FirstName, last_name = @LastName,
                   date_of_birth = @DateOfBirth, gender = @Gender, language = @Language,
                   account_person_name = @AccountPersonName, account_person_surname = @AccountPersonSurname,
                   account_person_id_number = @AccountPersonIdNumber,
                   account_person_cellphone = @AccountPersonCellphone,
                   account_person_office = @AccountPersonOffice, account_person_home = @AccountPersonHome,
                   account_person_email = @AccountPersonEmail, relation = @Relation,
                   mother_name = @MotherName, mother_surname = @MotherSurname,
                   mother_office = @MotherOffice, mother_cell = @MotherCell,
                   mother_home = @MotherHome, mother_email = @MotherEmail,
                   father_name = @FatherName, father_surname = @FatherSurname,
                   father_office = @FatherOffice, father_cell = @FatherCell,
                   father_home = @FatherHome, father_email = @FatherEmail,
                   address1 = @Address1, address2 = @Address2, postal_code = @PostalCode,
                   school_name = @SchoolName, school_id = @SchoolId,
                   class_group_code = @ClassGroupCode, class_group_id = @ClassGroupId,
                   grade = @Grade, teacher = @Teacher, attending_kcow_at = @AttendingKcowAt,
                   aftercare = @Aftercare, extra = @Extra, home_time = @HomeTime,
                   start_classes = @StartClasses, terms = @Terms, seat = @Seat, truck = @Truck,
                   family = @Family, sequence = @Sequence, financial_code = @FinancialCode,
                   charge = @Charge, deposit = @Deposit, pay_date = @PayDate,
                   is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, student, transaction);
    }
}
