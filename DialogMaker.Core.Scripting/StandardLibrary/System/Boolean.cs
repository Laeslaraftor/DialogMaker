namespace System;

public struct Boolean
{
    public override string ToString()
    {
        if (this)
        {
            return TrueString;
        }
        
        return FalseString;
    }
    public override int GetHashCode() => this ? 1 : 0;

    private static readonly string TrueString = "True";
    private static readonly string FalseString = "False";
}