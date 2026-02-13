using Kcow.Domain.Entities;

namespace Kcow.Application.Import.Mappers;

/// <summary>
/// Maps LegacySchoolRecord to School entity.
/// Mapping rules:
///   SchoolDescription → Name (fallback to ShortSchool)
///   ShortSchool → ShortName
///   Trok → TruckId (validated against known truck IDs)
///   Price (double) → Price (decimal)
///   Formula (float) → Formula (decimal)
///   All string fields are trimmed.
/// </summary>
public sealed class SchoolDataMapper : IDataMapper<LegacySchoolRecord, School>
{
    private static readonly IReadOnlySet<int> EmptyTruckIds = new HashSet<int>();
    private readonly IReadOnlySet<int> _validTruckIds;

    public SchoolDataMapper() : this(EmptyTruckIds) { }

    public SchoolDataMapper(IReadOnlySet<int> validTruckIds)
    {
        _validTruckIds = validTruckIds;
    }

    public MappingResult<School> Map(LegacySchoolRecord source)
    {
        var result = new MappingResult<School> { Success = true };

        var name = Trim(source.SchoolDescription) ?? Trim(source.ShortSchool) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            result.Warnings.Add(new MappingWarning("Name", $"School {source.SchoolId} is missing a description and short name."));
        }

        // Validate TruckId
        var truckId = source.Trok.HasValue ? (int?)source.Trok.Value : null;
        if (truckId.HasValue && _validTruckIds.Count > 0 && !_validTruckIds.Contains(truckId.Value))
        {
            result.Warnings.Add(new MappingWarning("TruckId",
                $"School {source.SchoolId} references invalid TruckId {truckId.Value}. Truck will be set to null.",
                truckId.Value.ToString(), null));
            truckId = null;
        }

        result.Data = new School
        {
            Id = source.SchoolId,
            Name = name,
            ShortName = Trim(source.ShortSchool),
            TruckId = truckId,
            Price = source.Price.HasValue ? Convert.ToDecimal(source.Price.Value) : null,
            FeeDescription = Trim(source.FormulaDescription),
            Formula = source.Formula.HasValue ? Convert.ToDecimal(source.Formula.Value) : null,
            VisitDay = Trim(source.Day),
            VisitSequence = Trim(source.Sequence),
            ContactPerson = Trim(source.ContactPerson),
            ContactCell = Trim(source.ContactCell),
            Telephone = Trim(source.Telephone),
            Fax = Trim(source.Fax),
            Email = Trim(source.EmailAddress),
            CircularsEmail = Trim(source.Omsendbriewe),
            Address = Trim(source.Address1),
            Address2 = Trim(source.Address2),
            Headmaster = Trim(source.Headmaster),
            HeadmasterCell = Trim(source.HeadmasterCell),
            MoneyMessage = Trim(source.MoneyMessage),
            PrintInvoice = source.Print,
            Language = Trim(source.Taal),
            ImportFlag = source.Import,
            Afterschool1Name = Trim(source.Naskool1Name),
            Afterschool1Contact = Trim(source.Naskool1Contact),
            Afterschool2Name = Trim(source.Naskool2Name),
            Afterschool2Contact = Trim(source.Naskool2Contact),
            SafeNotes = Trim(source.Kluis),
            WebPage = Trim(source.WebPage),
            KcowWebPageLink = Trim(source.KcowWebPageLink),
            LegacyId = source.SchoolId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        return result;
    }

    public MappingResult<List<School>> MapMany(IEnumerable<LegacySchoolRecord> sources)
    {
        var schools = new List<School>();
        var allWarnings = new List<MappingWarning>();
        var allErrors = new List<MappingError>();

        foreach (var source in sources)
        {
            var result = Map(source);
            if (result.Success && result.Data is not null)
            {
                schools.Add(result.Data);
            }
            allWarnings.AddRange(result.Warnings);
            allErrors.AddRange(result.Errors);
        }

        return new MappingResult<List<School>>
        {
            Data = schools,
            Success = allErrors.Count == 0,
            Warnings = allWarnings,
            Errors = allErrors
        };
    }

    private static string? Trim(string? value) => value?.Trim();
}
