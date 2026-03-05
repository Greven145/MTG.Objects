using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace MTG.Objects.SourceGenerator;

internal static class DiagnosticHelper
{
    internal static void ReportGeneratorException(
        SourceProductionContext context,
        System.Exception ex,
        DiagnosticDescriptor errorDescriptor,
        DiagnosticDescriptor debugDescriptor,
        string generatorName,
        string jsonFileName)
    {
        if (ex is JsonException)
        {
            context.ReportDiagnostic(Diagnostic.Create(errorDescriptor, Location.None,
                $"Failed to parse {jsonFileName}: {ex.Message}"));
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(errorDescriptor, Location.None,
                $"Failed to generate: {ex.GetType().Name}: {ex.Message}"));
        }

        context.ReportDiagnostic(Diagnostic.Create(debugDescriptor, Location.None,
            $"{generatorName}: Exception stack trace: {ex.StackTrace}"));
    }
}
