using System.Globalization;

namespace MTG.Objects.ValueObjects;

public class NumberOfCards : ValueObject {
    private readonly int _number;

    public NumberOfCards(int numberOfCards) => _number = Guard.Against.Negative(numberOfCards, nameof(numberOfCards));

    public static implicit operator int(NumberOfCards card) => card._number;

    public static explicit operator NumberOfCards(int numberOfCards) => new(numberOfCards);

    protected bool Equals(NumberOfCards other) => _number == other._number;

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _number);

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        if (obj.GetType() != this.GetType()) {
            return false;
        }

        return Equals((NumberOfCards)obj);
    }

    public override string ToString() => _number.ToString(new NumberFormatInfo());
}
