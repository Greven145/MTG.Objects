//------------------
//<auto-generated />
//------------------

using System.CodeDom.Compiler;

namespace MTG.Objects.Enum.Card;

[GeneratedCode("MTG.Object.Generator.Modules.EnumGenerator.Services.EnumCodeGenerator", "1.0.0.0")]
public sealed class ColorIndicator : SmartEnum<ColorIndicator> {
    public static readonly ColorIndicator B = new(nameof(B), 0);
    public static readonly ColorIndicator G = new(nameof(G), 1);
    public static readonly ColorIndicator R = new(nameof(R), 2);
    public static readonly ColorIndicator U = new(nameof(U), 3);
    public static readonly ColorIndicator W = new(nameof(W), 4);
    public ColorIndicator(string name, int value) : base(name, value){}
}
