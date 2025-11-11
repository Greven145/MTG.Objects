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
            .Select(static (file, ct) => file.GetText(ct)!.ToString());

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
            var (jsonContent, _) = source;
            
            if (string.IsNullOrEmpty(jsonContent))
                return;

            GenerateEnums(spc, jsonContent);
        });
    }

    private static void GenerateEnums(SourceProductionContext context, string jsonContent)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            foreach (var category in root.EnumerateObject())
            {
                var categoryName = category.Name.Pascalize();
                
                foreach (var enumType in category.Value.EnumerateObject())
                {
                    var typeName = enumType.Name.Pascalize();
                    
                    // Skip if the value is not an array
                    if (enumType.Value.ValueKind != JsonValueKind.Array)
                    {
                        // Check if it's an object with nested arrays
                        if (enumType.Value.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var nestedEnum in enumType.Value.EnumerateObject())
                            {
                                if (nestedEnum.Value.ValueKind != JsonValueKind.Array) continue;
                                var nestedTypeName = nestedEnum.Name.Pascalize();
                                var nestedValues = nestedEnum.Value.EnumerateArray()
                                    .Select(v => v.GetString() ?? string.Empty)
                                    .Where(v => !string.IsNullOrEmpty(v))
                                    .ToList();

                                if (!nestedValues.Any()) continue;
                                var nestedSource = GenerateEnumClass($"{categoryName}.{typeName}", nestedTypeName, nestedValues);
                                context.AddSource($"{categoryName}.{typeName}.{nestedTypeName}.g.cs", SourceText.From(nestedSource, Encoding.UTF8));
                            }
                        }
                        continue;
                    }
                    
                    var values = enumType.Value.EnumerateArray()
                        .Select(v => v.GetString() ?? string.Empty)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .ToList();

                    if (!values.Any()) continue;
                    var source = GenerateEnumClass(categoryName, typeName, values);
                    context.AddSource($"{categoryName}.{typeName}.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
        }
        catch (JsonException ex)
        {
            // If JSON parsing fails, report a diagnostic
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "MTG001",
                    "Invalid JSON",
                    $"Failed to parse EnumValues.json file: {ex.Message}",
                    "MTG.Objects.SourceGenerator",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
        }
        catch (System.Exception ex)
        {
            // Catch any other exceptions
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "MTG001",
                    "Generation Error",
                    $"Failed to generate enums: {ex.Message}",
                    "MTG.Objects.SourceGenerator",
                    DiagnosticSeverity.Error,
                    true),
                Location.None));
        }
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
