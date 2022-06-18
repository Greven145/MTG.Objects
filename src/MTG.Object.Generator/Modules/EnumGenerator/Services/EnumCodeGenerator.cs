using System.Reflection;
using System.Text;
using Humanizer;
using MTG.Object.Generator.Modules.EnumGenerator.Interfaces;
using MTG.Object.Generator.Modules.Shared.Services.Bases;

namespace MTG.Object.Generator.Modules.EnumGenerator.Services;

internal class EnumCodeGenerator : CodeGenerator, IEnumCodeGenerator {
    public override void GenerateConstructor(StringBuilder classBuilder, string className) {
        classBuilder.AppendLine($"    public {className}(string name, int value) : base(name, value){{}}");
    }

    public override void GenerateHeader(StringBuilder classBuilder, string @namespace, params string[] subNamespaces) {
        base.GenerateHeader(classBuilder, $"Enum.{@namespace}");
        classBuilder.AppendLine($"[GeneratedCode(\"MTG.Object.Generator.Modules.EnumGenerator.Services.EnumCodeGenerator\", \"{Assembly.GetExecutingAssembly().GetName().Version!.ToString()}\")]");
        classBuilder.AppendLine($"public sealed class {subNamespaces[0]} : SmartEnum<{subNamespaces[0]}> {{");
    }

    public void FormatEnumName(string name, string className, ICollection<string> values, ref int skippedOffset,
        StringBuilder classBuilder, int valueIndex) {
        var elementName = name
            .Replace('-', '_') // Keyword Jump-Start
            .Replace(".", "") // Subtype B.O.B.
            .Replace(",", "") // Subtype Biggest,
            .Replace("/", "") // Subtype And/Or,
            .Replace("?", "QuestionMark") // Subtype Elemental?,
            .Replace("’", "") // Subtype Bolas’s Meditation Realm,
            .Replace("'", "") // AbilityWord Council's Judgement
            .Replace("(", "") // Watermark Sets
            .Replace(")", "") // Watermark Sets
            .Pascalize()
            .Replace("D&d", "DnD"); // Watermark Sets

        if (className == "FrameVersion" && int.TryParse(elementName, out _)) {
            elementName = $"Frame{elementName}";
        }

        if (values.Contains(elementName)) {
            skippedOffset++;
            return;
        }

        values.Add(elementName);
        classBuilder.AppendLine(
            $"    public static readonly {className} {elementName} = new(nameof({elementName}), {valueIndex - skippedOffset});");
    }
}