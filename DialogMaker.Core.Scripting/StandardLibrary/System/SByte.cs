namespace System;

public struct SByte
{
    public override string ToString() => long.GetString(this);
}