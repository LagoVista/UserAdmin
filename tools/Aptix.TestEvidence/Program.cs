using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

const string propertyAttributeName = "NUnit.Framework.PropertyAttribute";
const string evidencePropertyName = "AptixEvidence";

var assemblyPath = ReadArgument(args, "--assembly");
var outputPath = ReadArgument(args, "--output", required: false);

if (String.IsNullOrWhiteSpace(assemblyPath))
{
    Console.Error.WriteLine("Usage: Aptix.TestEvidence --assembly <test-assembly.dll> [--output <metadata.json>]");
    return 2;
}

assemblyPath = Path.GetFullPath(assemblyPath);
if (!File.Exists(assemblyPath))
{
    Console.Error.WriteLine($"Test assembly was not found: {assemblyPath}");
    return 3;
}

var assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
AssemblyLoadContext.Default.Resolving += (_, assemblyName) =>
{
    var candidate = Path.Combine(assemblyDirectory, $"{assemblyName.Name}.dll");
    return File.Exists(candidate) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(candidate) : null;
};

var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
var tests = new List<TestEvidenceMetadata>();
var issues = new List<string>();

foreach (var type in GetLoadableTypes(assembly, issues).OrderBy(type => type.FullName))
{
    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(method => method.Name))
    {
        var evidence = new List<EvidenceReferenceSet>();

        foreach (var attribute in method.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == propertyAttributeName))
        {
            if (attribute.ConstructorArguments.Count < 2)
                continue;

            var propertyName = attribute.ConstructorArguments[0].Value as string;
            var propertyValue = attribute.ConstructorArguments[1].Value?.ToString();

            if (!String.Equals(propertyName, evidencePropertyName, StringComparison.Ordinal) || String.IsNullOrWhiteSpace(propertyValue))
                continue;

            var parsed = ParseEvidence(propertyValue, type, method, issues);
            if (parsed != null && !evidence.Any(existing => existing.Profile == parsed.Profile && existing.References.SequenceEqual(parsed.References)))
                evidence.Add(parsed);
        }

        if (evidence.Count > 0)
        {
            tests.Add(new TestEvidenceMetadata
            {
                TestType = type.FullName ?? type.Name,
                TestMethod = method.Name,
                Evidence = evidence
            });
        }
    }
}

var document = new TestEvidenceDocument
{
    SchemaVersion = "1.0",
    AssemblyPath = assemblyPath,
    AssemblyName = assembly.GetName().Name ?? Path.GetFileNameWithoutExtension(assemblyPath),
    ReadUtc = DateTime.UtcNow,
    Tests = tests,
    Issues = issues
};

var json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
if (!String.IsNullOrWhiteSpace(outputPath))
{
    outputPath = Path.GetFullPath(outputPath);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    await File.WriteAllTextAsync(outputPath, json);
}
else
{
    Console.WriteLine(json);
}

return issues.Count == 0 ? 0 : 1;

static string? ReadArgument(string[] arguments, string name, bool required = true)
{
    var index = Array.FindIndex(arguments, argument => String.Equals(argument, name, StringComparison.OrdinalIgnoreCase));
    if (index >= 0 && index + 1 < arguments.Length)
        return arguments[index + 1];

    return required ? null : String.Empty;
}

static IEnumerable<Type> GetLoadableTypes(Assembly assembly, List<string> issues)
{
    try
    {
        return assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException exception)
    {
        foreach (var loaderException in exception.LoaderExceptions.Where(exception => exception != null))
            issues.Add(loaderException!.Message);

        return exception.Types.Where(type => type != null)!;
    }
}

static EvidenceReferenceSet? ParseEvidence(string value, Type type, MethodInfo method, List<string> issues)
{
    var segments = value.Split('|', StringSplitOptions.TrimEntries);
    if (segments.Length < 2 || segments.Any(String.IsNullOrWhiteSpace))
    {
        issues.Add($"Malformed AptixEvidence value on {type.FullName}.{method.Name}: {value}");
        return null;
    }

    return new EvidenceReferenceSet
    {
        Profile = segments[0],
        References = segments.Skip(1).Distinct(StringComparer.Ordinal).ToList()
    };
}

sealed class TestEvidenceDocument
{
    public string SchemaVersion { get; set; } = String.Empty;
    public string AssemblyPath { get; set; } = String.Empty;
    public string AssemblyName { get; set; } = String.Empty;
    public DateTime ReadUtc { get; set; }
    public List<TestEvidenceMetadata> Tests { get; set; } = new();
    public List<string> Issues { get; set; } = new();
}

sealed class TestEvidenceMetadata
{
    public string TestType { get; set; } = String.Empty;
    public string TestMethod { get; set; } = String.Empty;
    public List<EvidenceReferenceSet> Evidence { get; set; } = new();
}

sealed class EvidenceReferenceSet
{
    public string Profile { get; set; } = String.Empty;
    public List<string> References { get; set; } = new();
}
