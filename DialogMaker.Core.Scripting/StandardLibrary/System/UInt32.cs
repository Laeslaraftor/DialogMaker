namespace System;

public struct UInt32
{
    public override string ToString() => Numbers.Int64ToString((long)this);
    public override int GetHashCode() => (int)this;
}