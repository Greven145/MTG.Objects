namespace MTG.Objects.SourceGenerator;

/// <summary>
/// Marker attribute to trigger generation of MTG Sets dictionary from embedded JSON data.
/// Apply this to a class or assembly to generate the Sets class.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Assembly)]
public class GenerateSetsAttribute : System.Attribute
{
}
