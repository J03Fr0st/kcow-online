using Kcow.Domain.Entities;
using Kcow.Domain.Enums;
using System.Globalization;

namespace Kcow.Application.Import;

public sealed record LegacyChildMappingResult(
    Student? Student,
    FamilyInfo? FamilyInfo,
    IReadOnlyList<string> Warnings);

public sealed record FamilyInfo(
    string FamilyName,
    string PrimaryContactName,
    string? Phone,
    string? Email,
    string? Address);

public sealed class LegacyChildMapper
{
    private readonly IReadOnlyDictionary<string, int> _schoolIdsByName;
    private readonly IReadOnlyDictionary<string, int> _classGroupIdsByCode;

    public LegacyChildMapper(
        IReadOnlyDictionary<string, int> schoolIdsByName,
        IReadOnlyDictionary<string, int> classGroupIdsByCode)
    {
        _schoolIdsByName = schoolIdsByName;
        _classGroupIdsByCode = classGroupIdsByCode;
    }

    public LegacyChildMappingResult Map(LegacyChildRecord record)
    {
        var warnings = new List<string>();

        // Skip if no reference (required field)
        if (string.IsNullOrWhiteSpace(record.Reference))
        {
            warnings.Add("Student has no Reference - skipping import.");
            return new LegacyChildMappingResult(null, null, warnings);
        }

        // Map SchoolId
        int? schoolId = null;
        if (!string.IsNullOrWhiteSpace(record.SchoolName))
        {
            if (_schoolIdsByName.TryGetValue(record.SchoolName, out var sid))
            {
                schoolId = sid;
            }
            else
            {
                warnings.Add($"School '{record.SchoolName}' not found in database.");
            }
        }

        // Map ClassGroupId
        int? classGroupId = null;
        if (!string.IsNullOrWhiteSpace(record.ClassGroup))
        {
            if (_classGroupIdsByCode.TryGetValue(record.ClassGroup, out var cid))
            {
                classGroupId = cid;
            }
            else
            {
                warnings.Add($"ClassGroup '{record.ClassGroup}' not found in database.");
            }
        }

        var student = new Student
        {
            // Basic Info
            Reference = record.Reference,
            FirstName = record.ChildName,
            LastName = record.ChildSurname,
            DateOfBirth = ParseDateTime(record.ChildBirthdate),
            Gender = record.Sex,
            Language = record.Language,

            // Account Person
            AccountPersonName = record.AccountPersonName,
            AccountPersonSurname = record.AccountPersonSurname,
            AccountPersonIdNumber = record.AccountPersonIdnumber,
            AccountPersonCellphone = record.AccountPersonCellphone,
            AccountPersonOffice = record.AccountPersonOffice,
            AccountPersonHome = record.AccountPersonHome,
            AccountPersonEmail = record.AccountPersonEmail,
            Relation = record.Relation,

            // Mother
            MotherName = record.MotherName,
            MotherSurname = record.MotherSurname,
            MotherOffice = record.MotherOffice,
            MotherCell = record.MotherCell,
            MotherHome = record.MotherHome,
            MotherEmail = record.MotherEmail,

            // Father
            FatherName = record.FatherName,
            FatherSurname = record.FatherSurname,
            FatherOffice = record.FatherOffice,
            FatherCell = record.FatherCell,
            FatherHome = record.FatherHome,
            FatherEmail = record.FatherEmail,

            // Address
            Address1 = record.Address1,
            Address2 = record.Address2,
            PostalCode = record.Code,

            // Enrollment
            SchoolName = record.SchoolName,
            SchoolId = schoolId,
            ClassGroupCode = record.ClassGroup,
            ClassGroupId = classGroupId,
            Grade = record.Grade,
            Teacher = record.Teacher,
            AttendingKcowAt = record.AttendingKcowAt,
            Aftercare = record.Aftercare,
            Extra = record.Extra,
            HomeTime = ParseDateTime(record.HomeTime),
            StartClasses = ParseDateTime(record.StartClasses),
            Terms = record.Terms,
            Seat = record.Seat,
            Truck = record.Truck,
            Family = record.Family,
            Sequence = record.Sequence,

            // Financial
            FinancialCode = record.FinancialCode,
            Charge = ParseDecimal(record.Charge),
            Deposit = record.Deposit,
            PayDate = record.PayDate,

            // T-Shirt 1
            TshirtCode = record.TshirtCode,
            TshirtMoney1 = record.TshirtMoney1,
            TshirtMoneyDate1 = ParseDateTime(record.TshirtMoneyDate1),
            TshirtReceived1 = record.TshirtReceived1,
            TshirtRecDate1 = ParseDateTime(record.TshirtRecDate1),
            ReceiveNote1 = record.ReceiveNote1,
            TshirtSize1 = record.TshirtSize1,
            TshirtColor1 = record.TshirtColor1,
            TshirtDesign1 = record.TshirtDesign1,

            // T-Shirt 2
            TshirtSize2 = record.TshirtSize2,
            TshirtMoney2 = record.TshirtMoney2,
            TshirtMoneyDate2 = ParseDateTime(record.TshirtMoneyDate2),
            TshirtReceived2 = record.TshirtReceived2,
            TshirtRecDate2 = ParseDateTime(record.TshirtRecDate2),
            ReceiveNote2 = record.ReceiveNote2,
            TshirtColor2 = record.TshirtColor2,
            TshirtDesign2 = record.TshirtDesign2,

            // Status
            Indicator1 = record.Indicator1,
            Indicator2 = record.Indicator2,
            GeneralNote = record.GeneralNote,
            PrintIdCard = ParseBool(record.PrintIdCard),
            AcceptTermsCond = record.AcceptTermsCond,
            Status = record.Status,
            SmsOrEmail = record.SmsOrEmail,
            SchoolClose = ParseDateTime(record.SchoolClose),
            Cnt = ParseFloat(record.Cnt),
            OnlineEntry = ParseInt(record.OnlineEntry),
            LegacyCreated = ParseDateTime(record.Created),
            Submitted = ParseDateTime(record.Submitted),
            LegacyUpdated = ParseDateTime(record.Updated),
            BookEmail = record.BookEmail,
            Report1GivenOut = record.Report1GivenOut,
            AccountGivenOut = record.AccountGivenOut,
            CertificatePrinted = record.CertificatePrinted,
            Report2GivenOut = record.Report2GivenOut,
            Social = record.Social,
            ActivityReportGivenOut = record.ActivityReportGivenOut,
            PhotoUrl = record.Photo,
            PhotoUpdated = ParseDateTime(record.PhotoUpdated),

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Extract family information if present
        FamilyInfo? familyInfo = null;
        if (!string.IsNullOrWhiteSpace(record.Family))
        {
            // Family format: "FAITH LEDWABA/SELLO LEDWABA"
            // Use the family string as family name, extract primary contact
            var familyName = record.Family;
            var primaryContact = record.AccountPersonName ?? record.MotherName ?? record.FatherName ?? string.Empty;

            // Use best available phone
            var phone = record.AccountPersonCellphone ?? record.MotherCell ?? record.FatherCell;
            var email = record.AccountPersonEmail ?? record.MotherEmail ?? record.FatherEmail;

            // Build address
            var addressParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(record.Address1)) addressParts.Add(record.Address1);
            if (!string.IsNullOrWhiteSpace(record.Address2)) addressParts.Add(record.Address2);
            if (!string.IsNullOrWhiteSpace(record.Code)) addressParts.Add(record.Code);
            var address = addressParts.Count > 0 ? string.Join(", ", addressParts) : null;

            familyInfo = new FamilyInfo(
                familyName,
                primaryContact,
                phone,
                email,
                address);
        }

        return new LegacyChildMappingResult(student, familyInfo, warnings);
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        // Try various date formats
        var formats = new[] { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var fallbackResult))
        {
            return fallbackResult;
        }

        return null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return null;
    }

    private static float? ParseFloat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return null;
    }

    private static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        return null;
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.Trim();
        if (normalized == "1" || normalized.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        if (normalized == "0" || normalized.Equals("false", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (bool.TryParse(normalized, out var result))
        {
            return result;
        }
        return false;
    }
}
