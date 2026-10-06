using System.Native;
using System.Reflection;
using Internal.System.Runtime;

namespace System;

public unsafe class MethodDelegate : Delegate
{
    public MethodDelegate(MethodInfo methodInfo, object? target)
    {
        _methodInfo = methodInfo;
        _runtimeMethodInfo = methodInfo.RuntimeMethodInfo;
        Target = target;
    }
    public MethodDelegate(nint runtimeMethodInfo, object? target)
    {
        _runtimeMethodInfo = (RuntimeMethodInfo*)runtimeMethodInfo;
        Target = target;
    }

    public override MethodInfo? MethodInfo
    {
        get
        {
            if (_methodInfo == null)
            {
                if (_runtimeMethodInfo == 0)
                {
                    throw new InvalidOperationException("Runtime method information not provided!");
                }

                _methodInfo = new(_runtimeMethodInfo);
            }

            return _methodInfo;
        }
    }
    public override object? Target { get; }

    private readonly Pointer<RuntimeMethodInfo> _runtimeMethodInfo;
    private MethodInfo? _methodInfo;

    public override object? DynamicInvoke(params object?[]? parameters)
    {
        return Invoke(_runtimeMethodInfo, Target, parameters);
    }
}