namespace System;

public struct Char
{
    public override string ToString()
    {
        Span<char> values = stackalloc char[] { this };
        return new(values);
    }
}