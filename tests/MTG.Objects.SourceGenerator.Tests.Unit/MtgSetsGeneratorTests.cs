using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MTG.Objects.SourceGenerator;
using System.Collections.Immutable;
using System.Text;

namespace MTG.Objects.SourceGenerator.Tests.Unit;

public class MtgSetsGeneratorTests
{
    [Fact]
    public void Generator_WithValidSetJson_GeneratesSetsClass()
    {
        // Arrange
        var jsonContent = @"{
  ""data"": [
    { ""code"": ""10E"", ""name"": ""Tenth Edition"" },
    { ""code"": ""2ED"", ""name"": ""Unlimited Edition"" },
    { ""code"": ""ARB"", ""name"": ""Alara Reborn"" }
  ]
}";

        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, jsonContent, "SetList.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        Assert.Empty(result.Diagnostics);
        
        var generatedTrees = result.GeneratedTrees;
        Assert.Contains(generatedTrees, t => t.FilePath.Contains("Sets.g.cs"));
    }

    [Fact]
    public void Generator_SetsClass_ContainsExpectedSets()
    {
        // Arrange
        var jsonContent = @"{
  ""data"": [
    { ""code"": ""LEA"", ""name"": ""Limited Edition Alpha"" },
    { ""code"": ""LEB"", ""name"": ""Limited Edition Beta"" },
    { ""code"": ""2ED"", ""name"": ""Unlimited Edition"" }
  ]
}";

        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, jsonContent, "SetList.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        var setsFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Sets.g.cs"));
        
        Assert.NotNull(setsFile);
        var source = setsFile.ToString();
        
        Assert.Contains(@"{ ""LEA"", ""Limited Edition Alpha"" }", source);
        Assert.Contains(@"{ ""LEB"", ""Limited Edition Beta"" }", source);
        Assert.Contains(@"{ ""2ED"", ""Unlimited Edition"" }", source);
    }

    [Fact]
    public void Generator_SetsClass_HasCorrectStructure()
    {
        // Arrange
        var jsonContent = @"{
  ""data"": [
    { ""code"": ""TST"", ""name"": ""Test Set"" }
  ]
}";

        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, jsonContent, "SetList.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        var setsFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Sets.g.cs"));
        
        Assert.NotNull(setsFile);
        var source = setsFile.ToString();
        
        // Check class structure
        Assert.Contains("namespace MTG.Objects.Set", source);
        Assert.Contains("public sealed class Sets : Dictionary<string, string>", source);
        Assert.Contains("public static readonly Sets SetList;", source);
        Assert.Contains("private Sets() { }", source);
        Assert.Contains("static Sets()", source);
    }

    [Fact]
    public void Generator_SetsWithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var jsonContent = @"{
  ""data"": [
    { ""code"": ""TST"", ""name"": ""Test \"Quoted\" Set"" }
  ]
}";

        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, jsonContent, "SetList.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        var setsFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Sets.g.cs"));
        
        Assert.NotNull(setsFile);
        var source = setsFile.ToString();
        
        Assert.Contains(@"{ ""TST"", ""Test \""Quoted\"" Set"" }", source);
    }

    [Fact]
    public void Generator_WithoutJsonFile_DoesNotGenerateCode()
    {
        // Arrange
        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, null, null);

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        
        // Should only contain the attribute definition, not the Sets class
        var nonAttributeFiles = result.GeneratedTrees
            .Where(t => !t.FilePath.Contains("GenerateSetsAttribute"))
            .ToList();
        
        Assert.Empty(nonAttributeFiles);
    }

    [Fact]
    public void Generator_WithMalformedJson_ReportsDiagnostic()
    {
        // Arrange
        var jsonContent = "{ invalid json }";
        var generator = new MtgSetsGenerator();
        var driver = CreateDriver(generator, jsonContent, "SetList.json");

        // Act
        driver = driver.RunGenerators();

        // Assert
        var result = driver.GetRunResult();
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(result.Diagnostics, d => d.Id == "MTG002");
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

[assembly: GenerateSets]
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
