namespace System;

public class Random
{
    public Random(int seed)
    {
        int ii = 0;
        int mj = 0;
        int mk = 0;

        int subtraction = (seed == int.MinValue) ? int.MaxValue : Math.Abs(seed);
        mj = MagicSeed - subtraction;
        _seedArray[55] = mj;
        mk = 1;

        for (int i = 1; i < 55; i++)
        {
            ii = (21 * i) % 55;
            _seedArray[ii] = mk;
            mk = mj - mk;
            
            if (mk < 0)
            {
                mk += MagicBig;
            }

            mj = _seedArray[ii];
        }
        for (int k = 1; k < 5; k++)
        {
            for (int i = 1; i < 56; i++)
            {
                _seedArray[i] -= _seedArray[1 + (i + 30) % 55];

                if (_seedArray[i] < 0)
                {
                    _seedArray[i] += MagicBig;
                }
            }
        }

        _inext = 0;
        _inextp = 21;
    }
    public Random() : this(Environment.TickCount)
    {
    }

    private readonly int[] _seedArray = new int[56];
    private int _inext;
    private int _inextp;

    protected virtual double Sample()
    {
        int locINext = _inext;

        if (++locINext >= 56)
        {
            locINext = 1;
        }

        int locINextp = _inextp;

        if (++locINextp >= 56)
        {
            locINextp = 1;
        }

        int retVal = _seedArray[locINext] - _seedArray[locINextp];

        if (retVal == MagicBig)
        {
            retVal--;
        }
        if (retVal < 0)
        {
            retVal += MagicBig;
        }

        _seedArray[locINext] = retVal;
        _inext = locINext;
        _inextp = locINextp;

        return retVal * (1.0 / MagicBig);
    }

    public virtual int Next() => (int)(Sample() * MagicBig);
    public virtual int Next(int maxValue)
    {
        if (0 > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue));
        }

        return (int)(Sample() * maxValue);
    }

    public virtual int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(minValue));
        }

        long range = (long)maxValue - minValue;

        if (range <= int.MaxValue)
        {
            return (int)(Sample() * range) + minValue;
        }
        
        return (int)((long)(GetSampleForLargeRange() * range) + minValue);
    }
    public virtual double NextDouble() => Sample();

    // Для диапазонов больше int.MaxValue
    private double GetSampleForLargeRange()
    {
        int result = Next();

        if (Next() % 2 == 0)
        {
             result = -result;
        }

        double d = result;
        d += int.MaxValue - 1;
        d /= 2 * (uint)int.MaxValue - 1;

        return d;
    }

    public virtual void NextBytes(byte[] buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)Next(256);
        }
    }

    public static Random Shared
    {
        get
        {
            field ??= new();
            return field;
        }
    }

    private static readonly int MagicBig = int.MaxValue;
    private static readonly int MagicSeed = 161803398;
    private static readonly int MZ = 0;
}