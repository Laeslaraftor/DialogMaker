namespace System;

public static unsafe class BitConverter
{
    public static readonly bool IsLittleEndian = GetIsLittleEndian();

    public static double Int64BitsToDouble(long value)
    {
        return *(double*)&value;
    }
    public static long DoubleToInt64Bits(double value)
    {
        return *(long*)&value;
    }

    private static extern bool GetIsLittleEndian();
}