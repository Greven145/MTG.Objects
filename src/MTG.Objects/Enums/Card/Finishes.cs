//------------------
//<auto-generated />
//------------------

using System.CodeDom.Compiler;

namespace MTG.Objects.Enum.Card;

[GeneratedCode("MTG.Object.Generator.Modules.EnumGenerator.Services.EnumCodeGenerator", "1.0.0.0")]
public sealed class Finishes : SmartEnum<Finishes> {
    public static readonly Finishes Etched = new(nameof(Etched), 0);
    public static readonly Finishes Foil = new(nameof(Foil), 1);
    public static readonly Finishes Glossy = new(nameof(Glossy), 2);
    public static readonly Finishes Nonfoil = new(nameof(Nonfoil), 3);
    public static readonly Finishes Signed = new(nameof(Signed), 4);
    public Finishes(string name, int value) : base(name, value){}
}
