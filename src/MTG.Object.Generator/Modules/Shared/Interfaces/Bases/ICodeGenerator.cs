using System.Text;

namespace MTG.Object.Generator.Modules.Shared.Interfaces.Bases;

internal interface ICodeGenerator {
    void GenerateFooter(StringBuilder classBuilder);
    void GenerateConstructor(StringBuilder classBuilder, string className);
    void GenerateHeader(StringBuilder classBuilder, string @namespace, params string[] subNamespaces);
}