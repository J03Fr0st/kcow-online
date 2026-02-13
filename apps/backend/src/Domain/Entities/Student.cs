namespace Kcow.Domain.Entities;

/// <summary>
/// Student entity aligned with legacy XSD schema (Children.xsd).
/// Represents a student enrolled in the KCOW program with personal, family, enrollment, and financial information.
/// </summary>
public class Student
{
    // Primary identifier (auto-generated)
    public int Id { get; set; }

    // XSD Field: "Reference" (10 chars max, required) - Unique reference code
    public string Reference { get; set; } = string.Empty;

    // XSD Field: "Child_Name" (50 chars max) - Renamed to FirstName for clarity
    public string? FirstName { get; set; }

    // XSD Field: "Child_Surname" (50 chars max) - Renamed to LastName for clarity
    public string? LastName { get; set; }

    // XSD Field: "Child_birthdate" (datetime)
    public DateTime? DateOfBirth { get; set; }

    // XSD Field: "Sex" (3 chars max) - Gender (M/F)
    public string? Gender { get; set; }

    // XSD Field: "Language" (3 chars max) - Language preference (Afr/Eng)
    public string? Language { get; set; }

    // Account Person Fields (Responsible Adult)
    // XSD Field: "Account_Person_Name" (50 chars max)
    public string? AccountPersonName { get; set; }

    // XSD Field: "Account_Person_Surname" (50 chars max)
    public string? AccountPersonSurname { get; set; }

    // XSD Field: "Account_Person_Idnumber" (20 chars max)
    public string? AccountPersonIdNumber { get; set; }

    // XSD Field: "Account_Person_Cellphone" (20 chars max)
    public string? AccountPersonCellphone { get; set; }

    // XSD Field: "Account_Person_Office" (20 chars max)
    public string? AccountPersonOffice { get; set; }

    // XSD Field: "Account_Person_Home" (20 chars max)
    public string? AccountPersonHome { get; set; }

    // XSD Field: "Account_Person_Email" (100 chars max)
    public string? AccountPersonEmail { get; set; }

    // XSD Field: "Relation" (20 chars max)
    public string? Relation { get; set; }

    // Mother's Details
    // XSD Field: "Mother_Name" (50 chars max)
    public string? MotherName { get; set; }

    // XSD Field: "Mother_Surname" (50 chars max)
    public string? MotherSurname { get; set; }

    // XSD Field: "Mother_Office" (20 chars max)
    public string? MotherOffice { get; set; }

    // XSD Field: "Mother_Cell" (20 chars max)
    public string? MotherCell { get; set; }

    // XSD Field: "Mother_Home" (20 chars max)
    public string? MotherHome { get; set; }

    // XSD Field: "Mother_Email" (100 chars max)
    public string? MotherEmail { get; set; }

    // Father's Details
    // XSD Field: "Father_Name" (50 chars max)
    public string? FatherName { get; set; }

    // XSD Field: "Father_Surname" (50 chars max)
    public string? FatherSurname { get; set; }

    // XSD Field: "Father_Office" (20 chars max)
    public string? FatherOffice { get; set; }

    // XSD Field: "Father_Cell" (20 chars max)
    public string? FatherCell { get; set; }

    // XSD Field: "Father_Home" (20 chars max)
    public string? FatherHome { get; set; }

    // XSD Field: "Father_Email" (100 chars max)
    public string? FatherEmail { get; set; }

    // Address Fields
    // XSD Field: "Address1" (50 chars max)
    public string? Address1 { get; set; }

    // XSD Field: "Address2" (50 chars max)
    public string? Address2 { get; set; }

    // XSD Field: "Code" (10 chars max) - Postal code
    public string? PostalCode { get; set; }

    // Enrollment Fields
    // XSD Field: "School_Name" (50 chars max) - Denormalized school name
    public string? SchoolName { get; set; }

    // Foreign key to School entity
    public int? SchoolId { get; set; }

    // XSD Field: "Class_Group" (10 chars max) - Class group code
    public string? ClassGroupCode { get; set; }

    // Foreign key to ClassGroup entity
    public int? ClassGroupId { get; set; }

    // XSD Field: "Grade" (5 chars max)
    public string? Grade { get; set; }

    // XSD Field: "Teacher" (50 chars max)
    public string? Teacher { get; set; }

    // XSD Field: "Attending_KCOW_at" (50 chars max)
    public string? AttendingKcowAt { get; set; }

    // XSD Field: "Aftercare" (50 chars max)
    public string? Aftercare { get; set; }

    // XSD Field: "Extra" (50 chars max)
    public string? Extra { get; set; }

    // XSD Field: "Home_Time" (datetime)
    public DateTime? HomeTime { get; set; }

    // XSD Field: "Start_Classes" (datetime)
    public DateTime? StartClasses { get; set; }

    // XSD Field: "Terms" (10 chars max)
    public string? Terms { get; set; }

    // XSD Field: "Seat" (5 chars max)
    public string? Seat { get; set; }

    // XSD Field: "Truck" (3 chars max)
    public string? Truck { get; set; }

    // XSD Field: "Family" (50 chars max) - Family grouping code
    public string? Family { get; set; }

    // XSD Field: "Sequence" (50 chars max)
    public string? Sequence { get; set; }

    // Financial Fields
    // XSD Field: "Financial_Code" (10 chars max)
    public string? FinancialCode { get; set; }

    // XSD Field: "Charge" (money)
    public decimal? Charge { get; set; }

    // XSD Field: "Deposit" (50 chars max)
    public string? Deposit { get; set; }

    // XSD Field: "PayDate" (50 chars max)
    public string? PayDate { get; set; }

    // T-Shirt Order Fields (Set 1)
    // XSD Field: "Tshirt_Code" (5 chars max, required)
    public string? TshirtCode { get; set; }

    // XSD Field: "Tshirt_Money_1" (30 chars max)
    public string? TshirtMoney1 { get; set; }

    // XSD Field: "Tshirt_MoneyDate_1" (datetime)
    public DateTime? TshirtMoneyDate1 { get; set; }

    // XSD Field: "Tshirt_Received_1" (30 chars max)
    public string? TshirtReceived1 { get; set; }

    // XSD Field: "Tshirt_RecDate_1" (datetime)
    public DateTime? TshirtRecDate1 { get; set; }

    // XSD Field: "Receive_Note_1" (255 chars max)
    public string? ReceiveNote1 { get; set; }

    // XSD Field: "TshirtSize1" (10 chars max)
    public string? TshirtSize1 { get; set; }

    // XSD Field: "TshirtColor1" (20 chars max)
    public string? TshirtColor1 { get; set; }

    // XSD Field: "TshirtDesign1" (20 chars max)
    public string? TshirtDesign1 { get; set; }

    // T-Shirt Order Fields (Set 2)
    // XSD Field: "TshirtSize2" (10 chars max)
    public string? TshirtSize2 { get; set; }

    // XSD Field: "Tshirt_Money_2" (30 chars max)
    public string? TshirtMoney2 { get; set; }

    // XSD Field: "Tshirt_MoneyDate_2" (datetime)
    public DateTime? TshirtMoneyDate2 { get; set; }

    // XSD Field: "Tshirt_Received_2" (30 chars max)
    public string? TshirtReceived2 { get; set; }

    // XSD Field: "Tshirt_RecDate_2" (datetime)
    public DateTime? TshirtRecDate2 { get; set; }

    // XSD Field: "Receive_Note_2" (255 chars max)
    public string? ReceiveNote2 { get; set; }

    // XSD Field: "TshirtColor2" (20 chars max)
    public string? TshirtColor2 { get; set; }

    // XSD Field: "TshirtDesign2" (20 chars max)
    public string? TshirtDesign2 { get; set; }

    // Status & Tracking Fields
    // XSD Field: "Indicator_1" (3 chars max)
    public string? Indicator1 { get; set; }

    // XSD Field: "Indicator_2" (3 chars max)
    public string? Indicator2 { get; set; }

    // XSD Field: "General_Note" (255 chars max)
    public string? GeneralNote { get; set; }

    // XSD Field: "Print_Id_Card" (bit, required, default false)
    public bool PrintIdCard { get; set; } = false;

    // XSD Field: "AcceptTermsCond" (50 chars max)
    public string? AcceptTermsCond { get; set; }

    // XSD Field: "Status" (20 chars max)
    public string? Status { get; set; }

    // XSD Field: "SmsOrEmail" (10 chars max) - Contact preference
    public string? SmsOrEmail { get; set; }

    // XSD Field: "SchoolClose" (datetime, used as time in legacy)
    public DateTime? SchoolClose { get; set; }

    // XSD Field: "Cnt" (float)
    public float? Cnt { get; set; }

    // XSD Field: "OnlineEntry" (int)
    public int? OnlineEntry { get; set; }

    // XSD Field: "Created" (datetime) - Legacy timestamp (distinct from CreatedAt)
    public DateTime? LegacyCreated { get; set; }

    // XSD Field: "Submitted" (datetime)
    public DateTime? Submitted { get; set; }

    // XSD Field: "Updated" (datetime) - Legacy timestamp (distinct from UpdatedAt)
    public DateTime? LegacyUpdated { get; set; }

    // XSD Field: "BookEmail" (255 chars max)
    public string? BookEmail { get; set; }

    // XSD Field: "Report1GivenOut" (255 chars max)
    public string? Report1GivenOut { get; set; }

    // XSD Field: "AccountGivenOut" (255 chars max)
    public string? AccountGivenOut { get; set; }

    // XSD Field: "CertificatePrinted" (255 chars max)
    public string? CertificatePrinted { get; set; }

    // XSD Field: "Report2GivenOut" (255 chars max)
    public string? Report2GivenOut { get; set; }

    // XSD Field: "Social" (255 chars max)
    public string? Social { get; set; }

    // XSD Field: "ActivityReportGivenOut" (255 chars max)
    public string? ActivityReportGivenOut { get; set; }

    // XSD Field: "Photo" (attachment) - Stored as URL/path
    public string? PhotoUrl { get; set; }

    // XSD Field: "PhotoUpdated" (datetime)
    public DateTime? PhotoUpdated { get; set; }

    // Soft delete flag (not in XSD, application-level)
    public bool IsActive { get; set; } = true;

    public string? LegacyId { get; set; }

    // Audit fields (not in XSD, application-level)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public School? School { get; set; }
    public ClassGroup? ClassGroup { get; set; }
    public ICollection<StudentFamily> StudentFamilies { get; set; } = new List<StudentFamily>();
}
