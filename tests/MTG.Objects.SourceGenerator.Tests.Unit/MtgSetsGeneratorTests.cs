using MTG.Objects.SourceGenerator.Tests.Unit.Helpers;

namespace MTG.Objects.SourceGenerator.Tests.Unit;

public class MtgSetsGeneratorTests
{
    private static string MakeJson(params (string code, string name)[] sets)
    {
        var entries = string.Join(",\n    ",
            sets.Select(s => $@"{{ ""code"": ""{s.code}"", ""name"": ""{s.name}"" }}"));
        return $@"{{
  ""data"": [
    {entries}
  ]
}}";
    }

    // ── Existing tests (fixed) ──────────────────────────────────────────

    [Fact]
    public void Generator_WithValidSetJson_GeneratesSetsClass()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("10E", "Tenth Edition"), ("2ED", "Unlimited Edition"), ("ARB", "Alara Reborn")));

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Sets.g.cs"));
    }

    [Fact]
    public void Generator_SetsClass_ContainsExpectedSets()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("LEA", "Limited Edition Alpha"), ("LEB", "Limited Edition Beta"), ("2ED", "Unlimited Edition")));

        var source = GetSetsSource(result);

        Assert.Contains("public static readonly SetInfo LimitedEditionAlpha = new(\"LEA\", \"Limited Edition Alpha\")", source);
        Assert.Contains("public static readonly SetInfo LimitedEditionBeta = new(\"LEB\", \"Limited Edition Beta\")", source);
        Assert.Contains("public static readonly SetInfo UnlimitedEdition = new(\"2ED\", \"Unlimited Edition\")", source);
    }

    [Fact]
    public void Generator_SetsClass_HasCorrectStructure()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var source = GetSetsSource(result);

        Assert.Contains("namespace MTG.Objects.Set", source);
        Assert.Contains("public static class Sets", source);
        Assert.Contains("TryGetByCode", source);
        Assert.Contains("TryGetByName", source);
        Assert.Contains("IEnumerable<SetInfo> All", source);
    }

    [Fact]
    public void Generator_SetsWithSpecialCharacters_EscapesCorrectly()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Dragon's Maze")));

        var source = GetSetsSource(result);

        Assert.Contains("DragonsMaze", source);
        Assert.Contains("Dragon's Maze", source);
    }

    [Fact]
    public void Generator_WithoutJsonFile_DoesNotGenerateCode()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(null);

        var nonAttributeFiles = result.GeneratedTrees
            .Where(t => !t.FilePath.Contains("GenerateSetsAttribute"))
            .ToList();

        Assert.Empty(nonAttributeFiles);
    }

    [Fact]
    public void Generator_WithMalformedJson_ReportsDiagnostic()
    {
        var result = GeneratorTestHelper.RunSetsGenerator("{ invalid json }");

        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(result.Diagnostics, d => d.Id == "MTGSETS002");
    }

    // ── FormatPropertyName edge cases ────────────────────────────────────

    [Fact]
    public void FormatPropertyName_ColonRemoval()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("CMM", "Commander: Masters")));

        var source = GetSetsSource(result);

        Assert.Contains("CommanderMasters", source);
    }

    [Fact]
    public void FormatPropertyName_DigitPrefix_AddsUnderscore()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("10E", "10th Edition")));

        var source = GetSetsSource(result);

        Assert.Contains("_10thEdition", source);
    }

    [Fact]
    public void FormatPropertyName_Ampersand_BecomesAnd()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("AFR", "Dungeons & Dragons")));

        var source = GetSetsSource(result);

        Assert.Contains("DungeonsAndDragons", source);
    }

    [Fact]
    public void FormatPropertyName_Slash_BecomesSpace()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Either/Or")));

        var source = GetSetsSource(result);

        Assert.Contains("EitherOr", source);
    }

    [Fact]
    public void FormatPropertyName_ApostropheRemoval()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("DGM", "Dragon's Maze")));

        var source = GetSetsSource(result);

        Assert.Contains("DragonsMaze", source);
    }

    // ── Generation logic ─────────────────────────────────────────────────

    [Fact]
    public void Generator_DuplicateSetNames_AppendCodeSuffix()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("GNT", "Game Night"), ("GN2", "Game Night")));

        var source = GetSetsSource(result);

        // Sorted by code: GN2 comes first (keeps name), GNT gets code appended
        Assert.Contains("GameNight", source);
        Assert.Contains("GameNight_GNT", source);
    }

    [Fact]
    public void Generator_MissingDataProperty_NoSetsGenerated()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(@"{ ""other"": [] }");

        var nonAttributeFiles = result.GeneratedTrees
            .Where(t => !t.FilePath.Contains("GenerateSetsAttribute"))
            .ToList();

        Assert.Empty(nonAttributeFiles);
    }

    [Fact]
    public void Generator_EmptyDataArray_GeneratesEmptySetsClass()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(@"{ ""data"": [] }");

        var source = GetSetsSource(result);

        Assert.Contains("public static class Sets", source);
        Assert.Contains("TryGetByCode", source);
    }

    [Fact]
    public void Generator_SetMissingCode_SkipsEntry()
    {
        var json = @"{
  ""data"": [
    { ""name"": ""Missing Code Set"" },
    { ""code"": ""OK1"", ""name"": ""Valid Set"" }
  ]
}";
        var result = GeneratorTestHelper.RunSetsGenerator(json);

        var source = GetSetsSource(result);

        Assert.Contains("ValidSet", source);
        Assert.DoesNotContain("MissingCodeSet", source);
    }

    [Fact]
    public void Generator_SetMissingName_SkipsEntry()
    {
        var json = @"{
  ""data"": [
    { ""code"": ""NON"" },
    { ""code"": ""OK1"", ""name"": ""Valid Set"" }
  ]
}";
        var result = GeneratorTestHelper.RunSetsGenerator(json);

        var source = GetSetsSource(result);

        Assert.Contains("ValidSet", source);
    }

    [Fact]
    public void Generator_SetsSortedByCodeAlphabetically()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("ZZZ", "Zebra Set"), ("AAA", "Alpha Set"), ("MMM", "Middle Set")));

        var source = GetSetsSource(result);

        var alphaPos = source.IndexOf("AlphaSet", StringComparison.Ordinal);
        var middlePos = source.IndexOf("MiddleSet", StringComparison.Ordinal);
        var zebraPos = source.IndexOf("ZebraSet", StringComparison.Ordinal);

        Assert.True(alphaPos < middlePos, "Alpha should come before Middle");
        Assert.True(middlePos < zebraPos, "Middle should come before Zebra");
    }

    // ── Generated code structure ─────────────────────────────────────────

    [Fact]
    public void Generator_SetInfoRecord_HasCorrectStructure()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var setInfoFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("SetInfo.g.cs"));

        Assert.NotNull(setInfoFile);
        var source = setInfoFile.ToString();

        Assert.Contains("public sealed record SetInfo(string Code, string Name)", source);
    }

    [Fact]
    public void Generator_SetsClass_ContainsAutoGeneratedComment()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var source = GetSetsSource(result);

        Assert.Contains("// <auto-generated/>", source);
    }

    [Fact]
    public void Generator_SetsClass_ContainsNullableEnable()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var source = GetSetsSource(result);

        Assert.Contains("#nullable enable", source);
    }

    [Fact]
    public void Generator_SetsClass_ContainsGeneratedCodeAttribute()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var source = GetSetsSource(result);

        Assert.Contains("[GeneratedCode(", source);
    }

    [Fact]
    public void Generator_SetsClass_ContainsAllCodesAndAllNames()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var source = GetSetsSource(result);

        Assert.Contains("IEnumerable<string> AllCodes", source);
        Assert.Contains("IEnumerable<string> AllNames", source);
    }

    [Fact]
    public void Generator_SetsClass_ContainsDocComments()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Test Set")));

        var source = GetSetsSource(result);

        Assert.Contains("/// <summary>", source);
        Assert.Contains("Attempts to retrieve set information by set code.", source);
    }

    [Fact]
    public void Generator_SetsClass_EscapesBackslashesAndQuotes()
    {
        // Use a name with a backslash to test escaping
        var json = @"{
  ""data"": [
    { ""code"": ""TST"", ""name"": ""Test\\Set"" }
  ]
}";
        var result = GeneratorTestHelper.RunSetsGenerator(json);

        var source = GetSetsSource(result);

        // The backslash should be escaped in the generated C# string
        Assert.Contains("Test\\\\Set", source);
    }

    [Fact]
    public void Generator_AttributeFileAlwaysEmitted()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(null);

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("GenerateSetsAttribute"));
    }

    [Fact]
    public void Generator_SetsWithParentheses_HandledCorrectly()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Mystery Booster (Convention)")));

        var source = GetSetsSource(result);

        Assert.Contains("MysteryBoosterConvention", source);
    }

    [Fact]
    public void Generator_SetsWithHyphens_ConvertedToPascalCase()
    {
        var result = GeneratorTestHelper.RunSetsGenerator(
            MakeJson(("TST", "Welcome-Deck-2017")));

        var source = GetSetsSource(result);

        Assert.Contains("WelcomeDeck2017", source);
    }

    private static string GetSetsSource(Microsoft.CodeAnalysis.GeneratorDriverRunResult result)
    {
        var setsFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.Contains("Sets.g.cs"));
        Assert.NotNull(setsFile);
        return setsFile.ToString();
    }
}
