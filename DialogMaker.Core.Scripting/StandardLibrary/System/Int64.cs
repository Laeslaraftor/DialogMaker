namespace System;

public struct Int64
{
    public override string ToString() => Numbers.Int64ToString(this);
    public override int GetHashCode() => (int)this ^ (int)(this >> 32);
}