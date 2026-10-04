namespace System;

public static class BitConverter
{
    public static double Int64BitsToDouble(long value)
    {
        return *(double*)&value;
    }
    public static long DoubleToInt64Bits(double value)
    {
        return *(long*)&value;
    }
}