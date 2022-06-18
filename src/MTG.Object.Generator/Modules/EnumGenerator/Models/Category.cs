using Humanizer;

namespace MTG.Object.Generator.Modules.EnumGenerator.Models;

internal record Category(string Name, IEnumerable<EnumType> Types) {
    public string Name { get; init; } = Name.Pascalize();
}