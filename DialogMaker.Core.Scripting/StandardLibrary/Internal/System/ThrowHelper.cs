using System;

namespace Internal.System;

internal static class ThrowHelper
{
    private static readonly string _timeSpanTooLong = "TimeSpan too long";
    private static readonly string _argumentCannotBeNaN = "Argument cannot be NaN";

    public static void ThrowArgumentOutOfRange_TimeSpanTooLong()
    {
        throw new ArgumentOutOfRangeException(_timeSpanTooLong);
    }
    public static void ThrowOverflowException_TimeSpanTooLong()
    {
        throw new OverflowException(_timeSpanTooLong);
    }
    public static void ThrowArgumentException_Arg_CannotBeNaN()
    {
        throw new ArgumentException(_argumentCannotBeNaN);
    }
    public static void ThrowOverflowException()
    {
        throw new OverflowException();
    }
}