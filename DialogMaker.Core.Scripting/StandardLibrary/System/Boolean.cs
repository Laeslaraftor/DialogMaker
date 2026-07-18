namespace System;

public struct Boolean
{
    private readonly bool _value;

    public override string ToString()
    {
        if (this)
        {
            return "True";
        }
        
        return "False";
    }
}