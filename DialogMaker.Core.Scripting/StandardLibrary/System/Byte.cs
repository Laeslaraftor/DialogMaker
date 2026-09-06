namespace System;

public struct Byte
{
    public override string ToString() => Numbers.Int64ToString((long)this);

    public static readonly byte MinValue = 0;
    public static readonly byte MaxValue = 255;
}