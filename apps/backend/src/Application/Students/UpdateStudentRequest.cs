using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Students;

/// <summary>
/// Request model for updating an existing student.
/// </summary>
public class UpdateStudentRequest
{
    [Required(ErrorMessage = "Reference is required")]
    [MaxLength(10, ErrorMessage = "Reference cannot exceed 10 characters")]
    public required string Reference { get; set; }

    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(3, ErrorMessage = "Gender cannot exceed 3 characters")]
    public string? Gender { get; set; }

    [MaxLength(3, ErrorMessage = "Language cannot exceed 3 characters")]
    public string? Language { get; set; }

    [MaxLength(50, ErrorMessage = "Account person name cannot exceed 50 characters")]
    public string? AccountPersonName { get; set; }

    [MaxLength(50, ErrorMessage = "Account person surname cannot exceed 50 characters")]
    public string? AccountPersonSurname { get; set; }

    [MaxLength(20, ErrorMessage = "Account person ID number cannot exceed 20 characters")]
    public string? AccountPersonIdNumber { get; set; }

    [MaxLength(20, ErrorMessage = "Account person cellphone cannot exceed 20 characters")]
    public string? AccountPersonCellphone { get; set; }

    [MaxLength(20, ErrorMessage = "Account person office cannot exceed 20 characters")]
    public string? AccountPersonOffice { get; set; }

    [MaxLength(20, ErrorMessage = "Account person home cannot exceed 20 characters")]
    public string? AccountPersonHome { get; set; }

    [MaxLength(100, ErrorMessage = "Account person email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? AccountPersonEmail { get; set; }

    [MaxLength(20, ErrorMessage = "Relation cannot exceed 20 characters")]
    public string? Relation { get; set; }

    [MaxLength(50, ErrorMessage = "Mother name cannot exceed 50 characters")]
    public string? MotherName { get; set; }

    [MaxLength(50, ErrorMessage = "Mother surname cannot exceed 50 characters")]
    public string? MotherSurname { get; set; }

    [MaxLength(20, ErrorMessage = "Mother office cannot exceed 20 characters")]
    public string? MotherOffice { get; set; }

    [MaxLength(20, ErrorMessage = "Mother cell cannot exceed 20 characters")]
    public string? MotherCell { get; set; }

    [MaxLength(20, ErrorMessage = "Mother home cannot exceed 20 characters")]
    public string? MotherHome { get; set; }

    [MaxLength(100, ErrorMessage = "Mother email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? MotherEmail { get; set; }

    [MaxLength(50, ErrorMessage = "Father name cannot exceed 50 characters")]
    public string? FatherName { get; set; }

    [MaxLength(50, ErrorMessage = "Father surname cannot exceed 50 characters")]
    public string? FatherSurname { get; set; }

    [MaxLength(20, ErrorMessage = "Father office cannot exceed 20 characters")]
    public string? FatherOffice { get; set; }

    [MaxLength(20, ErrorMessage = "Father cell cannot exceed 20 characters")]
    public string? FatherCell { get; set; }

    [MaxLength(20, ErrorMessage = "Father home cannot exceed 20 characters")]
    public string? FatherHome { get; set; }

    [MaxLength(100, ErrorMessage = "Father email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? FatherEmail { get; set; }

    [MaxLength(50, ErrorMessage = "Address 1 cannot exceed 50 characters")]
    public string? Address1 { get; set; }

    [MaxLength(50, ErrorMessage = "Address 2 cannot exceed 50 characters")]
    public string? Address2 { get; set; }

    [MaxLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
    public string? PostalCode { get; set; }

    [MaxLength(50, ErrorMessage = "School name cannot exceed 50 characters")]
    public string? SchoolName { get; set; }

    public int? SchoolId { get; set; }

    [MaxLength(10, ErrorMessage = "Class group code cannot exceed 10 characters")]
    public string? ClassGroupCode { get; set; }

    public int? ClassGroupId { get; set; }

    [MaxLength(5, ErrorMessage = "Grade cannot exceed 5 characters")]
    public string? Grade { get; set; }

    [MaxLength(50, ErrorMessage = "Teacher cannot exceed 50 characters")]
    public string? Teacher { get; set; }

    [MaxLength(50, ErrorMessage = "Attending KCOW at cannot exceed 50 characters")]
    public string? AttendingKcowAt { get; set; }

    [MaxLength(50, ErrorMessage = "Aftercare cannot exceed 50 characters")]
    public string? Aftercare { get; set; }

    [MaxLength(50, ErrorMessage = "Extra cannot exceed 50 characters")]
    public string? Extra { get; set; }

    public DateTime? HomeTime { get; set; }

    public DateTime? StartClasses { get; set; }

    [MaxLength(10, ErrorMessage = "Terms cannot exceed 10 characters")]
    public string? Terms { get; set; }

    [MaxLength(5, ErrorMessage = "Seat cannot exceed 5 characters")]
    public string? Seat { get; set; }

    [MaxLength(3, ErrorMessage = "Truck cannot exceed 3 characters")]
    public string? Truck { get; set; }

    [MaxLength(50, ErrorMessage = "Family cannot exceed 50 characters")]
    public string? Family { get; set; }

    [MaxLength(50, ErrorMessage = "Sequence cannot exceed 50 characters")]
    public string? Sequence { get; set; }

    [MaxLength(10, ErrorMessage = "Financial code cannot exceed 10 characters")]
    public string? FinancialCode { get; set; }

    public decimal? Charge { get; set; }

    [MaxLength(50, ErrorMessage = "Deposit cannot exceed 50 characters")]
    public string? Deposit { get; set; }

    [MaxLength(10, ErrorMessage = "Pay date cannot exceed 10 characters")]
    public string? PayDate { get; set; }

    [MaxLength(4, ErrorMessage = "T-Shirt code cannot exceed 4 characters")]
    public string? TshirtCode { get; set; }

    [MaxLength(30, ErrorMessage = "T-Shirt money 1 cannot exceed 30 characters")]
    public string? TshirtMoney1 { get; set; }

    public DateTime? TshirtMoneyDate1 { get; set; }

    [MaxLength(30, ErrorMessage = "T-Shirt received 1 cannot exceed 30 characters")]
    public string? TshirtReceived1 { get; set; }

    public DateTime? TshirtRecDate1 { get; set; }

    [MaxLength(255, ErrorMessage = "Receive note 1 cannot exceed 255 characters")]
    public string? ReceiveNote1 { get; set; }

    [MaxLength(10, ErrorMessage = "T-Shirt size 1 cannot exceed 10 characters")]
    public string? TshirtSize1 { get; set; }

    [MaxLength(20, ErrorMessage = "T-Shirt color 1 cannot exceed 20 characters")]
    public string? TshirtColor1 { get; set; }

    [MaxLength(20, ErrorMessage = "T-Shirt design 1 cannot exceed 20 characters")]
    public string? TshirtDesign1 { get; set; }

    [MaxLength(10, ErrorMessage = "T-Shirt size 2 cannot exceed 10 characters")]
    public string? TshirtSize2 { get; set; }

    [MaxLength(30, ErrorMessage = "T-Shirt money 2 cannot exceed 30 characters")]
    public string? TshirtMoney2 { get; set; }

    public DateTime? TshirtMoneyDate2 { get; set; }

    [MaxLength(30, ErrorMessage = "T-Shirt received 2 cannot exceed 30 characters")]
    public string? TshirtReceived2 { get; set; }

    public DateTime? TshirtRecDate2 { get; set; }

    [MaxLength(255, ErrorMessage = "Receive note 2 cannot exceed 255 characters")]
    public string? ReceiveNote2 { get; set; }

    [MaxLength(20, ErrorMessage = "T-Shirt color 2 cannot exceed 20 characters")]
    public string? TshirtColor2 { get; set; }

    [MaxLength(20, ErrorMessage = "T-Shirt design 2 cannot exceed 20 characters")]
    public string? TshirtDesign2 { get; set; }

    [MaxLength(3, ErrorMessage = "Indicator 1 cannot exceed 3 characters")]
    public string? Indicator1 { get; set; }

    [MaxLength(3, ErrorMessage = "Indicator 2 cannot exceed 3 characters")]
    public string? Indicator2 { get; set; }

    [MaxLength(255, ErrorMessage = "General note cannot exceed 255 characters")]
    public string? GeneralNote { get; set; }

    public bool PrintIdCard { get; set; }

    [MaxLength(50, ErrorMessage = "Accept terms cond cannot exceed 50 characters")]
    public string? AcceptTermsCond { get; set; }

    [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string? Status { get; set; }

    [MaxLength(255, ErrorMessage = "SMS or Email cannot exceed 255 characters")]
    public string? SmsOrEmail { get; set; }

    public DateTime? SchoolClose { get; set; }

    public float? Cnt { get; set; }

    public int? OnlineEntry { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsActive { get; set; }
}
