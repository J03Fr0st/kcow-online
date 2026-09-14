using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kcow.Application.Import.Mappers;
using Kcow.Domain.Entities;

namespace Kcow.Application.Import;

/// <summary>Immutable snapshot of mapped input. Execution never re-reads source files.
/// Database conflicts and optional foreign-key resolution are evaluated at execution time.</summary>
public sealed record ImportPlan(string InputPath, string SourceFingerprint, ConflictResolutionMode ConflictMode,
    EntityPlan<School> Schools, EntityPlan<ClassGroup> ClassGroups, EntityPlan<Activity> Activities, EntityPlan<StudentMappingData> Students)
{
    public string RunId { get; } = Guid.NewGuid().ToString("N");
}

public sealed class EntityPlan<T>(int parsed, bool missing, IReadOnlyList<string> parseErrors, MappingResult<List<T>> mapping)
{
    private readonly string _snapshot = JsonSerializer.Serialize(mapping);
    public int Parsed { get; } = parsed;
    public bool Missing { get; } = missing;
    public IReadOnlyList<string> ParseErrors { get; } = Array.AsReadOnly(parseErrors.ToArray());
    public MappingResult<List<T>> GetMapping() => JsonSerializer.Deserialize<MappingResult<List<T>>>(_snapshot)!;
}

public sealed class ImportPlanBuilder(ILegacyParser parser)
{
    private static readonly string[] Sources = ["1_School/School", "2_Class_Group/Class Group", "3_Activity/Activity", "4_Children/Children"];

    public ImportPlan Build(string inputPath, ConflictResolutionMode mode = ConflictResolutionMode.FailOnConflict, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var fingerprint = Fingerprint(inputPath, ct);
        var schools = BuildEntity(inputPath, Sources[0], parser.ParseSchools, new SchoolDataMapper().MapMany, ct);
        var groups = BuildEntity(inputPath, Sources[1], parser.ParseClassGroups, new ClassGroupDataMapper().MapMany, ct);
        var activities = BuildEntity(inputPath, Sources[2], parser.ParseActivities, new ActivityDataMapper().MapMany, ct);
        var students = BuildEntity(inputPath, Sources[3], parser.ParseChildren, new StudentDataMapper().MapMany, ct);
        if (Fingerprint(inputPath, ct) != fingerprint) throw new IOException("Import source changed while planning; create a fresh plan.");
        return new ImportPlan(inputPath, fingerprint, mode, schools, groups, activities, students);
    }

    private static EntityPlan<T> BuildEntity<S, T>(string root, string relative, Func<string,string,ParseResult<S>> parse,
        Func<IEnumerable<S>,MappingResult<List<T>>> map, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var xml = Path.Combine(root, relative + ".xml"); var xsd = Path.Combine(root, relative + ".xsd");
        if (!File.Exists(xml) || !File.Exists(xsd)) return new(0, true, [], new() { Data = [] });
        try
        {
            var result = parse(xml, xsd);
            ct.ThrowIfCancellationRequested();
            return new(result.Records.Count, false, result.Errors.Select(e => e.ToString()).ToArray(), map(result.Records));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is IOException or System.Xml.XmlException or System.Xml.Schema.XmlSchemaException)
        {
            return new(0, false, [ex.Message], new() { Data = [] });
        }
    }

    private static string Fingerprint(string root, CancellationToken ct)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var source in Sources)
        foreach (var extension in new[] { ".xml", ".xsd" })
        {
            ct.ThrowIfCancellationRequested();
            var relative = source + extension; var path = Path.Combine(root, relative);
            hash.AppendData(Encoding.UTF8.GetBytes(relative));
            if (!File.Exists(path)) { hash.AppendData([0]); continue; }
            hash.AppendData([1]);
            using var stream = File.OpenRead(path);
            var buffer = new byte[65536]; int count;
            while ((count = stream.Read(buffer)) > 0) { ct.ThrowIfCancellationRequested(); hash.AppendData(buffer.AsSpan(0, count)); }
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
