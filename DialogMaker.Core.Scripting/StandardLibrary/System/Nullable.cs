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

    public static implicit operator T?(T value) => new(value, value != null);
    public static explicit operator T(T? value) => value.Value;
}