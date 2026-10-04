namespace System;

public struct Int32
{
    public override string ToString() => Numbers.Int64ToString((long)this);
    public override int GetHashCode() => this;

    public static readonly int MaxValue = 2147483647;
    public static readonly int MinValue = -2147483648;
}