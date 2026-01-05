using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Schools;

/// <summary>
/// Request model for updating an existing school.
/// </summary>
public class UpdateSchoolRequest
{
    /// <summary>
    /// School name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public required string Name { get; set; }

    [MaxLength(50, ErrorMessage = "Short name cannot exceed 50 characters")]
    public string? ShortName { get; set; }

    public int? TruckId { get; set; }

    public decimal? Price { get; set; }

    [MaxLength(255, ErrorMessage = "Fee description cannot exceed 255 characters")]
    public string? FeeDescription { get; set; }

    public decimal? Formula { get; set; }

    [MaxLength(50, ErrorMessage = "Visit day cannot exceed 50 characters")]
    public string? VisitDay { get; set; }

    [MaxLength(50, ErrorMessage = "Visit sequence cannot exceed 50 characters")]
    public string? VisitSequence { get; set; }

    [MaxLength(200, ErrorMessage = "Contact person cannot exceed 200 characters")]
    public string? ContactPerson { get; set; }

    [MaxLength(50, ErrorMessage = "Contact cell cannot exceed 50 characters")]
    public string? ContactCell { get; set; }

    [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [MaxLength(50, ErrorMessage = "Telephone cannot exceed 50 characters")]
    public string? Telephone { get; set; }

    [MaxLength(50, ErrorMessage = "Fax cannot exceed 50 characters")]
    public string? Fax { get; set; }

    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }

    [EmailAddress(ErrorMessage = "Circulars email must be a valid email address")]
    [MaxLength(255, ErrorMessage = "Circulars email cannot exceed 255 characters")]
    public string? CircularsEmail { get; set; }

    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [MaxLength(50, ErrorMessage = "Address 2 cannot exceed 50 characters")]
    public string? Address2 { get; set; }

    [MaxLength(50, ErrorMessage = "Headmaster cannot exceed 50 characters")]
    public string? Headmaster { get; set; }

    [MaxLength(50, ErrorMessage = "Headmaster cell cannot exceed 50 characters")]
    public string? HeadmasterCell { get; set; }

    [MaxLength(50, ErrorMessage = "Language cannot exceed 50 characters")]
    public string? Language { get; set; }

    public bool PrintInvoice { get; set; }

    public bool ImportFlag { get; set; }

    [MaxLength(255, ErrorMessage = "Afterschool 1 name cannot exceed 255 characters")]
    public string? Afterschool1Name { get; set; }

    [MaxLength(255, ErrorMessage = "Afterschool 1 contact cannot exceed 255 characters")]
    public string? Afterschool1Contact { get; set; }

    [MaxLength(255, ErrorMessage = "Afterschool 2 name cannot exceed 255 characters")]
    public string? Afterschool2Name { get; set; }

    [MaxLength(255, ErrorMessage = "Afterschool 2 contact cannot exceed 255 characters")]
    public string? Afterschool2Contact { get; set; }

    public string? SchedulingNotes { get; set; }

    public string? MoneyMessage { get; set; }

    public string? SafeNotes { get; set; }

    [MaxLength(500, ErrorMessage = "Web page cannot exceed 500 characters")]
    public string? WebPage { get; set; }

    [MaxLength(500, ErrorMessage = "KCOW web page link cannot exceed 500 characters")]
    public string? KcowWebPageLink { get; set; }
}
