namespace System.Reflection;

public abstract class MemberInfo
{
    public abstract string Name { get; }
    public abstract Type? DeclaringType { get; }
}