namespace System;

public struct SByte
{
    public override string ToString() => Numbers.Int64ToString((long)this);
}