namespace System;

public struct Int64
{
    public override string ToString() => Numbers.Int64ToString(this);
    public override int GetHashCode() => (int)this ^ (int)(this >> 32);

    public static readonly long MaxValue = 9223372036854775807;
    public static readonly long MinValue = -9223372036854775808;
}