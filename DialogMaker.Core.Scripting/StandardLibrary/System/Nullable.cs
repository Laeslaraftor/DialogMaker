namespace System;

public struct Nullable<T> where T : struct
{
    public Nullable(T value, bool hasValue)
    {
        HasValue = hasValue;
        _value = value;
    }

    public bool HasValue { get; }
    public T Value
    {
        get
        {
            if (HasValue)
            {
                return _value;
            }

            throw new NullReferenceException();
        }
    }

    private readonly T _value;

    public T GetValueOrDefault()
    {
        if (HasValue)
        {
            return Value;
        }

        return null;
    }
    public T GetValueOrDefault(T defaultValue)
    {
        if (HasValue)
        {
            return Value;
        }

        return defaultValue;
    }

    public override string ToString()
    {
        if (HasValue)
        {
            return _value.ToString();
        }

        return Null.TextValue;
    }
    public override int GetHashCode()
    {
        if (HasValue)
        {
            return _value.GetHashCode();
        }

        return 0;
    }
    public override bool Equals(object? obj)
    {
        if (obj == null && !HasValue)
        {
            return true;
        }
        if (obj is T? nullableType)
        {
            if (nullableType.HasValue && HasValue)
            {
                return _value.Equals(nullableType._value);
            }

            return false;
        }
        else if (obj is T typedValue && HasValue)
        {
            return _value.Equals(typedValue);
        }

        return false;
    }

    public static implicit operator T?(T value) => new Nullable<T>(value, value != null);
    public static explicit operator T(T? value) => value.Value;
}