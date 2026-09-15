namespace System;

using Internal.System.Runtime;
using System.Native;

public sealed class Object
{
    public virtual bool Equals(object? obj) => ContentEquals(this, obj);
    public virtual int GetHashCode() => GetHashCode(this);
    public virtual string ToString() => GetType().FullName;
    public Type GetType()
    {
        var typeToken = CompilerServices.GetObjectTypeToken(this);
        return RuntimeHelper.CreateType(typeToken);
    }

    public static bool Equals(object? a, object? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }
        if (a == null && b != null ||
            a != null && b == null)
        {
            return false;
        }

        return a.Equals(b) || b.Equals(a);
    }
    public static bool ReferenceEquals(object? a, object? b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a == null && b != null ||
            a != null && b == null)
        {
            return false;
        }

        var aAddress = CompilerServices.GetObjectAddress(a);
        var bAddress = CompilerServices.GetObjectAddress(b);

        return aAddress == bAddress;
    }

    private static extern int GetHashCode(object obj);
    private static extern bool ContentEquals(object? a, object? b);
}