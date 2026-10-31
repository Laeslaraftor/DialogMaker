using System.Native;
using System.Reflection;
using Internal.System.Runtime;

namespace System;

public abstract unsafe class Delegate
{
    public abstract MethodInfo MethodInfo { get; }
    public abstract object? Target { get; }

    public object? DynamicInvoke(params object?[]? parameters)
    {
        var invocationList = GetInvocationList();
        int length = invocationList.Length;
        object? result = null;

        for (int i = 0; i < invocationList.Length; i++)
        {
            var @delegate = invocationList[i];

            if (@delegate is MulticastDelegate)
            {
                @delegate.DynamicInvoke(parameters);
                continue;
            }

            var delegateRuntimeMethodInfo = @delegate.GetRuntimeMethodInfo();

            result = Invoke((RuntimeMethodInfo*)delegateRuntimeMethodInfo, @delegate.Target, parameters);
        }

        return result;
    }
    public virtual Delegate[] GetInvocationList()
    {
        return new Delegate[] { this };
    }

    protected abstract nint GetRuntimeMethodInfo();
    protected virtual Delegate CombineImpl(Delegate d)
    {
        return new MulticastDelegate(this, d);
    }
    protected virtual Delegate RemoveImpl(Delegate d)
    {
        return this;
    }

    internal static object? Invoke(RuntimeMethodInfo* runtimeMethodInfo, object? instance, params object?[]? parameters)
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

        object? result = __Invoke((nint)runtimeMethodInfo, instance, parameters);

        return runtimeMethodInfo->ReturnType != null ? result : null;
    }

    public static Delegate Combine(params Delegate[] delegates)
    {
        int length = delegates.Length;

        if (length == 0)
        {
            throw new ArgumentException("Empty delegates array");
        }

        var firstDelegate = delegates[0];

        for (int i = 1; i < length; i++)
        {
            var currentDelegate = delegates[i];

            if (currentDelegate.MethodInfo != firstDelegate.MethodInfo)
            {
                throw new ArgumentException("Provided delegates are not instances of the same delegate type");
            }

            firstDelegate = firstDelegate.CombineImpl(delegates[i]);
        }

        return firstDelegate;
    }
    public static Delegate Combine(Delegate? a, Delegate? b)
    {
        if (a != null && b != null)
        {
            if (a.MethodInfo != b.MethodInfo)
            {
                throw new ArgumentException("Provided delegates are not instances of the same delegate type");
            }

            return a.CombineImpl(b);
        }
        else if (a != null && b == null)
        {
            return a;
        }
        else if (a == null && b != null)
        {
            return b;
        }

        throw new ArgumentNullException("Both delegates arguments are null");
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