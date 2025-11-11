using MTG.Objects.Set;

namespace MTG.Objects.ValueObjects;

public class Edition : ValueObject {
    private readonly string _code;
    private readonly string _name;

    private Edition(string name, string code) {
        _name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        _code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
    }

    public override string ToString() => this;

    public static (bool Successful, Edition? Edition) TryParse(ReadOnlySpan<char> input) {
        if (input.IsEmpty) {
            return (false, null);
        }

        var trimmed = input.Trim();
        var openingParenthesisIndex = trimmed.IndexOf('(');
        var closingParenthesisIndex = trimmed.IndexOf(')');


        if (openingParenthesisIndex == -1 || closingParenthesisIndex == -1) {
            return ParsePartialSetIdentifier(trimmed);
        }

        return ParseFullEditionIdentifier(trimmed, openingParenthesisIndex, closingParenthesisIndex);
    }

    public static implicit operator string(Edition edition) => $"{edition._name} ({edition._code})";

    public void Deconstruct(out string name, out string code) {
        name = _name;
        code = _code;
    }

    private static (bool successful, Edition? edition) ParsePartialSetIdentifier(ReadOnlySpan<char> trimmed) {
        var nameOrCode = trimmed.ToString();

        // Try to find by code first
        if (Sets.TryGetByCode(nameOrCode, out var setByCode) && setByCode is not null) {
            return (true, new Edition(setByCode.Name, setByCode.Code));
        }

        // Try to find by name
        if (Sets.TryGetByName(nameOrCode, out var setByName) && setByName is not null) {
            return (true, new Edition(setByName.Name, setByName.Code));
        }

        return (false, null);
    }

    private static (bool successful, Edition? edition) ParseFullEditionIdentifier(ReadOnlySpan<char> trimmed,
        int openingParenthesisIndex, int closingParenthesisIndex) {
        var name = trimmed[..openingParenthesisIndex].TrimEnd();
        var code = trimmed[(openingParenthesisIndex + 1)..closingParenthesisIndex].Trim();

        var codeAsString = code.ToString();
        var nameAsString = name.ToString();

        // Validate that the code exists and matches the name
        if (Sets.TryGetByCode(codeAsString, out var set) && set is not null && set.Name == nameAsString) {
            return (true, new Edition(nameAsString, codeAsString));
        }

        return (false, null);
    }
}