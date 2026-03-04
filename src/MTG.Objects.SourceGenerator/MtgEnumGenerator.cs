using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MTG.Objects.SourceGenerator;

[Generator]
public class MtgEnumGenerator : IIncrementalGenerator
{
    // Diagnostic descriptors for debugging
    private static readonly DiagnosticDescriptor DebugInfo = new(
        "MTGGEN001",
        "Source Generator Debug Info",
        "{0}",
        "MTG.Objects.SourceGenerator",
        DiagnosticSeverity.Info,
        true);

    private static readonly DiagnosticDescriptor ErrorDiagnostic = new(
        "MTGGEN002",
        "Source Generator Error",
        "{0}",
        "MTG.Objects.SourceGenerator",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the marker attribute
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("GenerateEnumsAttribute.g.cs", SourceText.From(GenerateEnumsAttributeSource, Encoding.UTF8));
        });

        // Find all additional files that match EnumValues.json
        var enumJsonFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith("EnumValues.json"))
            .Select(static (file, ct) =>
            {
                var content = file.GetText(ct)!.ToString();
                return (file.Path, Content: content);
            });

        // Find classes with the GenerateEnums attribute
        var classesWithAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "MTG.Objects.SourceGenerator.GenerateEnumsAttribute",
                predicate: static (node, _) => true,
                transform: static (ctx, _) => ctx.TargetSymbol)
            .Collect();

        // Combine the JSON and attribute presence
        var combined = enumJsonFiles.Combine(classesWithAttribute);

        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var (jsonData, attributes) = source;
            
            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                "MtgEnumGenerator: Initialized and RegisterSourceOutput called"));
            
            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgEnumGenerator: JSON path: {jsonData.Path ?? "null"}"));
            
            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgEnumGenerator: Attributes found: {attributes.Length}"));
            
            if (string.IsNullOrEmpty(jsonData.Content))
            {
                spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                    "MtgEnumGenerator: JSON content is null or empty"));
                return;
            }

            spc.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None, 
                $"MtgEnumGenerator: JSON content length: {jsonData.Content.Length} characters"));

            GenerateEnums(spc, jsonData.Content);
        });
    }

    private static void GenerateEnums(SourceProductionContext context, string jsonContent)
    {
        try
        {
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
                "MtgEnumGenerator: Starting JSON parsing"));

            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            var categoryCount = 0;
            var enumCount = 0;

            foreach (var category in root.EnumerateObject())
            {
                categoryCount++;
                var categoryName = category.Name.Pascalize();

                context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
                    $"MtgEnumGenerator: Processing category '{category.Name}' (pascalized: '{categoryName}')"));

                enumCount += ProcessCategory(context, category.Value, categoryName);
            }

            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
                $"MtgEnumGenerator: Completed. Processed {categoryCount} categories and generated {enumCount} enums"));
        }
        catch (System.Exception ex)
        {
            DiagnosticHelper.ReportGeneratorException(context, ex, ErrorDiagnostic, DebugInfo, "MtgEnumGenerator", "EnumValues.json");
        }
    }

    private static int ProcessCategory(SourceProductionContext context, JsonElement categoryValue, string categoryName)
    {
        var enumCount = 0;

        foreach (var enumType in categoryValue.EnumerateObject())
        {
            var typeName = enumType.Name.Pascalize();

            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
                $"MtgEnumGenerator: Processing enum type '{enumType.Name}' in category '{categoryName}' (pascalized: '{typeName}')"));

            if (enumType.Value.ValueKind == JsonValueKind.Array)
            {
                enumCount += EmitEnumFromArray(context, categoryName, typeName, enumType.Value);
            }
            else if (enumType.Value.ValueKind == JsonValueKind.Object)
            {
                enumCount += ProcessNestedEnums(context, categoryName, typeName, enumType.Value);
            }
        }

        return enumCount;
    }

    private static int EmitEnumFromArray(SourceProductionContext context, string categoryName, string typeName, JsonElement array)
    {
        var values = array.EnumerateArray()
            .Select(v => v.GetString() ?? string.Empty)
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        if (!values.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
                $"MtgEnumGenerator: '{typeName}' has no values, skipping"));
            return 0;
        }

        context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
            $"MtgEnumGenerator: Generating enum '{categoryName}.{typeName}' with {values.Count} values"));

        var source = GenerateEnumClass(categoryName, typeName, values);
        context.AddSource($"{categoryName}.{typeName}.g.cs", SourceText.From(source, Encoding.UTF8));
        return 1;
    }

    private static int ProcessNestedEnums(SourceProductionContext context, string categoryName, string typeName, JsonElement obj)
    {
        context.ReportDiagnostic(Diagnostic.Create(DebugInfo, Location.None,
            $"MtgEnumGenerator: '{typeName}' is an object, checking for nested enums"));

        var enumCount = 0;
        var nestedCategoryName = $"{categoryName}.{typeName}";

        foreach (var nestedEnum in obj.EnumerateObject())
        {
            if (nestedEnum.Value.ValueKind != JsonValueKind.Array) continue;

            enumCount += EmitEnumFromArray(context, nestedCategoryName, nestedEnum.Name.Pascalize(), nestedEnum.Value);
        }

        return enumCount;
    }

    private static string GenerateEnumClass(string categoryName, string typeName, List<string> values)
    {
        var sb = new StringBuilder();
        var seenNames = new HashSet<string>();
        var valueIndex = 0;

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using Ardalis.SmartEnum;");
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine();
        sb.AppendLine($"namespace MTG.Objects.Enum.{categoryName}");
        sb.AppendLine("{");
        sb.AppendLine($"    [GeneratedCode(\"{typeof(MtgEnumGenerator).FullName}\", \"1.0.0.0\")]");
        sb.AppendLine($"    public sealed class {typeName} : SmartEnum<{typeName}>");
        sb.AppendLine("    {");

        // Generate static fields
        foreach (var value in values)
        {
            var elementName = FormatEnumName(value, typeName);
            
            if (!seenNames.Add(elementName))
                continue;

            sb.AppendLine($"        public static readonly {typeName} {elementName} = new(nameof({elementName}), {valueIndex});");
            valueIndex++;
        }

        sb.AppendLine();
        // Generate private constructor
        sb.AppendLine($"        private {typeName}(string name, int value) : base(name, value) {{ }}");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string FormatEnumName(string name, string className)
    {
        var elementName = name
            .Replace('-', ' ')           // Convert hyphens to spaces for pascalization
            .Replace(".", " ")           // Convert periods to spaces
            .Replace(",", " ")           // Convert commas to spaces
            .Replace("/", " ")           // Convert slashes to spaces
            .Replace("?", " QuestionMark ") // Replace question marks
            .Replace("'", "")            // Remove apostrophes (Bolas's ? Bolass)
            .Replace("'", "")            // Remove smart quotes
            .Replace("(", " ")           // Remove opening parenthesis
            .Replace(")", " ")           // Remove closing parenthesis
            .Replace("&", " And ")       // Convert ampersand to And
            .Pascalize()                 // Convert to PascalCase
            .Replace("D And D", "DnD");  // Special case for D&D

        // Remove any remaining non-word characters
        elementName = System.Text.RegularExpressions.Regex.Replace(elementName, @"[^\w]", "");

        // Special case for FrameVersions (note: className after pascalization)
        if ((className.Equals("FrameVersions", System.StringComparison.OrdinalIgnoreCase) || 
             className.Equals("FrameVersion", System.StringComparison.OrdinalIgnoreCase)) && 
            int.TryParse(elementName, out _))
        {
            elementName = $"Frame{elementName}";
        }
        
        // General case: prefix any identifier that starts with a digit
        else if (elementName.Length > 0 && char.IsDigit(elementName[0]))
        {
            elementName = $"_{elementName}";
        }

        return elementName;
    }

    private const string GenerateEnumsAttributeSource = @"// <auto-generated/>
namespace MTG.Objects.SourceGenerator
{
    /// <summary>
    /// Marker attribute to trigger generation of MTG enums from embedded JSON data.
    /// Apply this to a class or assembly to generate SmartEnum classes.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Assembly)]
    public class GenerateEnumsAttribute : System.Attribute
    {
    }
}
";
}
