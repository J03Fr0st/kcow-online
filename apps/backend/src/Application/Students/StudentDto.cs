namespace Kcow.Application.Students;

/// <summary>
/// Data transfer object for student information.
/// Includes all XSD schema fields for legacy compatibility.
/// </summary>
public class StudentDto
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Language { get; set; }

    // Account Person Fields
    public string? AccountPersonName { get; set; }
    public string? AccountPersonSurname { get; set; }
    public string? AccountPersonIdNumber { get; set; }
    public string? AccountPersonCellphone { get; set; }
    public string? AccountPersonOffice { get; set; }
    public string? AccountPersonHome { get; set; }
    public string? AccountPersonEmail { get; set; }
    public string? Relation { get; set; }

    // Mother's Details
    public string? MotherName { get; set; }
    public string? MotherSurname { get; set; }
    public string? MotherOffice { get; set; }
    public string? MotherCell { get; set; }
    public string? MotherHome { get; set; }
    public string? MotherEmail { get; set; }

    // Father's Details
    public string? FatherName { get; set; }
    public string? FatherSurname { get; set; }
    public string? FatherOffice { get; set; }
    public string? FatherCell { get; set; }
    public string? FatherHome { get; set; }
    public string? FatherEmail { get; set; }

    // Address Fields
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? PostalCode { get; set; }

    // Enrollment Fields
    public string? SchoolName { get; set; }
    public int? SchoolId { get; set; }
    public string? ClassGroupCode { get; set; }
    public int? ClassGroupId { get; set; }
    public string? Grade { get; set; }
    public string? Teacher { get; set; }
    public string? AttendingKcowAt { get; set; }
    public string? Aftercare { get; set; }
    public string? Extra { get; set; }
    public DateTime? HomeTime { get; set; }
    public DateTime? StartClasses { get; set; }
    public string? Terms { get; set; }
    public string? Seat { get; set; }
    public string? Truck { get; set; }
    public string? Family { get; set; }
    public string? Sequence { get; set; }

    // Financial Fields
    public string? FinancialCode { get; set; }
    public decimal? Charge { get; set; }
    public string? Deposit { get; set; }
    public string? PayDate { get; set; }

    // T-Shirt Order Fields
    public string? TshirtCode { get; set; }
    public string? TshirtMoney1 { get; set; }
    public DateTime? TshirtMoneyDate1 { get; set; }
    public string? TshirtReceived1 { get; set; }
    public DateTime? TshirtRecDate1 { get; set; }
    public string? ReceiveNote1 { get; set; }
    public string? TshirtSize1 { get; set; }
    public string? TshirtColor1 { get; set; }
    public string? TshirtDesign1 { get; set; }
    public string? TshirtSize2 { get; set; }
    public string? TshirtMoney2 { get; set; }
    public DateTime? TshirtMoneyDate2 { get; set; }
    public string? TshirtReceived2 { get; set; }
    public DateTime? TshirtRecDate2 { get; set; }
    public string? ReceiveNote2 { get; set; }
    public string? TshirtColor2 { get; set; }
    public string? TshirtDesign2 { get; set; }

    // Status & Tracking Fields
    public string? Indicator1 { get; set; }
    public string? Indicator2 { get; set; }
    public string? GeneralNote { get; set; }
    public bool PrintIdCard { get; set; }
    public string? AcceptTermsCond { get; set; }
    public string? Status { get; set; }
    public string? SmsOrEmail { get; set; }
    public DateTime? SchoolClose { get; set; }
    public float? Cnt { get; set; }
    public int? OnlineEntry { get; set; }
    public DateTime? LegacyCreated { get; set; }
    public DateTime? Submitted { get; set; }
    public DateTime? LegacyUpdated { get; set; }
    public string? BookEmail { get; set; }
    public string? Report1GivenOut { get; set; }
    public string? AccountGivenOut { get; set; }
    public string? CertificatePrinted { get; set; }
    public string? Report2GivenOut { get; set; }
    public string? Social { get; set; }
    public string? ActivityReportGivenOut { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime? PhotoUpdated { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Nested school and class group information
    public SchoolDto? School { get; set; }
    public ClassGroupDto? ClassGroup { get; set; }
}

/// <summary>
/// Lightweight school information for nested display.
/// </summary>
public class SchoolDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

/// <summary>
/// Lightweight class group information for nested display.
/// </summary>
public class ClassGroupDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
