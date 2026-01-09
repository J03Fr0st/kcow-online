using Kcow.Application.Common;
using Kcow.Application.Students;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Students;

/// <summary>
/// Implementation of student management service.
/// </summary>
public class StudentService : IStudentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(AppDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of students with optional filtering.
    /// </summary>
    public async Task<PagedResponse<StudentListDto>> GetPagedAsync(int page, int pageSize, int? schoolId = null, int? classGroupId = null, string? search = null)
    {
        var query = _context.Students
            .Include(s => s.School)
            .Include(s => s.ClassGroup)
            .Where(s => s.IsActive)  // Only show active students
            .AsNoTracking();

        if (schoolId.HasValue)
            query = query.Where(s => s.SchoolId == schoolId);

        if (classGroupId.HasValue)
            query = query.Where(s => s.ClassGroupId == classGroupId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Escape LIKE special characters to prevent SQL injection
            var escapedSearch = search.Replace("%", "\\%")
                                     .Replace("_", "\\_")
                                     .Replace("[", "\\[")
                                     .Replace("]", "\\]");

            query = query.Where(s =>
                ((s.FirstName != null && EF.Functions.Like(s.FirstName, $"%{escapedSearch}%")) ||
                (s.LastName != null && EF.Functions.Like(s.LastName, $"%{escapedSearch}%")) ||
                EF.Functions.Like(s.Reference, $"%{escapedSearch}%")));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentListDto
            {
                Id = s.Id,
                Reference = s.Reference,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Grade = s.Grade,
                IsActive = s.IsActive,
                Status = s.Status,
                School = s.School != null ? new SchoolDto { Id = s.School.Id, Name = s.School.Name } : null,
                ClassGroup = s.ClassGroup != null ? new ClassGroupDto { Id = s.ClassGroup.Id, Name = s.ClassGroup.Name } : null
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved paged students: Page {Page}, PageSize {PageSize}, Total {TotalCount}", page, pageSize, totalCount);

        return new PagedResponse<StudentListDto>(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Gets a student by ID with school and class group details.
    /// </summary>
    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var student = await _context.Students
            .Include(s => s.School)
            .Include(s => s.ClassGroup)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            _logger.LogWarning("Student with ID {StudentId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved student with ID {StudentId}", id);

        return MapToDto(student);
    }

    /// <summary>
    /// Creates a new student.
    /// </summary>
    public async Task<StudentDto> CreateAsync(CreateStudentRequest request)
    {
        // Optimistic duplicate check for user feedback (still vulnerable to race condition)
        var exists = await _context.Students.AnyAsync(s => s.Reference == request.Reference && s.IsActive);
        if (exists)
        {
            throw new InvalidOperationException($"Student with reference '{request.Reference}' already exists");
        }

        var student = new Student
        {
            Reference = request.Reference,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Language = request.Language,
            AccountPersonName = request.AccountPersonName,
            AccountPersonSurname = request.AccountPersonSurname,
            AccountPersonIdNumber = request.AccountPersonIdNumber,
            AccountPersonCellphone = request.AccountPersonCellphone,
            AccountPersonOffice = request.AccountPersonOffice,
            AccountPersonHome = request.AccountPersonHome,
            AccountPersonEmail = request.AccountPersonEmail,
            Relation = request.Relation,
            MotherName = request.MotherName,
            MotherSurname = request.MotherSurname,
            MotherOffice = request.MotherOffice,
            MotherCell = request.MotherCell,
            MotherHome = request.MotherHome,
            MotherEmail = request.MotherEmail,
            FatherName = request.FatherName,
            FatherSurname = request.FatherSurname,
            FatherOffice = request.FatherOffice,
            FatherCell = request.FatherCell,
            FatherHome = request.FatherHome,
            FatherEmail = request.FatherEmail,
            Address1 = request.Address1,
            Address2 = request.Address2,
            PostalCode = request.PostalCode,
            SchoolName = request.SchoolName,
            SchoolId = request.SchoolId,
            ClassGroupCode = request.ClassGroupCode,
            ClassGroupId = request.ClassGroupId,
            Grade = request.Grade,
            Teacher = request.Teacher,
            AttendingKcowAt = request.AttendingKcowAt,
            Aftercare = request.Aftercare,
            Extra = request.Extra,
            HomeTime = request.HomeTime,
            StartClasses = request.StartClasses,
            Terms = request.Terms,
            Seat = request.Seat,
            Truck = request.Truck,
            Family = request.Family,
            Sequence = request.Sequence,
            FinancialCode = request.FinancialCode,
            Charge = request.Charge,
            Deposit = request.Deposit,
            PayDate = request.PayDate,
            TshirtCode = request.TshirtCode,
            TshirtMoney1 = request.TshirtMoney1,
            TshirtMoneyDate1 = request.TshirtMoneyDate1,
            TshirtReceived1 = request.TshirtReceived1,
            TshirtRecDate1 = request.TshirtRecDate1,
            ReceiveNote1 = request.ReceiveNote1,
            TshirtSize1 = request.TshirtSize1,
            TshirtColor1 = request.TshirtColor1,
            TshirtDesign1 = request.TshirtDesign1,
            TshirtSize2 = request.TshirtSize2,
            TshirtMoney2 = request.TshirtMoney2,
            TshirtMoneyDate2 = request.TshirtMoneyDate2,
            TshirtReceived2 = request.TshirtReceived2,
            TshirtRecDate2 = request.TshirtRecDate2,
            ReceiveNote2 = request.ReceiveNote2,
            TshirtColor2 = request.TshirtColor2,
            TshirtDesign2 = request.TshirtDesign2,
            Indicator1 = request.Indicator1,
            Indicator2 = request.Indicator2,
            GeneralNote = request.GeneralNote,
            PrintIdCard = request.PrintIdCard,
            AcceptTermsCond = request.AcceptTermsCond,
            Status = request.Status,
            SmsOrEmail = request.SmsOrEmail,
            SchoolClose = request.SchoolClose,
            Cnt = request.Cnt,
            OnlineEntry = request.OnlineEntry,
            PhotoUrl = request.PhotoUrl,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Students.Add(student);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException != null)
        {
            // Unique constraint violation (race condition detected)
            // Check if it's a Postgres unique_violation (23505) or SQL Server error 2601
            var message = ex.InnerException.Message;
            if (message.Contains("unique") || message.Contains("duplicate") || message.Contains("23505") || message.Contains("2601"))
            {
                throw new InvalidOperationException($"Student with reference '{request.Reference}' already exists", ex);
            }
            throw; // Re-throw if it's not a unique constraint violation
        }

        _logger.LogInformation("Created student with ID {StudentId} and reference {Reference}", student.Id, student.Reference);

        return await GetByIdAsync(student.Id) ?? MapToDto(student);
    }

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    public async Task<StudentDto?> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            _logger.LogWarning("Cannot update: Student with ID {StudentId} not found", id);
            return null;
        }

        // Check for duplicate reference (excluding current student)
        var duplicateExists = await _context.Students.AnyAsync(s => s.Reference == request.Reference && s.Id != id);
        if (duplicateExists)
        {
            throw new InvalidOperationException($"Student with reference '{request.Reference}' already exists");
        }

        student.Reference = request.Reference;
        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.DateOfBirth = request.DateOfBirth;
        student.Gender = request.Gender;
        student.Language = request.Language;
        student.AccountPersonName = request.AccountPersonName;
        student.AccountPersonSurname = request.AccountPersonSurname;
        student.AccountPersonIdNumber = request.AccountPersonIdNumber;
        student.AccountPersonCellphone = request.AccountPersonCellphone;
        student.AccountPersonOffice = request.AccountPersonOffice;
        student.AccountPersonHome = request.AccountPersonHome;
        student.AccountPersonEmail = request.AccountPersonEmail;
        student.Relation = request.Relation;
        student.MotherName = request.MotherName;
        student.MotherSurname = request.MotherSurname;
        student.MotherOffice = request.MotherOffice;
        student.MotherCell = request.MotherCell;
        student.MotherHome = request.MotherHome;
        student.MotherEmail = request.MotherEmail;
        student.FatherName = request.FatherName;
        student.FatherSurname = request.FatherSurname;
        student.FatherOffice = request.FatherOffice;
        student.FatherCell = request.FatherCell;
        student.FatherHome = request.FatherHome;
        student.FatherEmail = request.FatherEmail;
        student.Address1 = request.Address1;
        student.Address2 = request.Address2;
        student.PostalCode = request.PostalCode;
        student.SchoolName = request.SchoolName;
        student.SchoolId = request.SchoolId;
        student.ClassGroupCode = request.ClassGroupCode;
        student.ClassGroupId = request.ClassGroupId;
        student.Grade = request.Grade;
        student.Teacher = request.Teacher;
        student.AttendingKcowAt = request.AttendingKcowAt;
        student.Aftercare = request.Aftercare;
        student.Extra = request.Extra;
        student.HomeTime = request.HomeTime;
        student.StartClasses = request.StartClasses;
        student.Terms = request.Terms;
        student.Seat = request.Seat;
        student.Truck = request.Truck;
        student.Family = request.Family;
        student.Sequence = request.Sequence;
        student.FinancialCode = request.FinancialCode;
        student.Charge = request.Charge;
        student.Deposit = request.Deposit;
        student.PayDate = request.PayDate;
        student.TshirtCode = request.TshirtCode;
        student.TshirtMoney1 = request.TshirtMoney1;
        student.TshirtMoneyDate1 = request.TshirtMoneyDate1;
        student.TshirtReceived1 = request.TshirtReceived1;
        student.TshirtRecDate1 = request.TshirtRecDate1;
        student.ReceiveNote1 = request.ReceiveNote1;
        student.TshirtSize1 = request.TshirtSize1;
        student.TshirtColor1 = request.TshirtColor1;
        student.TshirtDesign1 = request.TshirtDesign1;
        student.TshirtSize2 = request.TshirtSize2;
        student.TshirtMoney2 = request.TshirtMoney2;
        student.TshirtMoneyDate2 = request.TshirtMoneyDate2;
        student.TshirtReceived2 = request.TshirtReceived2;
        student.TshirtRecDate2 = request.TshirtRecDate2;
        student.ReceiveNote2 = request.ReceiveNote2;
        student.TshirtColor2 = request.TshirtColor2;
        student.TshirtDesign2 = request.TshirtDesign2;
        student.Indicator1 = request.Indicator1;
        student.Indicator2 = request.Indicator2;
        student.GeneralNote = request.GeneralNote;
        student.PrintIdCard = request.PrintIdCard;
        student.AcceptTermsCond = request.AcceptTermsCond;
        student.Status = request.Status;
        student.SmsOrEmail = request.SmsOrEmail;
        student.SchoolClose = request.SchoolClose;
        student.Cnt = request.Cnt;
        student.OnlineEntry = request.OnlineEntry;
        student.PhotoUrl = request.PhotoUrl;
        student.IsActive = request.IsActive;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated student with ID {StudentId}", id);

        return await GetByIdAsync(id);
    }

    /// <summary>
    /// Archives (soft-deletes) a student.
    /// </summary>
    public async Task<bool> ArchiveAsync(int id)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (student == null)
        {
            _logger.LogWarning("Cannot archive: Student with ID {StudentId} not found", id);
            return false;
        }

        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived student with ID {StudentId}", id);
        return true;
    }

    /// <summary>
    /// Searches for students by name (case-insensitive contains search).
    /// </summary>
    public async Task<List<StudentSearchResultDto>> SearchAsync(string query, int limit = 10)
    {
        // Enforce reasonable limit to prevent DoS
        const int maxLimit = 50;
        if (limit < 1) limit = 10;
        if (limit > maxLimit) limit = maxLimit;

        // Escape LIKE special characters to prevent SQL injection
        var escapedQuery = query.Replace("%", "\\%")
                               .Replace("_", "\\_")
                               .Replace("[", "\\[")
                               .Replace("]", "\\]");

        var results = await _context.Students
            .Include(s => s.School)
            .Include(s => s.ClassGroup)
            .Where(s => s.IsActive &&
                       ((s.FirstName != null && EF.Functions.Like(s.FirstName, $"%{escapedQuery}%")) ||
                        (s.LastName != null && EF.Functions.Like(s.LastName, $"%{escapedQuery}%"))))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Take(limit)
            .AsNoTracking()
            .Select(s => new StudentSearchResultDto
            {
                Id = s.Id,
                FullName = $"{s.FirstName} {s.LastName}".Trim(),
                SchoolName = s.School != null ? s.School.Name : "No School",
                Grade = s.Grade ?? "No Grade",
                ClassGroupName = s.ClassGroup != null ? s.ClassGroup.Name : "No Class"
            })
            .ToListAsync();

        _logger.LogInformation("Student search for '{Query}' returned {Count} results", query, results.Count);

        return results;
    }

    private static StudentDto MapToDto(Student s)
    {
        return new StudentDto
        {
            Id = s.Id,
            Reference = s.Reference,
            FirstName = s.FirstName,
            LastName = s.LastName,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            Language = s.Language,
            AccountPersonName = s.AccountPersonName,
            AccountPersonSurname = s.AccountPersonSurname,
            AccountPersonIdNumber = s.AccountPersonIdNumber,
            AccountPersonCellphone = s.AccountPersonCellphone,
            AccountPersonOffice = s.AccountPersonOffice,
            AccountPersonHome = s.AccountPersonHome,
            AccountPersonEmail = s.AccountPersonEmail,
            Relation = s.Relation,
            MotherName = s.MotherName,
            MotherSurname = s.MotherSurname,
            MotherOffice = s.MotherOffice,
            MotherCell = s.MotherCell,
            MotherHome = s.MotherHome,
            MotherEmail = s.MotherEmail,
            FatherName = s.FatherName,
            FatherSurname = s.FatherSurname,
            FatherOffice = s.FatherOffice,
            FatherCell = s.FatherCell,
            FatherHome = s.FatherHome,
            FatherEmail = s.FatherEmail,
            Address1 = s.Address1,
            Address2 = s.Address2,
            PostalCode = s.PostalCode,
            SchoolName = s.SchoolName,
            SchoolId = s.SchoolId,
            ClassGroupCode = s.ClassGroupCode,
            ClassGroupId = s.ClassGroupId,
            Grade = s.Grade,
            Teacher = s.Teacher,
            AttendingKcowAt = s.AttendingKcowAt,
            Aftercare = s.Aftercare,
            Extra = s.Extra,
            HomeTime = s.HomeTime,
            StartClasses = s.StartClasses,
            Terms = s.Terms,
            Seat = s.Seat,
            Truck = s.Truck,
            Family = s.Family,
            Sequence = s.Sequence,
            FinancialCode = s.FinancialCode,
            Charge = s.Charge,
            Deposit = s.Deposit,
            PayDate = s.PayDate,
            TshirtCode = s.TshirtCode,
            TshirtMoney1 = s.TshirtMoney1,
            TshirtMoneyDate1 = s.TshirtMoneyDate1,
            TshirtReceived1 = s.TshirtReceived1,
            TshirtRecDate1 = s.TshirtRecDate1,
            ReceiveNote1 = s.ReceiveNote1,
            TshirtSize1 = s.TshirtSize1,
            TshirtColor1 = s.TshirtColor1,
            TshirtDesign1 = s.TshirtDesign1,
            TshirtSize2 = s.TshirtSize2,
            TshirtMoney2 = s.TshirtMoney2,
            TshirtMoneyDate2 = s.TshirtMoneyDate2,
            TshirtReceived2 = s.TshirtReceived2,
            TshirtRecDate2 = s.TshirtRecDate2,
            ReceiveNote2 = s.ReceiveNote2,
            TshirtColor2 = s.TshirtColor2,
            TshirtDesign2 = s.TshirtDesign2,
            Indicator1 = s.Indicator1,
            Indicator2 = s.Indicator2,
            GeneralNote = s.GeneralNote,
            PrintIdCard = s.PrintIdCard,
            AcceptTermsCond = s.AcceptTermsCond,
            Status = s.Status,
            SmsOrEmail = s.SmsOrEmail,
            SchoolClose = s.SchoolClose,
            Cnt = s.Cnt,
            OnlineEntry = s.OnlineEntry,
            LegacyCreated = s.LegacyCreated,
            Submitted = s.Submitted,
            LegacyUpdated = s.LegacyUpdated,
            BookEmail = s.BookEmail,
            Report1GivenOut = s.Report1GivenOut,
            AccountGivenOut = s.AccountGivenOut,
            CertificatePrinted = s.CertificatePrinted,
            Report2GivenOut = s.Report2GivenOut,
            Social = s.Social,
            ActivityReportGivenOut = s.ActivityReportGivenOut,
            PhotoUrl = s.PhotoUrl,
            PhotoUpdated = s.PhotoUpdated,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            School = s.School != null ? new SchoolDto { Id = s.School.Id, Name = s.School.Name } : null,
            ClassGroup = s.ClassGroup != null ? new ClassGroupDto { Id = s.ClassGroup.Id, Name = s.ClassGroup.Name } : null
        };
    }
}
