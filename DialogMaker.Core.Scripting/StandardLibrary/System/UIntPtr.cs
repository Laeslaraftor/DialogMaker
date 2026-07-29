namespace System;

public struct UIntPtr
{
    public override string ToString() => Numbers.UInt64ToString((ulong)this);
}