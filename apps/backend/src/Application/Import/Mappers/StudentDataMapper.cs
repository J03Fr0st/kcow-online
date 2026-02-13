using Kcow.Domain.Entities;
using System.Globalization;

namespace Kcow.Application.Import.Mappers;

/// <summary>
/// Result of mapping a legacy child record to a Student entity.
/// Includes optional family information extracted from the record.
/// </summary>
public sealed record StudentMappingData(Student Student, FamilyInfo? FamilyInfo);

/// <summary>
/// Maps LegacyChildRecord to Student entity.
/// Mapping rules:
///   Reference → Reference (required, records without it are rejected)
///   ChildName → FirstName
///   ChildSurname → LastName
///   ChildBirthdate → DateOfBirth (parsed from multiple date formats)
///   Sex → Gender (direct copy)
///   SchoolName → SchoolId (looked up from school name dictionary)
///   ClassGroup → ClassGroupId (looked up from code dictionary)
///   Charge → Charge (parsed as decimal)
///   Family field → FamilyInfo record with contact/address details
/// </summary>
public sealed class StudentDataMapper : IDataMapper<LegacyChildRecord, StudentMappingData>
{
    private static readonly IReadOnlyDictionary<string, int> EmptyDict =
        new Dictionary<string, int>();

    private static readonly string[] DateFormats =
        { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };

    private readonly IReadOnlyDictionary<string, int> _schoolIdsByName;
    private readonly IReadOnlyDictionary<string, int> _classGroupIdsByCode;

    public StudentDataMapper()
        : this(EmptyDict, EmptyDict) { }

    public StudentDataMapper(
        IReadOnlyDictionary<string, int>? schoolIdsByName = null,
        IReadOnlyDictionary<string, int>? classGroupIdsByCode = null)
    {
        _schoolIdsByName = schoolIdsByName ?? EmptyDict;
        _classGroupIdsByCode = classGroupIdsByCode ?? EmptyDict;
    }

    public MappingResult<StudentMappingData> Map(LegacyChildRecord source)
    {
        if (string.IsNullOrWhiteSpace(source.Reference))
        {
            return MappingResult<StudentMappingData>.Fail("Reference",
                "Student has no Reference - skipping import.");
        }

        var result = new MappingResult<StudentMappingData> { Success = true };

        // Resolve SchoolId
        int? schoolId = null;
        if (!string.IsNullOrWhiteSpace(source.SchoolName))
        {
            if (_schoolIdsByName.TryGetValue(source.SchoolName, out var sid))
                schoolId = sid;
            else
                result.Warnings.Add(new MappingWarning("SchoolId",
                    $"School '{source.SchoolName}' not found in database.",
                    source.SchoolName, null));
        }

        // Resolve ClassGroupId
        int? classGroupId = null;
        if (!string.IsNullOrWhiteSpace(source.ClassGroup))
        {
            if (_classGroupIdsByCode.TryGetValue(source.ClassGroup, out var cid))
                classGroupId = cid;
            else
                result.Warnings.Add(new MappingWarning("ClassGroupId",
                    $"ClassGroup '{source.ClassGroup}' not found in database.",
                    source.ClassGroup, null));
        }

        // Parse DateOfBirth
        var dateOfBirth = ParseDateTime(source.ChildBirthdate);
        if (!string.IsNullOrWhiteSpace(source.ChildBirthdate) && dateOfBirth is null)
        {
            result.Warnings.Add(new MappingWarning("DateOfBirth",
                $"Could not parse date '{source.ChildBirthdate}'.",
                source.ChildBirthdate, null));
        }

        var student = new Student
        {
            Reference = source.Reference,
            FirstName = source.ChildName,
            LastName = source.ChildSurname,
            DateOfBirth = dateOfBirth,
            Gender = source.Sex,
            Language = source.Language,
            AccountPersonName = source.AccountPersonName,
            AccountPersonSurname = source.AccountPersonSurname,
            AccountPersonIdNumber = source.AccountPersonIdnumber,
            AccountPersonCellphone = source.AccountPersonCellphone,
            AccountPersonOffice = source.AccountPersonOffice,
            AccountPersonHome = source.AccountPersonHome,
            AccountPersonEmail = source.AccountPersonEmail,
            Relation = source.Relation,
            MotherName = source.MotherName,
            MotherSurname = source.MotherSurname,
            MotherOffice = source.MotherOffice,
            MotherCell = source.MotherCell,
            MotherHome = source.MotherHome,
            MotherEmail = source.MotherEmail,
            FatherName = source.FatherName,
            FatherSurname = source.FatherSurname,
            FatherOffice = source.FatherOffice,
            FatherCell = source.FatherCell,
            FatherHome = source.FatherHome,
            FatherEmail = source.FatherEmail,
            Address1 = source.Address1,
            Address2 = source.Address2,
            PostalCode = source.Code,
            SchoolName = source.SchoolName,
            SchoolId = schoolId,
            ClassGroupCode = source.ClassGroup,
            ClassGroupId = classGroupId,
            Grade = source.Grade,
            Teacher = source.Teacher,
            AttendingKcowAt = source.AttendingKcowAt,
            Aftercare = source.Aftercare,
            Extra = source.Extra,
            HomeTime = ParseDateTime(source.HomeTime),
            StartClasses = ParseDateTime(source.StartClasses),
            Terms = source.Terms,
            Seat = source.Seat,
            Truck = source.Truck,
            Family = source.Family,
            Sequence = source.Sequence,
            FinancialCode = source.FinancialCode,
            Charge = ParseDecimal(source.Charge),
            Deposit = source.Deposit,
            PayDate = source.PayDate,
            TshirtCode = source.TshirtCode,
            TshirtMoney1 = source.TshirtMoney1,
            TshirtMoneyDate1 = ParseDateTime(source.TshirtMoneyDate1),
            TshirtReceived1 = source.TshirtReceived1,
            TshirtRecDate1 = ParseDateTime(source.TshirtRecDate1),
            ReceiveNote1 = source.ReceiveNote1,
            TshirtSize1 = source.TshirtSize1,
            TshirtColor1 = source.TshirtColor1,
            TshirtDesign1 = source.TshirtDesign1,
            TshirtSize2 = source.TshirtSize2,
            TshirtMoney2 = source.TshirtMoney2,
            TshirtMoneyDate2 = ParseDateTime(source.TshirtMoneyDate2),
            TshirtReceived2 = source.TshirtReceived2,
            TshirtRecDate2 = ParseDateTime(source.TshirtRecDate2),
            ReceiveNote2 = source.ReceiveNote2,
            TshirtColor2 = source.TshirtColor2,
            TshirtDesign2 = source.TshirtDesign2,
            Indicator1 = source.Indicator1,
            Indicator2 = source.Indicator2,
            GeneralNote = source.GeneralNote,
            PrintIdCard = ParseBool(source.PrintIdCard),
            AcceptTermsCond = source.AcceptTermsCond,
            Status = source.Status,
            SmsOrEmail = source.SmsOrEmail,
            SchoolClose = ParseDateTime(source.SchoolClose),
            Cnt = ParseFloat(source.Cnt),
            OnlineEntry = ParseInt(source.OnlineEntry),
            LegacyCreated = ParseDateTime(source.Created),
            Submitted = ParseDateTime(source.Submitted),
            LegacyUpdated = ParseDateTime(source.Updated),
            BookEmail = source.BookEmail,
            Report1GivenOut = source.Report1GivenOut,
            AccountGivenOut = source.AccountGivenOut,
            CertificatePrinted = source.CertificatePrinted,
            Report2GivenOut = source.Report2GivenOut,
            Social = source.Social,
            ActivityReportGivenOut = source.ActivityReportGivenOut,
            PhotoUrl = source.Photo,
            PhotoUpdated = ParseDateTime(source.PhotoUpdated),
            IsActive = true,
            LegacyId = source.Reference,
            CreatedAt = DateTime.UtcNow
        };

        // Extract family info
        FamilyInfo? familyInfo = null;
        if (!string.IsNullOrWhiteSpace(source.Family))
        {
            var primaryContact = source.AccountPersonName ?? source.MotherName ?? source.FatherName ?? string.Empty;
            var phone = source.AccountPersonCellphone ?? source.MotherCell ?? source.FatherCell;
            var email = source.AccountPersonEmail ?? source.MotherEmail ?? source.FatherEmail;

            var addressParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(source.Address1)) addressParts.Add(source.Address1);
            if (!string.IsNullOrWhiteSpace(source.Address2)) addressParts.Add(source.Address2);
            if (!string.IsNullOrWhiteSpace(source.Code)) addressParts.Add(source.Code);
            var address = addressParts.Count > 0 ? string.Join(", ", addressParts) : null;

            familyInfo = new FamilyInfo(source.Family, primaryContact, phone, email, address);
        }

        result.Data = new StudentMappingData(student, familyInfo);
        return result;
    }

    public MappingResult<List<StudentMappingData>> MapMany(IEnumerable<LegacyChildRecord> sources)
    {
        var students = new List<StudentMappingData>();
        var allWarnings = new List<MappingWarning>();
        var allErrors = new List<MappingError>();

        foreach (var source in sources)
        {
            var result = Map(source);
            if (result.Success && result.Data is not null)
            {
                students.Add(result.Data);
            }
            allWarnings.AddRange(result.Warnings);
            allErrors.AddRange(result.Errors);
        }

        return new MappingResult<List<StudentMappingData>>
        {
            Data = students,
            Success = true,
            Warnings = allWarnings,
            Errors = allErrors
        };
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        foreach (var format in DateFormats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;
        }
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var fallback))
            return fallback;
        return null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static float? ParseFloat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.Trim();
        if (normalized is "1" || normalized.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return true;
        if (bool.TryParse(normalized, out var result))
            return result;
        return false;
    }
}
