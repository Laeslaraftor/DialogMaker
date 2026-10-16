namespace System;

public struct Int32
{
    public override string ToString() => Numbers.Int64ToString((long)this);
    public override int GetHashCode() => this;

    public static readonly int MaxValue = 2147483647;
    public static readonly int MinValue = -2147483648;

    public static bool TryParse(string value, out int result)
    {
        result = 0;

        for (int i = 0; i < value.Length; i++)
        {
            if (i > 0)
            {
                result *= 10;
            }

            char symbol = value[i];

            if (char.TryParseNumber(symbol, out int number))
            {
                result += number;
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}