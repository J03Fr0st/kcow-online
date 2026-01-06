using Kcow.Domain.Entities;

namespace Kcow.Application.Import;

public sealed record LegacySchoolMappingResult(School School, IReadOnlyList<string> Warnings);

public sealed class LegacySchoolMapper
{
    private static readonly IReadOnlySet<int> EmptyTruckIds = new HashSet<int>();

    private readonly IReadOnlySet<int> _validTruckIds;

    public LegacySchoolMapper() : this(EmptyTruckIds) { }

    public LegacySchoolMapper(IReadOnlySet<int> validTruckIds)
    {
        _validTruckIds = validTruckIds;
    }

    public LegacySchoolMappingResult Map(LegacySchoolRecord record)
    {
        var warnings = new List<string>();

        var name = Trim(record.SchoolDescription) ?? Trim(record.ShortSchool) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            warnings.Add($"School {record.SchoolId} is missing a description and short name.");
        }

        // Validate TruckId foreign key
        if (record.Trok.HasValue && _validTruckIds.Count > 0 && !_validTruckIds.Contains(record.Trok.Value))
        {
            warnings.Add($"School {record.SchoolId} references invalid TruckId {record.Trok.Value}. Truck will be set to null.");
        }

        var truckId = record.Trok;
        if (truckId.HasValue && _validTruckIds.Count > 0 && !_validTruckIds.Contains(truckId.Value))
        {
            truckId = null;
        }

        var school = new School
        {
            Id = record.SchoolId,
            Name = name,
            ShortName = Trim(record.ShortSchool),
            TruckId = truckId,
            Price = record.Price.HasValue ? Convert.ToDecimal(record.Price.Value) : null,
            FeeDescription = Trim(record.FormulaDescription),
            Formula = record.Formula.HasValue ? Convert.ToDecimal(record.Formula.Value) : null,
            VisitDay = Trim(record.Day),
            VisitSequence = Trim(record.Sequence),
            ContactPerson = Trim(record.ContactPerson),
            ContactCell = Trim(record.ContactCell),
            Telephone = Trim(record.Telephone),
            Fax = Trim(record.Fax),
            Email = Trim(record.EmailAddress),
            CircularsEmail = Trim(record.Omsendbriewe),
            Address = Trim(record.Address1),
            Address2 = Trim(record.Address2),
            Headmaster = Trim(record.Headmaster),
            HeadmasterCell = Trim(record.HeadmasterCell),
            MoneyMessage = Trim(record.MoneyMessage),
            PrintInvoice = record.Print,
            Language = Trim(record.Taal),
            ImportFlag = record.Import,
            Afterschool1Name = Trim(record.Naskool1Name),
            Afterschool1Contact = Trim(record.Naskool1Contact),
            Afterschool2Name = Trim(record.Naskool2Name),
            Afterschool2Contact = Trim(record.Naskool2Contact),
            SafeNotes = Trim(record.Kluis),
            WebPage = Trim(record.WebPage),
            KcowWebPageLink = Trim(record.KcowWebPageLink),
            CreatedAt = DateTime.UtcNow
        };

        return new LegacySchoolMappingResult(school, warnings);
    }

    private static string? Trim(string? value) => value?.Trim();
}
