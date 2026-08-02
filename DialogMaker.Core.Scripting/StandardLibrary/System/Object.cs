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
        if (a == null && b == null ||
            ReferenceEquals(a, b))
        {
            return true;
        }
        if (a == null && b != null ||
            a != null && b == null)
        {
            return false;
        }

        return a.Equals(b);
    }
    public static extern bool ReferenceEquals(object? a, object? b);

    private static extern int GetHashCode(object obj);
    private static extern bool ContentEquals(object? a, object? b);
}