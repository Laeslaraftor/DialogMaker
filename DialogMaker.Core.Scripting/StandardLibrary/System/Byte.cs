namespace System;

public struct Byte
{
    public override string ToString() => long.GetString(this);
}