using System.Text;
using MTG.Object.Generator.Modules.Shared.Interfaces.Bases;

namespace MTG.Object.Generator.Modules.EnumGenerator.Interfaces;

internal interface IEnumCodeGenerator : ICodeGenerator {
    void FormatEnumName(string name, string className, ICollection<string> values, ref int skippedOffset,
        StringBuilder classBuilder, int valueIndex);
}