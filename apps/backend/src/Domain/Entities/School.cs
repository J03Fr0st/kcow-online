namespace Kcow.Domain.Entities;

public class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? SchoolDescription { get; set; }
    public int? TruckId { get; set; }
    public decimal? Price { get; set; }
    public string? FeeDescription { get; set; }
    public decimal? Formula { get; set; }
    public string? VisitDay { get; set; }
    public string? VisitSequence { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactCell { get; set; }
    public string? Phone { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? CircularsEmail { get; set; }
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? Headmaster { get; set; }
    public string? HeadmasterCell { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Language { get; set; }
    public bool PrintInvoice { get; set; }
    public bool ImportFlag { get; set; }
    public string? Afterschool1Name { get; set; }
    public string? Afterschool1Contact { get; set; }
    public string? Afterschool2Name { get; set; }
    public string? Afterschool2Contact { get; set; }
    public string? SchedulingNotes { get; set; }
    public string? MoneyMessage { get; set; }
    public string? SafeNotes { get; set; }
    public string? WebPage { get; set; }
    public string? Omsendbriewe { get; set; }
    public string? KcowWebPageLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
