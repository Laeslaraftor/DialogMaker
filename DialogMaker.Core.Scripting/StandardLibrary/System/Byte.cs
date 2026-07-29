namespace System;

public struct Byte
{
    public override string ToString() => Numbers.Int64ToString((long)this);
}