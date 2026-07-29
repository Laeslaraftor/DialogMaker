namespace System;

public struct Int32
{
    public override string ToString() => Numbers.Int64ToString((long)this);
}