using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ImportExecutionService>? _logger;

    public ImportExecutionService(IDbConnectionFactory connectionFactory, ILegacyParser parser, ILogger<ImportExecutionService>? logger = null)
    {
        _connectionFactory = connectionFactory;
        _parser = parser;
        _logger = logger;
    }

    public Task<ImportExecutionResult> ExecuteAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(inputPath, ConflictResolutionMode.FailOnConflict, cancellationToken);
    }

    public Task<ImportExecutionResult> ExecuteAsync(string inputPath, ConflictResolutionMode conflictMode, CancellationToken cancellationToken = default) =>
        ExecuteAsync(new ImportPlanBuilder(_parser).Build(inputPath, conflictMode, cancellationToken), cancellationToken);

    public async Task<ImportExecutionResult> ExecuteAsync(ImportPlan plan, CancellationToken cancellationToken = default)
    {
        var result = new ImportExecutionResult { InputPath = plan.InputPath, ConflictMode = plan.ConflictMode,
            RunId = plan.RunId, SourceFingerprint = plan.SourceFingerprint };
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        result.Schools = await ImportSchoolsAsync(connection, plan.Schools, plan.ConflictMode, result.Exceptions, cancellationToken);
        LogOutcome(plan, "Schools", result.Schools);
        result.ClassGroups = await ImportClassGroupsAsync(connection, plan.ClassGroups, plan.ConflictMode, result.Exceptions, cancellationToken);
        LogOutcome(plan, "ClassGroups", result.ClassGroups);
        result.Activities = await ImportActivitiesAsync(connection, plan.Activities, plan.ConflictMode, result.Exceptions, cancellationToken);
        LogOutcome(plan, "Activities", result.Activities);
        result.Students = await ImportStudentsAsync(connection, plan.Students, plan.ConflictMode, result.Exceptions, cancellationToken);
        LogOutcome(plan, "Students", result.Students);
        return result;
    }

    private void LogOutcome(ImportPlan plan, string entity, EntityImportResult outcome) =>
        _logger?.LogInformation("Import {RunId} source {Fingerprint} entity {Entity} committed={Committed}, imported={Imported}, updated={Updated}, skipped={Skipped}, rejected={Failed}",
            plan.RunId, plan.SourceFingerprint, entity, outcome.Committed, outcome.Imported, outcome.Updated, outcome.Skipped, outcome.Failed);

    private async Task<EntityImportResult> ImportSchoolsAsync(
        IDbConnection connection, EntityPlan<School> plan, ConflictResolutionMode conflictMode, List<ImportException> exceptions, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var counts = new EntityImportResult();
        foreach (var error in plan.ParseErrors) exceptions.Add(new ImportException("School", "", "_parse", error));
        var mapResult = plan.GetMapping();
        AddMappingExceptions(mapResult, "School", exceptions, ref counts);
        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var school in mapResult.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var existing = await FindByLegacyIdAsync<School>(connection, transaction, "schools", school.LegacyId, cancellationToken);
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
                                await UpdateSchoolAsync(connection, transaction, school, cancellationToken);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertSchoolAsync(connection, transaction, school, cancellationToken);
                    counts.Imported++;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("School", school.LegacyId ?? school.Id.ToString(), "_insert", ex.Message));
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            transaction.Commit();
            counts.Committed = true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    private async Task<EntityImportResult> ImportClassGroupsAsync(
        IDbConnection connection, EntityPlan<ClassGroup> plan, ConflictResolutionMode conflictMode, List<ImportException> exceptions, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var counts = new EntityImportResult();
        foreach (var error in plan.ParseErrors) exceptions.Add(new ImportException("ClassGroup", "", "_parse", error));
        var mapResult = plan.GetMapping();
        AddMappingExceptions(mapResult, "ClassGroup", exceptions, ref counts);
        var validSchoolIds = (await connection.QueryAsync<int>(new CommandDefinition("SELECT id FROM schools", cancellationToken: cancellationToken))).ToHashSet();
        foreach (var group in mapResult.Data ?? [])
            if (group.SchoolId.HasValue && validSchoolIds.Count > 0 && !validSchoolIds.Contains(group.SchoolId.Value))
            {
                exceptions.Add(new ImportException("ClassGroup", group.LegacyId ?? group.Name, "SchoolId", "Missing school reference cleared at execution"));
                group.SchoolId = null;
            }
        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var classGroup in mapResult.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var existing = await FindByLegacyIdAsync<ClassGroup>(connection, transaction, "class_groups", classGroup.LegacyId, cancellationToken);
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
                                await UpdateClassGroupAsync(connection, transaction, classGroup, cancellationToken);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertClassGroupAsync(connection, transaction, classGroup, cancellationToken);
                    counts.Imported++;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("ClassGroup", classGroup.LegacyId ?? classGroup.Name, "_insert", ex.Message));
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            transaction.Commit();
            counts.Committed = true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    private async Task<EntityImportResult> ImportActivitiesAsync(
        IDbConnection connection, EntityPlan<Activity> plan, ConflictResolutionMode conflictMode, List<ImportException> exceptions, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var counts = new EntityImportResult();
        foreach (var error in plan.ParseErrors) exceptions.Add(new ImportException("Activity", "", "_parse", error));
        var mapResult = plan.GetMapping();
        AddMappingExceptions(mapResult, "Activity", exceptions, ref counts);
        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var activity in mapResult.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var existing = await FindByLegacyIdAsync<Activity>(connection, transaction, "activities", activity.LegacyId, cancellationToken);
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
                                await UpdateActivityAsync(connection, transaction, activity, cancellationToken);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertActivityAsync(connection, transaction, activity, cancellationToken);
                    counts.Imported++;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("Activity", activity.LegacyId ?? activity.Id.ToString(), "_insert", ex.Message));
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            transaction.Commit();
            counts.Committed = true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    private async Task<EntityImportResult> ImportStudentsAsync(
        IDbConnection connection, EntityPlan<StudentMappingData> plan, ConflictResolutionMode conflictMode, List<ImportException> exceptions, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var counts = new EntityImportResult();
        foreach (var error in plan.ParseErrors) exceptions.Add(new ImportException("Student", "", "_parse", error));
        var mapResult = plan.GetMapping();
        AddMappingExceptions(mapResult, "Student", exceptions, ref counts);
        if (mapResult.Data is null || mapResult.Data.Count == 0) return counts;

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var data in mapResult.Data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var existing = await FindByLegacyIdAsync<Student>(connection, transaction, "students", data.Student.LegacyId, cancellationToken);
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
                                await UpdateStudentAsync(connection, transaction, data.Student, cancellationToken);
                                counts.Updated++;
                                continue;
                        }
                    }

                    await InsertStudentAsync(connection, transaction, data.Student, cancellationToken);
                    counts.Imported++;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    counts.Failed++;
                    exceptions.Add(new ImportException("Student", data.Student.LegacyId ?? data.Student.Reference ?? "", "_insert", ex.Message));
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            transaction.Commit();
            counts.Committed = true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return counts;
    }

    // Conflict detection: find existing record by legacy_id

    private static readonly HashSet<string> AllowedTableNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "schools", "class_groups", "activities", "students"
    };

    private static async Task<T?> FindByLegacyIdAsync<T>(
        IDbConnection connection, IDbTransaction transaction, string tableName, string? legacyId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(legacyId)) return default;
        if (!AllowedTableNames.Contains(tableName))
            throw new ArgumentException($"Invalid table name: {tableName}", nameof(tableName));
        var sql = $"SELECT * FROM {tableName} WHERE legacy_id = @LegacyId LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql, new { LegacyId = legacyId }, transaction, cancellationToken: cancellationToken));
    }

    private static void AddMappingExceptions<T>(MappingResult<List<T>> result, string entityType,
        List<ImportException> exceptions, ref EntityImportResult counts)
    {
        foreach (var error in result.Errors)
        {
            counts.Failed++;
            exceptions.Add(new ImportException(entityType, "", error.Field, error.Message));
        }

        foreach (var warning in result.Warnings.Where(w => w.Field != "_skip"))
            exceptions.Add(new ImportException(entityType, "", warning.Field, "Warning: " + warning.Message, warning.OriginalValue));

        // Count skipped items from warnings that indicate skips
        foreach (var warning in result.Warnings.Where(w => w.Field == "_skip"))
        {
            counts.Skipped++;
        }
    }

    // INSERT methods

    private static async Task InsertSchoolAsync(IDbConnection connection, IDbTransaction transaction, School school, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO schools (name, short_name, school_description, truck_id, price, fee_description, formula,
                   visit_day, visit_sequence, contact_person, contact_cell, phone, telephone, fax, email,
                   circulars_email, address, address2, headmaster, headmaster_cell, is_active, language,
                   print_invoice, import_flag, afterschool1_name, afterschool1_contact, afterschool2_name,
                   afterschool2_contact, scheduling_notes, money_message, safe_notes, web_page,
                   omsendbriewe, kcow_web_page_link, legacy_id, created_at)
            VALUES (@Name, @ShortName, @SchoolDescription, @TruckId, @Price, @FeeDescription, @Formula,
                   @VisitDay, @VisitSequence, @ContactPerson, @ContactCell, @Phone, @Telephone, @Fax, @Email,
                   @CircularsEmail, @Address, @Address2, @Headmaster, @HeadmasterCell, @IsActive, @Language,
                   @PrintInvoice, @ImportFlag, @Afterschool1Name, @Afterschool1Contact, @Afterschool2Name,
                   @Afterschool2Contact, @SchedulingNotes, @MoneyMessage, @SafeNotes, @WebPage,
                   @Omsendbriewe, @KcowWebPageLink, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(new CommandDefinition(sql, school, transaction, cancellationToken: cancellationToken));
    }

    private static async Task InsertClassGroupAsync(IDbConnection connection, IDbTransaction transaction, ClassGroup cg, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO class_groups (name, day_truck, description, school_id, truck_id, day_of_week,
                   start_time, end_time, sequence, evaluate, notes, import_flag, group_message,
                   send_certificates, money_message, ixl, is_active, legacy_id, created_at)
            VALUES (@Name, @DayTruck, @Description, @SchoolId, @TruckId, @DayOfWeek,
                   @StartTime, @EndTime, @Sequence, @Evaluate, @Notes, @ImportFlag, @GroupMessage,
                   @SendCertificates, @MoneyMessage, @Ixl, @IsActive, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(new CommandDefinition(sql, cg, transaction, cancellationToken: cancellationToken));
    }

    private static async Task InsertActivityAsync(IDbConnection connection, IDbTransaction transaction, Activity activity, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO activities (code, name, description, folder, grade_level, icon, is_active, legacy_id, created_at)
            VALUES (@Code, @Name, @Description, @Folder, @GradeLevel, @Icon, @IsActive, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(new CommandDefinition(sql, activity, transaction, cancellationToken: cancellationToken));
    }

    private static async Task InsertStudentAsync(IDbConnection connection, IDbTransaction transaction, Student student, CancellationToken cancellationToken)
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
                   tshirt_code, tshirt_money_1, tshirt_money_date_1, tshirt_received_1, tshirt_rec_date_1,
                   receive_note_1, tshirt_size1, tshirt_color1, tshirt_design1,
                   tshirt_size2, tshirt_money_2, tshirt_money_date_2, tshirt_received_2, tshirt_rec_date_2,
                   receive_note_2, tshirt_color2, tshirt_design2,
                   indicator1, indicator2, general_note, print_id_card, accept_terms_cond,
                   status, sms_or_email, school_close, cnt, online_entry,
                   created, submitted, updated,
                   book_email, report1_given_out, account_given_out, certificate_printed,
                   report2_given_out, social, activity_report_given_out, photo_url, photo_updated,
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
                   @TshirtCode, @TshirtMoney1, @TshirtMoneyDate1, @TshirtReceived1, @TshirtRecDate1,
                   @ReceiveNote1, @TshirtSize1, @TshirtColor1, @TshirtDesign1,
                   @TshirtSize2, @TshirtMoney2, @TshirtMoneyDate2, @TshirtReceived2, @TshirtRecDate2,
                   @ReceiveNote2, @TshirtColor2, @TshirtDesign2,
                   @Indicator1, @Indicator2, @GeneralNote, @PrintIdCard, @AcceptTermsCond,
                   @Status, @SmsOrEmail, @SchoolClose, @Cnt, @OnlineEntry,
                   @LegacyCreated, @Submitted, @LegacyUpdated,
                   @BookEmail, @Report1GivenOut, @AccountGivenOut, @CertificatePrinted,
                   @Report2GivenOut, @Social, @ActivityReportGivenOut, @PhotoUrl, @PhotoUpdated,
                   @IsActive, @LegacyId, @CreatedAt)";
        await connection.ExecuteAsync(new CommandDefinition(sql, student, transaction, cancellationToken: cancellationToken));
    }

    // UPDATE methods

    private static async Task UpdateSchoolAsync(IDbConnection connection, IDbTransaction transaction, School school, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE schools SET name = @Name, short_name = @ShortName,
                   school_description = @SchoolDescription, truck_id = @TruckId,
                   price = @Price, fee_description = @FeeDescription, formula = @Formula,
                   visit_day = @VisitDay, visit_sequence = @VisitSequence, contact_person = @ContactPerson,
                   contact_cell = @ContactCell, phone = @Phone, telephone = @Telephone, fax = @Fax, email = @Email,
                   circulars_email = @CircularsEmail, address = @Address, address2 = @Address2,
                   headmaster = @Headmaster, headmaster_cell = @HeadmasterCell, is_active = @IsActive,
                   language = @Language, print_invoice = @PrintInvoice, import_flag = @ImportFlag,
                   afterschool1_name = @Afterschool1Name, afterschool1_contact = @Afterschool1Contact,
                   afterschool2_name = @Afterschool2Name, afterschool2_contact = @Afterschool2Contact,
                   scheduling_notes = @SchedulingNotes, money_message = @MoneyMessage,
                   safe_notes = @SafeNotes, web_page = @WebPage,
                   omsendbriewe = @Omsendbriewe, kcow_web_page_link = @KcowWebPageLink,
                   updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(new CommandDefinition(sql, school, transaction, cancellationToken: cancellationToken));
    }

    private static async Task UpdateClassGroupAsync(IDbConnection connection, IDbTransaction transaction, ClassGroup cg, CancellationToken cancellationToken)
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
        await connection.ExecuteAsync(new CommandDefinition(sql, cg, transaction, cancellationToken: cancellationToken));
    }

    private static async Task UpdateActivityAsync(IDbConnection connection, IDbTransaction transaction, Activity activity, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE activities SET code = @Code, name = @Name, description = @Description,
                   folder = @Folder, grade_level = @GradeLevel, icon = @Icon,
                   is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(new CommandDefinition(sql, activity, transaction, cancellationToken: cancellationToken));
    }

    private static async Task UpdateStudentAsync(IDbConnection connection, IDbTransaction transaction, Student student, CancellationToken cancellationToken)
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
                   tshirt_code = @TshirtCode,
                   tshirt_money_1 = @TshirtMoney1, tshirt_money_date_1 = @TshirtMoneyDate1,
                   tshirt_received_1 = @TshirtReceived1, tshirt_rec_date_1 = @TshirtRecDate1,
                   receive_note_1 = @ReceiveNote1, tshirt_size1 = @TshirtSize1,
                   tshirt_color1 = @TshirtColor1, tshirt_design1 = @TshirtDesign1,
                   tshirt_size2 = @TshirtSize2,
                   tshirt_money_2 = @TshirtMoney2, tshirt_money_date_2 = @TshirtMoneyDate2,
                   tshirt_received_2 = @TshirtReceived2, tshirt_rec_date_2 = @TshirtRecDate2,
                   receive_note_2 = @ReceiveNote2, tshirt_color2 = @TshirtColor2,
                   tshirt_design2 = @TshirtDesign2,
                   indicator1 = @Indicator1, indicator2 = @Indicator2,
                   general_note = @GeneralNote, print_id_card = @PrintIdCard,
                   accept_terms_cond = @AcceptTermsCond, status = @Status,
                   sms_or_email = @SmsOrEmail, school_close = @SchoolClose,
                   cnt = @Cnt, online_entry = @OnlineEntry,
                   created = @LegacyCreated, submitted = @Submitted, updated = @LegacyUpdated,
                   book_email = @BookEmail, report1_given_out = @Report1GivenOut,
                   account_given_out = @AccountGivenOut, certificate_printed = @CertificatePrinted,
                   report2_given_out = @Report2GivenOut, social = @Social,
                   activity_report_given_out = @ActivityReportGivenOut,
                   photo_url = @PhotoUrl, photo_updated = @PhotoUpdated,
                   is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";
        await connection.ExecuteAsync(new CommandDefinition(sql, student, transaction, cancellationToken: cancellationToken));
    }
}
