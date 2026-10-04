namespace System;

public struct UInt64
{
    public override string ToString() => Numbers.UInt64ToString(this);
    public override int GetHashCode() => (int)this ^ (int)(this >> 32);
}