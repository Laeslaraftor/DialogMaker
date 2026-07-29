namespace System;

public struct IntPtr
{
    public override string ToString() => Numbers.Int64ToString((long)this);
}