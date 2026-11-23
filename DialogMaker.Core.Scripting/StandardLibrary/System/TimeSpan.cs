using Internal.System;

namespace System;

public struct TimeSpan
{
    public TimeSpan(long ticks)
    {
        _ticks = ticks;
    }
    public TimeSpan(int hours, int minutes, int seconds)
    {
        _ticks = TimeToTicks(hours, minutes, seconds);
    }
    public TimeSpan(int days, int hours, int minutes, int seconds)
        : this(days, hours, minutes, seconds, 0)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// days, hours, minutes, seconds, and milliseconds.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <remarks>
    /// The specified <paramref name="days"/>, <paramref name="hours"/>, <paramref name="minutes"/>, <paramref name="seconds"/>
    /// and <paramref name="milliseconds"/> are converted to ticks, and that value initializes this instance.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds) :
        this(days, hours, minutes, seconds, milliseconds, 0)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// days, hours, minutes, seconds, and milliseconds.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <remarks>
    /// The specified <paramref name="days"/>, <paramref name="hours"/>, <paramref name="minutes"/>, <paramref name="seconds"/>
    /// <paramref name="milliseconds"/> and <paramref name="microseconds"/> are converted to ticks, and that value initializes this instance.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds, int microseconds)
    {
        long totalMicroseconds = (days * MicrosecondsPerDay)
                               + (hours * MicrosecondsPerHour)
                               + (minutes * MicrosecondsPerMinute)
                               + (seconds * MicrosecondsPerSecond)
                               + (milliseconds * MicrosecondsPerMillisecond)
                               + microseconds;

        if ((totalMicroseconds > MaxMicroseconds) || (totalMicroseconds < MinMicroseconds))
        {
            ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
        }

        _ticks = totalMicroseconds * TicksPerMicrosecond;
    }

    public int Days => (int)(_ticks / TicksPerDay);
    public int Hours => (int)(_ticks / TicksPerHour % HoursPerDay);
    public int Milliseconds => (int)(_ticks / TicksPerMillisecond % MillisecondsPerSecond);
    /// <summary>
    /// Gets the microseconds component of the time interval represented by the current <see cref="TimeSpan"/> structure.
    /// </summary>
    /// <remarks>
    /// The <see cref="Microseconds"/> property represents whole microseconds, whereas the
    /// <see cref="TotalMicroseconds"/> property represents whole and fractional microseconds.
    /// </remarks>
    public int Microseconds => (int)(_ticks / TicksPerMicrosecond % MicrosecondsPerMillisecond);
    /// <summary>
    /// Gets the nanoseconds component of the time interval represented by the current <see cref="TimeSpan"/> structure.
    /// </summary>
    /// <remarks>
    /// The <see cref="Nanoseconds"/> property represents whole nanoseconds, whereas the
    /// <see cref="TotalNanoseconds"/> property represents whole and fractional nanoseconds.
    /// </remarks>
    public int Nanoseconds => (int)(_ticks % TicksPerMicrosecond * NanosecondsPerTick);
    public int Minutes => (int)(_ticks / TicksPerMinute % MinutesPerHour);
    public int Seconds => (int)(_ticks / TicksPerSecond % SecondsPerMinute);
    public double TotalDays => (double)_ticks / TicksPerDay;
    public double TotalHours => (double)_ticks / TicksPerHour;
    public double TotalMilliseconds
    {
        get
        {
            double temp = (double)_ticks / TicksPerMillisecond;

            if (temp > MaxMilliseconds)
            {
                return MaxMilliseconds;
            }

            if (temp < MinMilliseconds)
            {
                return MinMilliseconds;
            }
            return temp;
        }
    }
    /// <summary>
    /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole and fractional microseconds.
    /// </summary>
    /// <remarks>
    /// This property converts the value of this instance from ticks to microseconds.
    /// This number might include whole and fractional microseconds.
    ///
    /// The <see cref="TotalMicroseconds"/> property represents whole and fractional microseconds,
    /// whereas the <see cref="Microseconds"/> property represents whole microseconds.
    /// </remarks>
    public double TotalMicroseconds => (double)_ticks / TicksPerMicrosecond;
    /// <summary>
    /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole and fractional nanoseconds.
    /// </summary>
    /// <remarks>
    /// This property converts the value of this instance from ticks to nanoseconds.
    /// This number might include whole and fractional nanoseconds.
    ///
    /// The <see cref="TotalNanoseconds"/> property represents whole and fractional nanoseconds,
    /// whereas the <see cref="Nanoseconds"/> property represents whole nanoseconds.
    /// </remarks>
    public double TotalNanoseconds => (double)_ticks * NanosecondsPerTick;
    public double TotalMinutes => (double)_ticks / TicksPerMinute;
    public double TotalSeconds => (double)_ticks / TicksPerSecond;

    internal readonly long _ticks;

    /// <summary>
    /// Represents the number of nanoseconds per tick. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 100.
    /// </remarks>
    public static readonly long NanosecondsPerTick = 100;
    /// <summary>
    /// Represents the number of ticks in 1 microsecond. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 10.
    /// </remarks>
    public static readonly long TicksPerMicrosecond = 10;
    /// <summary>
    /// Represents the number of ticks in 1 millisecond. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 10 thousand; that is, 10,000.
    /// </remarks>
    public static readonly long TicksPerMillisecond = TicksPerMicrosecond * 1000;
    /// <summary>
    /// Represents the number of ticks in 1 second. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 10 million; that is, 10,000,000.
    /// </remarks>
    public static readonly long TicksPerSecond = TicksPerMillisecond * 1000;
    /// <summary>
    /// Represents the number of ticks in 1 minute. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 600 million; that is, 600,000,000.
    /// </remarks>
    public static readonly long TicksPerMinute = TicksPerSecond * 60;
    /// <summary>
    /// Represents the number of ticks in 1 hour. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 36 billion; that is, 36,000,000,000.
    /// </remarks>
    public static readonly long TicksPerHour = TicksPerMinute * 60;
    /// <summary>
    /// Represents the number of ticks in 1 day. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 864 billion; that is, 864,000,000,000.
    /// </remarks>
    public static readonly long TicksPerDay = TicksPerHour * 24;
    /// <summary>
    /// Represents the number of microseconds in 1 millisecond. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 1 thousand; that is, 1,000.
    /// </remarks>
    public static readonly long MicrosecondsPerMillisecond = TicksPerMillisecond / TicksPerMicrosecond;
    /// <summary>
    /// Represents the number of microseconds in 1 second. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 1 million; that is, 1,000,000.
    /// </remarks>
    public static readonly long MicrosecondsPerSecond = TicksPerSecond / TicksPerMicrosecond;
    /// <summary>
    /// Represents the number of microseconds in 1 minute. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 60 million; that is, 60,000,000.
    /// </remarks>
    public static readonly long MicrosecondsPerMinute = TicksPerMinute / TicksPerMicrosecond;
    /// <summary>
    /// Represents the number of microseconds in 1 hour. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 3.6 billion; that is, 3,600,000,000.
    /// </remarks>
    public static readonly long MicrosecondsPerHour = TicksPerHour / TicksPerMicrosecond;
    /// <summary>
    /// Represents the number of microseconds in 1 day. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 86.4 billion; that is, 86,400,000,000.
    /// </remarks>
    public static readonly long MicrosecondsPerDay = TicksPerDay / TicksPerMicrosecond;
    /// <summary>
    /// Represents the number of milliseconds in 1 second. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 1 thousand; that is, 1,000.
    /// </remarks>
    public static readonly long MillisecondsPerSecond = TicksPerSecond / TicksPerMillisecond;
    /// <summary>
    /// Represents the number of milliseconds in 1 minute. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 60 thousand; that is, 60,000.
    /// </remarks>
    public static readonly long MillisecondsPerMinute = TicksPerMinute / TicksPerMillisecond;
    /// <summary>
    /// Represents the number of milliseconds in 1 hour. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 3.6 million; that is, 3,600,000.
    /// </remarks>
    public static readonly long MillisecondsPerHour = TicksPerHour / TicksPerMillisecond;
    /// <summary>
    /// Represents the number of milliseconds in 1 day. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 86.4 million; that is, 86,400,000.
    /// </remarks>
    public static readonly long MillisecondsPerDay = TicksPerDay / TicksPerMillisecond;
    /// <summary>
    /// Represents the number of seconds in 1 minute. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 60.
    /// </remarks>
    public static readonly long SecondsPerMinute = TicksPerMinute / TicksPerSecond;
    /// <summary>
    /// Represents the number of seconds in 1 hour. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 3.6 thousand; that is, 3,600.
    /// </remarks>
    public static readonly long SecondsPerHour = TicksPerHour / TicksPerSecond;
    /// <summary>
    /// Represents the number of seconds in 1 day. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 86.4 thousand; that is, 86,400.
    /// </remarks>
    public static readonly long SecondsPerDay = TicksPerDay / TicksPerSecond;
    /// <summary>
    /// Represents the number of minutes in 1 hour. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 60.
    /// </remarks>
    public static readonly long MinutesPerHour = TicksPerHour / TicksPerMinute;
    /// <summary>
    /// Represents the number of minutes in 1 day. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 1440.
    /// </remarks>
    public static readonly long MinutesPerDay = TicksPerDay / TicksPerMinute;
    /// <summary>
    /// Represents the number of hours in 1 day. This field is static readonlyant.
    /// </summary>
    /// <remarks>
    /// The value of this static readonlyant is 24.
    /// </remarks>
    public static readonly int HoursPerDay = (int)(TicksPerDay / TicksPerHour);
    internal static readonly long MinTicks = long.MinValue;
    internal static readonly long MaxTicks = long.MaxValue;
    internal static readonly long MinMicroseconds = MinTicks / TicksPerMicrosecond;
    internal static readonly long MaxMicroseconds = MaxTicks / TicksPerMicrosecond;
    internal static readonly long MinMilliseconds = MinTicks / TicksPerMillisecond;
    internal static readonly long MaxMilliseconds = MaxTicks / TicksPerMillisecond;
    internal static readonly long MinSeconds = MinTicks / TicksPerSecond;
    internal static readonly long MaxSeconds = MaxTicks / TicksPerSecond;
    internal static readonly long MinMinutes = MinTicks / TicksPerMinute;
    internal static readonly long MaxMinutes = MaxTicks / TicksPerMinute;
    internal static readonly long MinHours = MinTicks / TicksPerHour;
    internal static readonly long MaxHours = MaxTicks / TicksPerHour;
    internal static readonly long MinDays = MinTicks / TicksPerDay;
    internal static readonly long MaxDays = MaxTicks / TicksPerDay;
    internal static readonly long TicksPerTenthSecond = TicksPerMillisecond * 100;
    public static readonly TimeSpan Zero = new(0);
    public static readonly TimeSpan MaxValue = new(MaxTicks);
    public static readonly TimeSpan MinValue = new(MinTicks);

    internal static long TimeToTicks(int hour, int minute, int second)
    {
        // totalSeconds is bounded by 2^31 * 2^12 + 2^31 * 2^8 + 2^31,
        // which is less than 2^44, meaning we won't overflow totalSeconds.
        long totalSeconds = (hour * SecondsPerHour)
                          + (minute * SecondsPerMinute)
                          + second;

        if ((totalSeconds > MaxSeconds) || (totalSeconds < MinSeconds))
        {
            ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
        }
        return totalSeconds * TicksPerSecond;
    }

    private static TimeSpan FromUnits(long units, long ticksPerUnit, long minUnits, long maxUnits)
    {
        if (units > maxUnits || units < minUnits)
        {
            ThrowHelper.ThrowArgumentOutOfRange_TimeSpanTooLong();
        }
        return TimeSpan.FromTicks(units * ticksPerUnit);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// days.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of days.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromDays(int days) => FromUnits(days, TicksPerDay, MinDays, MaxDays);
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// days, hours, minutes, seconds, milliseconds, and microseconds.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromDays(int days, int hours = 0, long minutes = 0, long seconds = 0, long milliseconds = 0, long microseconds = 0)
    {
        long totalMicroseconds = (days * MicrosecondsPerDay)
                                 + (hours * MicrosecondsPerHour)
                                 + (minutes * MicrosecondsPerMinute)
                                 + (seconds * MicrosecondsPerSecond)
                                 + (milliseconds * MicrosecondsPerMillisecond)
                                 + microseconds;

        return FromMicroseconds(totalMicroseconds);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// hours.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of hours.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromHours(int hours) => FromUnits(hours, TicksPerHour, MinHours, MaxHours);
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// hours, minutes, seconds, milliseconds, and microseconds.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of hours, minutes, seconds, milliseconds, and microseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromHours(int hours, long minutes = 0, long seconds = 0, long milliseconds = 0, long microseconds = 0)
    {
        long totalMicroseconds = (hours * MicrosecondsPerHour)
                                 + (minutes * MicrosecondsPerMinute)
                                 + (seconds * MicrosecondsPerSecond)
                                 + (milliseconds * MicrosecondsPerMillisecond)
                                 + microseconds;

        return FromMicroseconds(totalMicroseconds);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// minutes.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of minutes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromMinutes(long minutes) => FromUnits(minutes, TicksPerMinute, MinMinutes, MaxMinutes);
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// minutes, seconds, milliseconds, and microseconds.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of minutes, seconds, milliseconds, and microseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromMinutes(long minutes, long seconds = 0, long milliseconds = 0, long microseconds = 0)
    {
        long totalMicroseconds = (minutes * MicrosecondsPerMinute)
                                 + (seconds * MicrosecondsPerSecond)
                                 + (milliseconds * MicrosecondsPerMillisecond)
                                 + microseconds;

        return FromMicroseconds(totalMicroseconds);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// seconds.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of seconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromSeconds(long seconds) => FromUnits(seconds, TicksPerSecond, MinSeconds, MaxSeconds);
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// seconds, milliseconds, and microseconds.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of seconds, milliseconds, and microseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromSeconds(long seconds, long milliseconds = 0, long microseconds = 0)
    {
        long totalMicroseconds = (seconds * MicrosecondsPerSecond)
                                 + (milliseconds * MicrosecondsPerMillisecond)
                                 + microseconds;

        return FromMicroseconds(totalMicroseconds);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// milliseconds.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of milliseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameter specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromMilliseconds(long milliseconds) => FromUnits(milliseconds, TicksPerMillisecond, MinMilliseconds, MaxMilliseconds);
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// milliseconds, and microseconds.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of milliseconds, and microseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromMilliseconds(long milliseconds, long microseconds)
    {
        long totalMicroseconds = (milliseconds * MicrosecondsPerMillisecond) + microseconds;
        return FromMicroseconds(totalMicroseconds);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpan"/> structure to a specified number of
    /// microseconds.
    /// </summary>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>Returns a <see cref="TimeSpan"/> that represents a specified number of microseconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The parameters specify a <see cref="TimeSpan"/> value less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>
    /// </exception>
    public static TimeSpan FromMicroseconds(long microseconds) => FromUnits(microseconds, TicksPerMicrosecond, MinMicroseconds, MaxMicroseconds);
    public static TimeSpan FromHours(double value) => Interval(value, TicksPerHour);
    private static TimeSpan Interval(double value, double scale)
    {
        if (double.IsNaN(value))
        {
            ThrowHelper.ThrowArgumentException_Arg_CannotBeNaN();
        }
        return IntervalFromDoubleTicks(value * scale);
    }
    private static TimeSpan IntervalFromDoubleTicks(double ticks)
    {
        if ((ticks > MaxTicks) || (ticks < MinTicks) || double.IsNaN(ticks))
        {
            ThrowHelper.ThrowOverflowException_TimeSpanTooLong();
        }
        if (ticks == MaxTicks)
        {
            return MaxValue;
        }
        return new TimeSpan((long)ticks);
    }
    public static TimeSpan FromMilliseconds(double value) => Interval(value, TicksPerMillisecond);
    /// <summary>
    /// Returns a <see cref="TimeSpan"/> that represents a specified number of microseconds.
    /// </summary>
    /// <param name="value">A number of microseconds.</param>
    /// <returns>An object that represents <paramref name="value"/>.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
    ///
    /// -or-
    ///
    /// <paramref name="value"/> is <see cref="double.PositiveInfinity"/>
    ///
    /// -or-
    ///
    /// <paramref name="value"/> is <see cref="double.NegativeInfinity"/>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is equal to <see cref="double.NaN"/>.
    /// </exception>
    public static TimeSpan FromMicroseconds(double value) => Interval(value, TicksPerMicrosecond);
    public static TimeSpan FromMinutes(double value) => Interval(value, TicksPerMinute);
    public static TimeSpan FromSeconds(double value) => Interval(value, TicksPerSecond);
    public static TimeSpan FromTicks(long ticks) => new(ticks);

    public static TimeSpan operator +(TimeSpan left, TimeSpan right) => new(left._ticks + right._ticks);
    public static TimeSpan operator -(TimeSpan left, TimeSpan right) => new(left._ticks - right._ticks);
    public static TimeSpan operator *(TimeSpan left, TimeSpan right) => new(left._ticks * right._ticks);
    public static TimeSpan operator *(TimeSpan left, double right) => new((long)(left._ticks * right));
    public static TimeSpan operator /(TimeSpan left, TimeSpan right) => new(left._ticks / right._ticks);
    public static bool operator >(TimeSpan left, TimeSpan right) => left._ticks > right._ticks;
    public static bool operator >=(TimeSpan left, TimeSpan right) => left._ticks >= right._ticks;
    public static bool operator <(TimeSpan left, TimeSpan right) => left._ticks < right._ticks;
    public static bool operator <=(TimeSpan left, TimeSpan right) => left._ticks <= right._ticks;
}