namespace System;

public struct Int16
{
    public override string ToString() => Numbers.Int64ToString((long)this);
}