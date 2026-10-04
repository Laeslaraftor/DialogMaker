namespace System;

public struct Double
{
    public override string ToString() => Numbers.DecimalToString((decimal)this);

    public static readonly double NaN = 0xFFF8000000000000;
}