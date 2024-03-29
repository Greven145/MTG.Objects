﻿using System.Globalization;

namespace MTG.Objects.ValueObjects;

public class MultiverseId : ValueObject {
    private readonly int _id;

    public MultiverseId(int id) {
        _id = Guard.Against.Negative(id, nameof(id));
    }

    public override string ToString() => this;

    public static implicit operator int(MultiverseId card) => card._id;

    public static implicit operator string(MultiverseId card) => card._id.ToString(new NumberFormatInfo());

    public static explicit operator MultiverseId(int id) => new(id);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _id);

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        if (obj.GetType() != GetType()) {
            return false;
        }

        return Equals((MultiverseId)obj);
    }

    protected virtual bool Equals(MultiverseId other) => _id == other._id;
}