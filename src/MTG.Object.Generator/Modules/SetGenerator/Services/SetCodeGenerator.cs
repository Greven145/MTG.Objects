using System.Reflection;
using System.Text;
using MTG.Object.Generator.Modules.SetGenerator.Interfaces;
using MTG.Object.Generator.Modules.SetGenerator.Models.MTGJson;
using MTG.Object.Generator.Modules.Shared.Exceptions;
using MTG.Object.Generator.Modules.Shared.Services.Bases;

namespace MTG.Object.Generator.Modules.SetGenerator.Services;

internal class SetCodeGenerator : CodeGenerator, ISetCodeGenerator {
    public IEnumerable<Datum>? Data { get; set; }

    public override void GenerateConstructor(StringBuilder classBuilder, string className) {
        GeneratePrivateConstructor(classBuilder);
        GenerateStaticConstructor(classBuilder, className);
    }

    public override void GenerateHeader(StringBuilder classBuilder, string @namespace, params string[] subNamespaces) {
        base.GenerateHeader(classBuilder, "Set");
        classBuilder.AppendLine($"[GeneratedCode(\"MTG.Object.Generator.Modules.SetGenerator.Services.SetCodeGenerator\", \"{Assembly.GetExecutingAssembly().GetName().Version!.ToString()}\")]");
        classBuilder.AppendLine("public sealed class Sets : Dictionary<string,string> {");
        classBuilder.AppendLine("    public static readonly Sets SetList;");
    }

    private static void GeneratePrivateConstructor(StringBuilder classBuilder) {
        classBuilder.AppendLine("    private Sets(){}");
    }

    private void GenerateStaticConstructor(StringBuilder classBuilder, string className) {
        if (Data is null) {
            throw new CodeGenerationException($"Call to {nameof(GenerateConstructor)} without setting data first.");
        }

        classBuilder.AppendLine($"    static {className}() {{");
        classBuilder.AppendLine("        SetList = new Sets {");
        foreach (var datum in Data) {
            classBuilder.AppendLine($"            {{ \"{datum.Code}\", \"{datum.Name}\" }},");
        }

        classBuilder.AppendLine("        };");
        classBuilder.AppendLine("    }");
    }
}