using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace MTG.Objects.SourceGenerator.Tests.Unit.Helpers;

internal static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunEnumGenerator(string? jsonContent)
    {
        var generator = new MtgEnumGenerator();
        var driver = CreateDriver(generator, jsonContent, "EnumValues.json", "GenerateEnums");
        return driver.GetRunResult();
    }

    public static GeneratorDriverRunResult RunSetsGenerator(string? jsonContent)
    {
        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, jsonContent, "SetList.json", "GenerateSets");
        return driver.GetRunResult();
    }

    public static GeneratorDriver CreateDriver(
        IIncrementalGenerator generator,
        string? jsonContent,
        string? fileName,
        string attributeName)
    {
        var compilation = CreateCompilation(attributeName);
        var driver = CSharpGeneratorDriver.Create(generator);

        if (jsonContent is not null && fileName is not null)
        {
            var additionalText = new TestAdditionalText(fileName, jsonContent);
            driver = (CSharpGeneratorDriver)driver.AddAdditionalTexts(
                ImmutableArray.Create<AdditionalText>(additionalText));
        }

        return driver.RunGenerators(compilation);
    }

    private static CSharpCompilation CreateCompilation(string attributeName)
    {
        // Use class-level attribute, not assembly-level, because
        // ForAttributeWithMetadataName only finds attributes on type declarations.
        var source = $@"
using MTG.Objects.SourceGenerator;

[{attributeName}]
public partial class TestTarget {{ }}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>();

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
