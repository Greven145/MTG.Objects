using Microsoft.CodeAnalysis;
using MTG.Objects.SourceGenerator.Tests.Unit.Helpers;

namespace MTG.Objects.SourceGenerator.Tests.Unit;

public class DiagnosticHelperTests
{
    [Fact]
    public void EnumGenerator_JsonError_ContainsParseMessage()
    {
        var result = GeneratorTestHelper.RunEnumGenerator("{ bad json }");

        var errorDiag = result.Diagnostics
            .FirstOrDefault(d => d.Id == "MTGGEN002" && d.Severity == DiagnosticSeverity.Error);

        Assert.NotNull(errorDiag);
        Assert.Contains("Failed to parse EnumValues.json", errorDiag.GetMessage());
    }

    [Fact]
    public void EnumGenerator_JsonError_DebugDiagContainsStackTrace()
    {
        var result = GeneratorTestHelper.RunEnumGenerator("{ bad json }");

        var debugDiag = result.Diagnostics
            .FirstOrDefault(d => d.Id == "MTGGEN001" && d.GetMessage().Contains("stack trace"));

        Assert.NotNull(debugDiag);
    }

    [Fact]
    public void SetsGenerator_JsonError_ContainsParseMessage()
    {
        var result = GeneratorTestHelper.RunSetsGenerator("{ bad json }");

        var errorDiag = result.Diagnostics
            .FirstOrDefault(d => d.Id == "MTGSETS002" && d.Severity == DiagnosticSeverity.Error);

        Assert.NotNull(errorDiag);
        Assert.Contains("Failed to parse SetList.json", errorDiag.GetMessage());
    }

    [Fact]
    public void SetsGenerator_JsonError_DebugDiagContainsStackTrace()
    {
        var result = GeneratorTestHelper.RunSetsGenerator("{ bad json }");

        var debugDiag = result.Diagnostics
            .FirstOrDefault(d => d.Id == "MTGSETS001" && d.GetMessage().Contains("stack trace"));

        Assert.NotNull(debugDiag);
    }
}
