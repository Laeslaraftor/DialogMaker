namespace System;

public struct Char
{
    public override string ToString()
    {
        char[] values = new char[] { this };
        return new(values);
    }
}