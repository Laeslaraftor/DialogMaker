namespace System;

public struct UInt16
{
    public override string ToString() => Numbers.Int64ToString((long)this);
}