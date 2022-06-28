using System.Globalization;

namespace MTG.Objects.ValueObjects;

public class NumberOfCards : ValueObject
{
    private readonly int _number;

    public NumberOfCards(int numberOfCards)
    {
        _number = Guard.Against.Negative(numberOfCards, nameof(numberOfCards));
    }

    public static implicit operator int(NumberOfCards card)
    {
        return card._number;
    }

    public static explicit operator NumberOfCards(int numberOfCards)
    {
        return new NumberOfCards(numberOfCards);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), _number);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((NumberOfCards)obj);
    }

    public override string ToString()
    {
        return _number.ToString(new NumberFormatInfo());
    }

    protected virtual bool Equals(NumberOfCards other)
    {
        return _number == other._number;
    }
}
