using System.Reflection;

namespace System;

public abstract class Delegate
{
    protected Delegate(MethodInfo methodInfo, object? target)
    {
        MethodInfo = methodInfo;
        Target = target;
    }

    public MethodInfo MethodInfo { get; }
    public object? Target { get; }

    public object? DynamicInvoke(object? instance, params object?[]? parameters)
    {
        return MethodInfo.Invoke(Target, parameters);
    }
}