namespace System;

public struct Boolean
{
    public override string ToString()
    {
        if (this)
        {
            return "True";
        }
        
        return "False";
    }
}