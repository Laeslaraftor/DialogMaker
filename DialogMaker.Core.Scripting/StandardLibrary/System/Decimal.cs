namespace System;

public struct Decimal
{
    public override string ToString() => Numbers.DecimalToString(this);
}