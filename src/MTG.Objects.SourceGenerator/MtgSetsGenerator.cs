using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MTG.Objects.SourceGenerator;

[Generator]
public class MtgSetsGenerator : IIncrementalGenerator
{
    // Diagnostic descriptors for debugging
    private static readonly DiagnosticDescriptor DebugInfo = new(
        "MTGSETS001",
        "Sets Generator Debug Info",
        "{0}",
        "MTG.Objects.SourceGenerator",
        DiagnosticSeverity.Info,
        true);

    private static readonly DiagnosticDescriptor ErrorDiagnostic = new(
        "MTGSETS002",
        "Sets Generator Error",
        "{0}",
        "MTG.Objects.SourceGenerator",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the marker attribute so it's available to the consuming project
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("GenerateSetsAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8));
        });

        // Find all additional files that match SetList.json
        var setJsonFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith("SetList.json"))
            .Select(static (file, ct) =>
            {
                var content = file.GetText(ct)!.ToString();
                return (file.Path, Content: content);
            })
            .Collect();

        // Find classes with the GenerateSets attribute
        var classesWithAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "MTG.Objects.SourceGenerator.GenerateSetsAttribute",
                predicate: static (node, _) => true,
                transform: static (ctx, _) => ctx.TargetSymbol)
            .Collect();

        // Combine the JSON and attribute presence
        var combined = setJsonFiles.Combine(classesWithAttribute);

        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var (jsonFiles, attributes) = source;
            
            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                "MtgSetsGenerator: Initialized and RegisterSourceOutput called"));
            
            // Fail the build if SetList.json doesn't exist
            if (jsonFiles.Length == 0)
            {
                spc.ReportDiagnostic(Diagnostic.Create(ErrorDiagnostic, Location.None, 
                    "SetList.json file not found. Ensure the file exists and is marked as AdditionalFiles in the project."));
                return;
            }
            
            var jsonData = jsonFiles[0];
            
            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: JSON path: {jsonData.Path ?? "null"}"));
            
            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: Attributes found: {attributes.Length}"));
            
            if (string.IsNullOrEmpty(jsonData.Content))
            {
                spc.ReportDiagnostic(Diagnostic.Create(ErrorDiagnostic, Location.None, 
                    "SetList.json file is empty. The file must contain valid JSON data."));
                return;
            }

            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: JSON content length: {jsonData.Content.Length} characters"));

            GenerateSets(spc, jsonData.Content);
        });
    }

    private static void GenerateSets(SourceProductionContext context, string jsonContent)
    {
        try
        {
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                "MtgSetsGenerator: Starting JSON parsing"));

            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (!root.TryGetProperty("data", out var dataArray))
            {
                context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                    "MtgSetsGenerator: 'data' property not found in JSON root"));
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: 'data' array found with ValueKind: {dataArray.ValueKind}"));

            var sets = new List<(string Code, string Name)>();

            foreach (var item in dataArray.EnumerateArray())
            {
                if (item.TryGetProperty("code", out var code) &&
                    item.TryGetProperty("name", out var name))
                {
                    var codeValue = code.GetString() ?? string.Empty;
                    var nameValue = name.GetString() ?? string.Empty;
                    sets.Add((codeValue, nameValue));
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: Parsed {sets.Count} sets from JSON"));

            // Generate SetInfo record
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                "MtgSetsGenerator: Generating SetInfo.g.cs"));
            
            var setInfoSource = GenerateSetInfoRecord();
            context.AddSource("SetInfo.g.cs", SourceText.From(setInfoSource, Encoding.UTF8));

            // Generate Sets static class
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: Generating Sets.g.cs with {sets.Count} set properties"));
            
            var setsSource = GenerateSetsClass(sets);
            context.AddSource("Sets.g.cs", SourceText.From(setsSource, Encoding.UTF8));

            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                "MtgSetsGenerator: Completed successfully"));
        }
        catch (JsonException ex)
        {
            // If JSON parsing fails, report a diagnostic
            context.ReportDiagnostic(Diagnostic.Create(ErrorDiagnostic, Location.None, 
                $"Failed to parse SetList.json: {ex.Message}"));
            
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: JsonException stack trace: {ex.StackTrace}"));
        }
        catch (System.Exception ex)
        {
            // Catch any other exceptions
            context.ReportDiagnostic(Diagnostic.Create(ErrorDiagnostic, Location.None, 
                $"Failed to generate sets: {ex.GetType().Name}: {ex.Message}"));
            
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgSetsGenerator: Exception stack trace: {ex.StackTrace}"));
        }
    }

    private static string GenerateSetInfoRecord()
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine();
        sb.AppendLine("namespace MTG.Objects.Set");
        sb.AppendLine("{");
        sb.AppendLine($"    [GeneratedCode(\"{typeof(MtgSetsGenerator).FullName}\", \"1.0.0.0\")]");
        sb.AppendLine("    public sealed record SetInfo(string Code, string Name);");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateSetsClass(List<(string Code, string Name)> sets)
    {
        var sb = new StringBuilder();
        var seenPropertyNames = new HashSet<string>();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine("namespace MTG.Objects.Set");
        sb.AppendLine("{");
        sb.AppendLine($"    [GeneratedCode(\"{typeof(MtgSetsGenerator).FullName}\", \"1.0.0.0\")]");
        sb.AppendLine("    public static class Sets");
        sb.AppendLine("    {");

        // Generate static readonly properties for each set
        foreach (var (code, name) in sets.OrderBy(s => s.Code))
        {
            var propertyName = FormatPropertyName(name);
            
            // Handle duplicate property names
            if (seenPropertyNames.Contains(propertyName))
            {
                propertyName = $"{propertyName}_{code}";
            }
            seenPropertyNames.Add(propertyName);

            var escapedName = name.Replace("\\", "\\\\").Replace("\"", "\\\"");
            sb.AppendLine($"        public static readonly SetInfo {propertyName} = new(\"{code}\", \"{escapedName}\");");
        }

        sb.AppendLine();
        sb.AppendLine("        // Lookup dictionary for code-based retrieval");
        sb.AppendLine("        private static readonly Dictionary<string, SetInfo> _byCode = new()");
        sb.AppendLine("        {");

        // Reset for dictionary generation
        seenPropertyNames.Clear();
        
        foreach (var (code, name) in sets.OrderBy(s => s.Code))
        {
            var propertyName = FormatPropertyName(name);
            
            if (seenPropertyNames.Contains(propertyName))
            {
                propertyName = $"{propertyName}_{code}";
            }
            seenPropertyNames.Add(propertyName);

            sb.AppendLine($"            {{ \"{code}\", {propertyName} }},");
        }

        sb.AppendLine("        };");
        sb.AppendLine();
        sb.AppendLine("        // Lookup dictionary for name-based retrieval");
        sb.AppendLine("        private static readonly Dictionary<string, SetInfo> _byName = new()");
        sb.AppendLine("        {");

        // Reset for name dictionary generation
        seenPropertyNames.Clear();
        
        foreach (var (code, name) in sets.OrderBy(s => s.Code))
        {
            var propertyName = FormatPropertyName(name);
            var escapedName = name.Replace("\\", "\\\\").Replace("\"", "\\\"");
            
            if (seenPropertyNames.Contains(propertyName))
            {
                propertyName = $"{propertyName}_{code}";
            }
            seenPropertyNames.Add(propertyName);

            sb.AppendLine($"            {{ \"{escapedName}\", {propertyName} }},");
        }

        sb.AppendLine("        };");
        sb.AppendLine();
        
        // Add utility methods
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Attempts to retrieve set information by set code.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static bool TryGetByCode(string code, out SetInfo? set)");
        sb.AppendLine("            => _byCode.TryGetValue(code, out set);");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Attempts to retrieve set information by set name.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static bool TryGetByName(string name, out SetInfo? set)");
        sb.AppendLine("            => _byName.TryGetValue(name, out set);");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Gets all available sets.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static IEnumerable<SetInfo> All => _byCode.Values;");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Gets all set codes.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static IEnumerable<string> AllCodes => _byCode.Keys;");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Gets all set names.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public static IEnumerable<string> AllNames => _byName.Keys;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string FormatPropertyName(string name)
    {
        // Remove special characters and convert to PascalCase
        var propertyName = name
            .Replace(":", "")
            .Replace("'", "")
            .Replace("'", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("-", " ")
            .Replace("(", " ")   // Remove opening parenthesis
            .Replace(")", " ")   // Remove closing parenthesis
            .Replace("&", "And")
            .Replace("/", " ")   // Remove slashes
            .Replace("\"", "")   // Remove quotes
            .Pascalize();

        // Ensure it doesn't start with a digit
        if (propertyName.Length > 0 && char.IsDigit(propertyName[0]))
        {
            propertyName = "_" + propertyName;
        }

        // Remove any remaining invalid characters
        propertyName = System.Text.RegularExpressions.Regex.Replace(propertyName, @"[^\w]", "");

        return propertyName;
    }

    private const string AttributeSource = @"// <auto-generated/>
namespace MTG.Objects.SourceGenerator
{
    /// <summary>
    /// Marker attribute to trigger generation of MTG Sets dictionary from embedded JSON data.
    /// Apply this to a class or assembly to generate the Sets class.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Assembly)]
    public class GenerateSetsAttribute : System.Attribute
    {
    }
}
";
}
