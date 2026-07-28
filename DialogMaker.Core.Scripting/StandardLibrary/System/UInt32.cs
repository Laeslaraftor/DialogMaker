namespace System;

public struct UInt32
{
    public override string ToString() => long.GetString((long)this);
}