namespace System;

public struct UInt16
{
    public override string ToString() => long.GetString((long)this);
}