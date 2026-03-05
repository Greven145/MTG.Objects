using MTG.Objects.SourceGenerator.Tests.Unit.Helpers;

namespace MTG.Objects.SourceGenerator.Tests.Unit;

public class MtgEnumGeneratorTests
{
    private static string MakeEnumJson(string category, string enumName, params string[] values)
    {
        var items = string.Join(", ", values.Select(v => $@"""{v}"""));
        return $@"{{
  ""{category}"": {{
    ""{enumName}"": [{items}]
  }}
}}";
    }

    // ── Existing tests (updated to use helpers) ──────────────────────────

    [Fact]
    public void Generator_WithValidEnumJson_GeneratesClasses()
    {
        var jsonContent = @"{
  ""card"": {
    ""borderColors"": [""black"", ""white"", ""borderless""],
    ""frameVersions"": [""1993"", ""2015"", ""future""]
  }
}";
        var result = GeneratorTestHelper.RunEnumGenerator(jsonContent);

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Card.BorderColors.g.cs"));
        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Card.FrameVersions.g.cs"));
    }

    [Fact]
    public void Generator_BorderColorsEnum_ContainsExpectedValues()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "borderColors", "black", "white", "borderless", "gold", "silver"));

        var source = GetSource(result, "Card.BorderColors.g.cs");

        Assert.Contains("public static readonly BorderColors Black", source);
        Assert.Contains("public static readonly BorderColors White", source);
        Assert.Contains("public static readonly BorderColors Borderless", source);
        Assert.Contains("public static readonly BorderColors Gold", source);
        Assert.Contains("public static readonly BorderColors Silver", source);
    }

    [Fact]
    public void Generator_SpecialCharacters_AreFormattedCorrectly()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "subtypes", "B.O.B.", "And/Or", "Elemental?", "Bolas's Realm"));

        var source = GetSource(result, "Card.Subtypes.g.cs");

        Assert.Contains("BOB", source);
        Assert.Contains("AndOr", source);
        Assert.Contains("ElementalQuestionMark", source);
        Assert.Contains("BolassRealm", source);
    }

    [Fact]
    public void Generator_FrameVersion_AddsFramePrefix()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "frameVersion", "1993", "2015", "future"));

        var source = GetSource(result, "Card.FrameVersion.g.cs");

        Assert.Contains("public static readonly FrameVersion Frame1993", source);
        Assert.Contains("public static readonly FrameVersion Frame2015", source);
        Assert.Contains("public static readonly FrameVersion Future", source);
    }

    [Fact]
    public void Generator_WithoutJsonFile_DoesNotGenerateCode()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(null);

        var nonAttributeFiles = result.GeneratedTrees
            .Where(t => !t.FilePath.Contains("GenerateEnumsAttribute"))
            .ToList();

        Assert.Empty(nonAttributeFiles);
    }

    // ── FormatEnumName edge cases ────────────────────────────────────────

    [Fact]
    public void FormatEnumName_Hyphens_ConvertsToPascalCase()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "layouts", "meld-card"));

        var source = GetSource(result, "Card.Layouts.g.cs");

        Assert.Contains("MeldCard", source);
    }

    [Fact]
    public void FormatEnumName_Ampersand_BecomesAnd()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "types", "Love & Thunder"));

        var source = GetSource(result, "Card.Types.g.cs");

        Assert.Contains("LoveAndThunder", source);
    }

    [Fact]
    public void FormatEnumName_DAndD_BecomesAndForm()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "promoTypes", "D&D"));

        var source = GetSource(result, "Card.PromoTypes.g.cs");

        // D&D → "D And D" → Pascalize → "DAndD"
        Assert.Contains("DAndD", source);
    }

    [Fact]
    public void FormatEnumName_Parentheses_Removed()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "subtypes", "Token (Copy)"));

        var source = GetSource(result, "Card.Subtypes.g.cs");

        Assert.Contains("TokenCopy", source);
    }

    [Fact]
    public void FormatEnumName_NumericPrefix_NonFrameVersion_GetsUnderscore()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "types", "2headed"));

        var source = GetSource(result, "Card.Types.g.cs");

        Assert.Contains("_2headed", source);
    }

    [Fact]
    public void FormatEnumName_Commas_ConvertedToSpaces()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "types", "one,two,three"));

        var source = GetSource(result, "Card.Types.g.cs");

        Assert.Contains("OneTwoThree", source);
    }

    // ── Generation logic ─────────────────────────────────────────────────

    [Fact]
    public void Generator_MultipleCategoriesGenerateSeparateFiles()
    {
        var json = @"{
  ""card"": {
    ""colors"": [""W"", ""U""]
  },
  ""deck"": {
    ""types"": [""commander"", ""standard""]
  }
}";
        var result = GeneratorTestHelper.RunEnumGenerator(json);

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Card.Colors.g.cs"));
        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Deck.Types.g.cs"));
    }

    [Fact]
    public void Generator_EmptyArray_SkipsEnum()
    {
        var json = @"{
  ""card"": {
    ""emptyList"": [],
    ""colors"": [""W""]
  }
}";
        var result = GeneratorTestHelper.RunEnumGenerator(json);

        Assert.DoesNotContain(result.GeneratedTrees, t => t.FilePath.Contains("Card.EmptyList.g.cs"));
        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Card.Colors.g.cs"));
    }

    [Fact]
    public void Generator_DuplicateNamesAfterSanitization_Deduplicated()
    {
        // "a-b" and "a b" both become "AB" after sanitization
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "types", "a-b", "a b"));

        var source = GetSource(result, "Card.Types.g.cs");

        // Should only appear once
        var count = source.Split(new[] { "public static readonly Types AB" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Generator_NestedObject_GeneratesCompoundNamespace()
    {
        var json = @"{
  ""card"": {
    ""complex"": {
      ""subTypes"": [""arcane"", ""trap""]
    }
  }
}";
        var result = GeneratorTestHelper.RunEnumGenerator(json);

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("Card.Complex.SubTypes.g.cs"));
    }

    [Fact]
    public void Generator_WithMalformedJson_ReportsError()
    {
        var result = GeneratorTestHelper.RunEnumGenerator("{ not valid json }");

        Assert.Contains(result.Diagnostics, d => d.Id == "MTGGEN002");
    }

    // ── Generated code structure ─────────────────────────────────────────

    [Fact]
    public void Generator_ContainsAutoGeneratedComment()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "colors", "W"));

        var source = GetSource(result, "Card.Colors.g.cs");

        Assert.Contains("// <auto-generated/>", source);
    }

    [Fact]
    public void Generator_ContainsNullableEnable()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "colors", "W"));

        var source = GetSource(result, "Card.Colors.g.cs");

        Assert.Contains("#nullable enable", source);
    }

    [Fact]
    public void Generator_ContainsSmartEnumBaseClass()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "colors", "W"));

        var source = GetSource(result, "Card.Colors.g.cs");

        Assert.Contains("SmartEnum<Colors>", source);
    }

    [Fact]
    public void Generator_SequentialValueIndices()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "colors", "W", "U", "B"));

        var source = GetSource(result, "Card.Colors.g.cs");

        Assert.Contains("nameof(W), 0)", source);
        Assert.Contains("nameof(U), 1)", source);
        Assert.Contains("nameof(B), 2)", source);
    }

    [Fact]
    public void Generator_ContainsGeneratedCodeAttribute()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(
            MakeEnumJson("card", "colors", "W"));

        var source = GetSource(result, "Card.Colors.g.cs");

        Assert.Contains("[GeneratedCode(", source);
    }

    [Fact]
    public void Generator_AttributeFileAlwaysEmitted()
    {
        var result = GeneratorTestHelper.RunEnumGenerator(null);

        Assert.Contains(result.GeneratedTrees, t => t.FilePath.Contains("GenerateEnumsAttribute"));
    }

    private static string GetSource(Microsoft.CodeAnalysis.GeneratorDriverRunResult result, string fileName)
    {
        var file = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains(fileName));
        Assert.NotNull(file);
        return file.ToString();
    }
}
