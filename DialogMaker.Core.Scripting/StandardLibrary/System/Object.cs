namespace System;

public sealed class Object
{
    public virtual bool Equals(object obj) => Equals(this, obj);
    public virtual int GetHashCode() => GetHashCode(this);
    public virtual string ToString() => GetType().FullName;
    public extern Type GetType();

    public static bool Equals(object a, object b)
    {
        if (a == null && b == null)
        {
            return true;
        }

        return ReferenceEquals(a, b);
    }
    public static extern bool ReferenceEquals(object a, object b);

    private static extern int GetHashCode(object obj);
}