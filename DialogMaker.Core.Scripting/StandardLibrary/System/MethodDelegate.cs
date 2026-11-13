using System.Reflection;
using Internal.System.Runtime;

namespace System;

public unsafe class MethodDelegate : Delegate
{
    public MethodDelegate(MethodInfo methodInfo, object? target)
        : this(methodInfo)
    {
        TargetInstance = target;
    }
    public MethodDelegate(MethodInfo methodInfo)
    {
        RuntimeMethodInfo = (nint)methodInfo.RuntimeMethodInfo;
        _methodInfo = methodInfo;
    }
    public MethodDelegate(object? target, nint runtimeMethodInfo)
        : this(runtimeMethodInfo)
    {
        TargetInstance = target;
    }
    public MethodDelegate(nint runtimeMethodInfo)
    {
        RuntimeMethodInfo = runtimeMethodInfo;
    }

    public override MethodInfo MethodInfo
    {
        get
        {
            _methodInfo ??= new((RuntimeMethodInfo*)RuntimeMethodInfo);
            return _methodInfo;
        }
    }
    public override object? Target => TargetInstance;

    protected readonly nint RuntimeMethodInfo;
    protected readonly object? TargetInstance;
    private MethodInfo? _methodInfo;

    protected override nint GetRuntimeMethodInfo() => RuntimeMethodInfo;
}