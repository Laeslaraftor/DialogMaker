namespace System;

public static class GC
{
    public static void SuppressFinalize(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));        
        throw new NotImplementedException();
    }
    public static void Collect(int generation)
    {
        throw new NotImplementedException();
    }
}