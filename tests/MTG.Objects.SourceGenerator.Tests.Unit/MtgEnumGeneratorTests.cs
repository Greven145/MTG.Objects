using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MTG.Objects.SourceGenerator;
using System.Collections.Immutable;
using System.Text;

namespace MTG.Objects.SourceGenerator.Tests.Unit;

public class MtgEnumGeneratorTests
{
    [Fact]
    public void Generator_WithValidEnumJson_GeneratesClasses()
    {
        // Arrange
        var jsonContent = @"{
  ""card"": {
    ""borderColors"": [""black"", ""white"", ""borderless""],
    ""frameVersions"": [""1993"", ""2015"", ""future""]
  }
}";

        var generator = new MtgEnumGenerator();
        var driver = CreateDriver(generator, jsonContent, "EnumValues.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        Assert.Empty(result.Diagnostics);
        
        var generatedTrees = result.GeneratedTrees;
        Assert.Contains(generatedTrees, t => t.FilePath.Contains("Card.BorderColors.g.cs"));
        Assert.Contains(generatedTrees, t => t.FilePath.Contains("Card.FrameVersions.g.cs"));
    }

    [Fact]
    public void Generator_BorderColorsEnum_ContainsExpectedValues()
    {
        // Arrange
        var jsonContent = @"{
  ""card"": {
    ""borderColors"": [""black"", ""white"", ""borderless"", ""gold"", ""silver""]
  }
}";

        var generator = new MtgEnumGenerator();
        var driver = CreateDriver(generator, jsonContent, "EnumValues.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        var borderColorsFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Card.BorderColors.g.cs"));
        
        Assert.NotNull(borderColorsFile);
        var source = borderColorsFile.ToString();
        
        Assert.Contains("public static readonly BorderColors Black", source);
        Assert.Contains("public static readonly BorderColors White", source);
        Assert.Contains("public static readonly BorderColors Borderless", source);
        Assert.Contains("public static readonly BorderColors Gold", source);
        Assert.Contains("public static readonly BorderColors Silver", source);
    }

    [Fact]
    public void Generator_SpecialCharacters_AreFormattedCorrectly()
    {
        // Arrange
        var jsonContent = @"{
  ""card"": {
    ""subtypes"": [""B.O.B."", ""And/Or"", ""Elemental?"", ""Bolas's Realm""]
  }
}";

        var generator = new MtgEnumGenerator();
        var driver = CreateDriver(generator, jsonContent, "EnumValues.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        var subtypesFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Card.Subtypes.g.cs"));
        
        Assert.NotNull(subtypesFile);
        var source = subtypesFile.ToString();
        
        Assert.Contains("BOB", source); // . removed
        Assert.Contains("AndOr", source); // / removed
        Assert.Contains("ElementalQuestionMark", source); // ? replaced
        Assert.Contains("BolassRealm", source); // ' removed
    }

    [Fact]
    public void Generator_FrameVersion_AddsFramePrefix()
    {
        // Arrange
        var jsonContent = @"{
  ""card"": {
    ""frameVersion"": [""1993"", ""2015"", ""future""]
  }
}";

        var generator = new MtgEnumGenerator();
        var driver = CreateDriver(generator, jsonContent, "EnumValues.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        var frameVersionFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Card.FrameVersion.g.cs"));
        
        Assert.NotNull(frameVersionFile);
        var source = frameVersionFile.ToString();
        
        Assert.Contains("public static readonly FrameVersion Frame1993", source);
        Assert.Contains("public static readonly FrameVersion Frame2015", source);
        Assert.Contains("public static readonly FrameVersion Future", source);
    }

    [Fact]
    public void Generator_WithoutJsonFile_DoesNotGenerateCode()
    {
        // Arrange
        var generator = new MtgEnumGenerator();
        var driver = CreateDriver(generator, null, null);

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        
        // Should only contain the attribute definition, not any generated enums
        var nonAttributeFiles = result.GeneratedTrees
            .Where(t => !t.FilePath.Contains("GenerateEnumsAttribute"))
            .ToList();
        
        Assert.Empty(nonAttributeFiles);
    }

    private static GeneratorDriver CreateDriver(IIncrementalGenerator generator, string? jsonContent, string? fileName)
    {
        var compilation = CreateCompilation();
        var driver = CSharpGeneratorDriver.Create(generator);

        if (jsonContent != null && fileName != null)
        {
            var additionalText = new TestAdditionalText(fileName, jsonContent);
            driver = (CSharpGeneratorDriver)driver.AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(additionalText));
        }

        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }

    private static Compilation CreateCompilation()
    {
        var source = @"
using MTG.Objects.SourceGenerator;

[assembly: GenerateEnums]
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

    private class TestAdditionalText : AdditionalText
    {
        private readonly string _text;

        public TestAdditionalText(string path, string text)
        {
            Path = path;
            _text = text;
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(_text, Encoding.UTF8);
        }
    }
}
