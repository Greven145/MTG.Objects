//------------------
//<auto-generated />
//------------------

using System.CodeDom.Compiler;

namespace MTG.Objects.Enum.TcgplayerSkus;

[GeneratedCode("MTG.Object.Generator.Modules.EnumGenerator.Services.EnumCodeGenerator", "1.0.0.0")]
public sealed class Condition : SmartEnum<Condition> {
    public static readonly Condition NEARMINT = new(nameof(NEARMINT), 0);
    public static readonly Condition LIGHTLYPLAYED = new(nameof(LIGHTLYPLAYED), 1);
    public static readonly Condition MODERATELYPLAYED = new(nameof(MODERATELYPLAYED), 2);
    public static readonly Condition HEAVILYPLAYED = new(nameof(HEAVILYPLAYED), 3);
    public static readonly Condition DAMAGED = new(nameof(DAMAGED), 4);
    public static readonly Condition UNOPENED = new(nameof(UNOPENED), 5);
    public Condition(string name, int value) : base(name, value){}
}