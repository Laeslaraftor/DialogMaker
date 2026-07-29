namespace System;

public struct Double
{
    public override string ToString() => Numbers.DecimalToString((decimal)this);
}