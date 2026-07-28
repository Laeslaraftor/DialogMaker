namespace System;

public struct Int16
{
    public override string ToString() => long.GetString(this);
}