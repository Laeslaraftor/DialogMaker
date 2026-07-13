namespace System.Reflection;

public class MethodInfo : MemberInfo
{
    public override string Name => throw new NotImplementedException();
    public override Type? DeclaringType => throw new NotImplementedException();
    public Type? ReturnType { get; }

    public object? Invoke(object? instance, object?[]? args)
    {
        return null;
    }
}