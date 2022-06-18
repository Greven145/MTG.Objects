using Humanizer;

namespace MTG.Object.Generator.Modules.EnumGenerator.Models;

internal record EnumType(string Name, IEnumerable<string> Values) {
    public string Name { get; init; } = Name.Pascalize();
}