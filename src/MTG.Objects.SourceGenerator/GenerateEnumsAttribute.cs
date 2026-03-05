namespace MTG.Objects.SourceGenerator;

/// <summary>
/// Marker attribute to trigger generation of MTG enums from embedded JSON data.
/// Apply this to a class or namespace to generate SmartEnum classes.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Assembly)]
public class GenerateEnumsAttribute : System.Attribute
{
}
