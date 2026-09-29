using System.Linq;
using System.Native;
using System.Reflection;
using Internal.System.Runtime;

namespace System;

public class Delegate
{
    public Delegate(MethodInfo methodInfo, object? target)
    {
        _methodInfo = methodInfo;
        _runtimeMethodInfo = methodInfo.RuntimeMethodInfo;
        Target = target;
    }
    public Delegate(nint runtimeMethodInfo, object? target)
    {
        _runtimeMethodInfo = runtimeMethodInfo;
    }

    public MethodInfo? MethodInfo
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
    public object? Target { get; }

    private readonly Pointer<RuntimeMethodInfo> _runtimeMethodInfo;
    private MethodInfo? _methodInfo;

    public object? DynamicInvoke(params object?[]? parameters)
    {
        return Invoke(_runtimeMethodInfo, Target, parameters);
    }

    internal static object? Invoke(Pointer<RuntimeMethodInfo> runtimeMethodInfo, object? instance, params object?[]? parameters)
    {
        var methodInfo = runtimeMethodInfo[0];
        int availableParametersCount = 0;
        var methodInfoParamters = methodInfo.Parameters;

        for (int i = 0; i < methodInfoParamters.Length; i++)
        { 
            if (methodInfoParamters[i].Mode != DSharpMethodParameterMode.Out)
            {
                availableParametersCount++;
            }
        }
        if (parameters != null && availableParametersCount != parameters.Length)
        {
            throw new ArgumentException("Provided parameters count not match to method parameters. Required " + availableParametersCount + " parameter, but got " + parameters.Length);
        }

        return __Invoke(runtimeMethodInfo, instance, parameters);
    }

    // implemeted by compiler
    private static object? __Invoke(nint runtimeMethodInfo, object? instance, params object?[]? parameters)
    {
        // LoadLocal 0
        // LoadLocal 1
        // LoadLocal 2
        // DynamicCall
        // Return
        throw new NotImplementedException(nameof(__Invoke) + " method should be implemeted by compiler, but something went wrong and it was not implemented.");
    }
}