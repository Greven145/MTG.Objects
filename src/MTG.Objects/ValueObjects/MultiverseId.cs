namespace MTG.Objects.ValueObjects;

public class MultiverseId : ValueObject
{
    private readonly int _id;

    public MultiverseId(int id)
    {
        _id = Guard.Against.Negative(id, nameof(id));
    }

    public static implicit operator int(MultiverseId card)
    {
        return card._id;
    }

    public static explicit operator MultiverseId(int id)
    {
        return new MultiverseId(id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), _id);
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

        return Equals((MultiverseId)obj);
    }

    protected bool Equals(MultiverseId other)
    {
        return _id == other._id;
    }
}
