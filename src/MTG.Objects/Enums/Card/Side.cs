//------------------
//<auto-generated />
//------------------

using System.CodeDom.Compiler;

namespace MTG.Objects.Enum.Card;

[GeneratedCode("MTG.Object.Generator.Modules.EnumGenerator.Services.EnumCodeGenerator", "1.0.0.0")]
public sealed class Side : SmartEnum<Side> {
    public static readonly Side A = new(nameof(A), 0);
    public static readonly Side B = new(nameof(B), 1);
    public static readonly Side C = new(nameof(C), 2);
    public static readonly Side D = new(nameof(D), 3);
    public static readonly Side E = new(nameof(E), 4);
    public Side(string name, int value) : base(name, value){}
}
