namespace System;

public struct Single
{
    public override string ToString() => Numbers.DecimalToString((decimal)this);
}