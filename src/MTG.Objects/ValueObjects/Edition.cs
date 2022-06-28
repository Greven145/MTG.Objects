using MTG.Objects.Set;

namespace MTG.Objects.ValueObjects;

public class Edition : ValueObject
{
    private static readonly Dictionary<string, string> CodeToName;
    private static readonly Dictionary<string, string> NameToCode;
    private readonly string _code;

    private readonly string _name;

    static Edition()
    {
        CodeToName = Sets.SetList;
        NameToCode =
            new Dictionary<string, string>(CodeToName.Select(kv => new KeyValuePair<string, string>(kv.Value, kv.Key)));
    }

    private Edition(string name, string code)
    {
        _name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        _code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
    }

    public static (bool Successful, Edition? Edition) TryParse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
        {
            return (false, null);
        }

        var trimmed = input.Trim();
        var openingParenthesisIndex = trimmed.IndexOf('(');
        var closingParenthesisIndex = trimmed.IndexOf(')');


        if (openingParenthesisIndex == -1 || closingParenthesisIndex == -1)
        {
            return ParsePartialSetIdentifier(trimmed);
        }

        return ParseFullEditionIdentifier(trimmed, openingParenthesisIndex, closingParenthesisIndex);
    }

    public static implicit operator string(Edition edition)
    {
        return $"{edition._name} ({edition._code})";
    }

    public void Deconstruct(out string name, out string code)
    {
        name = _name;
        code = _code;
    }

    private static (bool successful, Edition? edition) ParsePartialSetIdentifier(ReadOnlySpan<char> trimmed)
    {
        var nameOrCode = trimmed.ToString();

        if (CodeToName.TryGetValue(nameOrCode, out var name))
        {
            return (true, new Edition(name, nameOrCode));
        }

        if (NameToCode.TryGetValue(nameOrCode, out var code))
        {
            return (true, new Edition(nameOrCode, code));
        }

        return (false, null);
    }

    private static (bool successful, Edition? edition) ParseFullEditionIdentifier(ReadOnlySpan<char> trimmed,
        int openingParenthesisIndex, int closingParenthesisIndex)
    {
        var name = trimmed[..openingParenthesisIndex].TrimEnd();
        var code = trimmed[(openingParenthesisIndex + 1)..closingParenthesisIndex].Trim();

        var codeAsString = code.ToString();

        if (CodeToName.TryGetValue(codeAsString, out var dictName) && dictName == name.ToString())
        {
            return (true, new Edition(name.ToString(), codeAsString));
        }

        return (false, null);
    }
}
