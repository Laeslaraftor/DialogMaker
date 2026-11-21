using Internal.System.Runtime;
using System.Native;

namespace System;

public class Object
{
    public virtual bool Equals(object? obj) => ContentEquals(this, obj);
    public virtual int GetHashCode() => GetHashCode(this);
    public virtual string ToString() => GetType().FullName;
    public unsafe Type GetType()
    {
        var typeToken = CompilerServices.GetObjectTypeToken(this);
        return RuntimeHelper.CreateType((nint)typeToken);
    }

    public static bool Equals(object? a, object? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        if (a != null && b != null)
        {
            return a.Equals(b) || b.Equals(a);
        }

        return false;
    }
    public static unsafe bool TypeEquals(object? a, object? b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a == null || b == null)
        {
            return false;
        }

        return CompilerServices.GetObjectTypeToken(a) == CompilerServices.GetObjectTypeToken(b);
    }
    public static bool ReferenceEquals(object? a, object? b)
    {
        if (a != null && b != null)
        {
            return CompilerServices.GetObjectAddress(a) == CompilerServices.GetObjectAddress(b);
        }
        
        return a == null && b == null;
    }

    private static extern int GetHashCode(object obj);
    private static extern bool ContentEquals(object? a, object? b);
}