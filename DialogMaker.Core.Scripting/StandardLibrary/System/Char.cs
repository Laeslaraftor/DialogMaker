namespace System;

public struct Char
{
    public override string ToString()
    {
        Span<char> values = stackalloc char[] { this };
        return new(values);
    }
    public override int GetHashCode() => (int)this;

    public static bool IsDigit(char value) => value >= '0' && value <= '9';
    public static bool TryParseDigit(char value, out int result)
    {
        if (value >= '0' && value <= '9')
        {
            result = value - '0';
            return true;
        }

        result = 0;
        return false;
    }
}