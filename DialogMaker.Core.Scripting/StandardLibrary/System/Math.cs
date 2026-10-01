namespace System;

public static class Math
{
    public static int Min(int a, int b) => a > b ? b : a;
    public static int Max(int a, int b) => a > b ? a : b;
    public static uint Min(uint a, uint b) => a > b ? b : a;
    public static uint Max(uint a, uint b) => a > b ? a : b;
    public static byte Min(byte a, byte b) => a > b ? b : a;
    public static byte Max(byte a, byte b) => a > b ? a : b;
    public static sbyte Min(sbyte a, sbyte b) => a > b ? b : a;
    public static sbyte Max(sbyte a, sbyte b) => a > b ? a : b;
    public static short Min(short a, short b) => a > b ? b : a;
    public static short Max(short a, short b) => a > b ? a : b;
    public static ushort Min(ushort a, ushort b) => a > b ? b : a;
    public static ushort Max(ushort a, ushort b) => a > b ? a : b;
    public static long Min(long a, long b) => a > b ? b : a;
    public static long Max(long a, long b) => a > b ? a : b;
    public static ulong Min(ulong a, ulong b) => a > b ? b : a;
    public static ulong Max(ulong a, ulong b) => a > b ? a : b;
    public static decimal Min(decimal a, decimal b) => a > b ? b : a;
    public static decimal Max(decimal a, decimal b) => a > b ? a : b;
    public static double Min(double a, double b) => a > b ? b : a;
    public static double Max(double a, double b) => a > b ? a : b;
    public static float Min(float a, float b) => a > b ? b : a;
    public static double Max(float a, float b) => a > b ? a : b;

    public static sbyte Abs(sbyte value) => value >= 0 ? value : (sbyte)-value;
    public static short Abs(short value) => value >= 0 ? value : (short)-value;
    public static int Abs(int value) => value >= 0 ? value : -value;
    public static long Abs(long value) => value >= 0 ? value : -value;
    public static float Abs(float value) => value >= 0 ? value : -value;
    public static double Abs(double value) => value >= 0 ? value : -value;
    public static decimal Abs(decimal value) => value >= 0 ? value : -value;
}