using System.Native;
using System.Reflection;
using Internal.System.Runtime;

namespace System;

public abstract unsafe class Delegate
{
    public Delegate(object? target, MethodInfo methodInfo)
        : this(1)
    {
        InvocationList[0] = new()
        {
            Target = target,
            RuntimeMethodInfo = (nint)methodInfo.RuntimeMethodInfo,
            MethodInfo = methodInfo,
            Delegate = this
        };
    }
    public Delegate(MethodInfo methodInfo) 
        : this(1)
    {
        InvocationList[0] = new()
        {
            RuntimeMethodInfo = (nint)methodInfo.RuntimeMethodInfo,
            MethodInfo = methodInfo,
            Delegate = this
        };
    }
    public Delegate(object? target, nint runtimeMethodInfo)
        : this(1)
    {
        InvocationList[0] = new()
        {
            Target = target,
            RuntimeMethodInfo = runtimeMethodInfo,
            Delegate = this
        };
    }
    public Delegate(nint runtimeMethodInfo)
        : this(1)
    {
        InvocationList[0] = new()
        {
            RuntimeMethodInfo = runtimeMethodInfo,
            Delegate = this
        };
    }
    protected Delegate(int capacity)
    {
        InvocationList = new InvocationInfo[capacity];
    }

    public MethodInfo MethodInfo
    {
        get
        {
            MethodInfo? methodInfo;
            var invocationList = InvocationList;
            int lastIndex = invocationList.Length - 1;
            
            lock (invocationList)
            {
                var invocationInfo = invocationList[lastIndex];
                methodInfo = invocationInfo.MethodInfo;

                if (methodInfo == null)
                {
                    methodInfo = new((RuntimeMethodInfo*)invocationInfo.RuntimeMethodInfo);
                    invocationInfo.MethodInfo = methodInfo;
                    invocationList[lastIndex] = invocationInfo;
                }
            }

            return methodInfo;
        }
    }
    public object? Target
    {
        get
        {
            var invocationList = InvocationList;
            return invocationList[invocationList.Length - 1].Target;
        }
    }

    private nint RuntimeMethodInfo 
    {
        get
        {
            var invocationList = InvocationList;
            return invocationList[invocationList.Length - 1].RuntimeMethodInfo;
        }
    }
    protected readonly InvocationInfo[] InvocationList;

    public object? DynamicInvoke(params object?[]? parameters)
    {
        var invocationList = InvocationList;
        var invocationListLength = invocationList.Length;
        object? result = null;

        for (int i = 0; i < invocationListLength; i++)
        {
            var info = invocationList[i];
            Invoke((RuntimeMethodInfo*)info.RuntimeMethodInfo, info.Target, parameters);
        }

        return result;
    }
    public Delegate[] GetInvocationList()
    {
        var invocationList = InvocationList;
        var length = invocationList.Length;
        
        if (length == 1)
        {
            return new Delegate[] { this };
        }

        var result = new Delegate[length];

        lock (invocationList)
        {
            for (int i = 0; i < length; i++)
            {
                var invocationInfo = invocationList[i];
                var @delegate = invocationInfo.Delegate;

                if (@delegate == null)
                {
                    @delegate = CreateNew(invocationInfo.Target, invocationInfo.RuntimeMethodInfo);
                    invocationInfo.Delegate = @delegate;
                    invocationList[i] = invocationInfo;
                }

                result[i] = @delegate;
            }
        }

        return result;
    }

    protected abstract Delegate CreateNew(int capacity);
    private Delegate CreateNew(object? target, nint runtimeMethodInfo)
    {
        var newDelegate = CreateNew(1);
        newDelegate.InvocationList[0] = new()
        {
            RuntimeMethodInfo = runtimeMethodInfo,
            Target = target,
            Delegate = newDelegate
        };

        return newDelegate;
    }

    private Delegate CombineImpl(Delegate other)
    {
        var currentInvocationList = InvocationList;
        var currentInvocationListLength = currentInvocationList.Length;
        var otherInvocationList = other.InvocationList;
        var otherInvocationListLength = otherInvocationList.Length;
        var newCapacity = currentInvocationListLength + otherInvocationListLength;
        var newDelegate = CreateNew(newCapacity);
        var newDelegateInvocationList = newDelegate.InvocationList;

        Array<InvocationInfo>.Copy(currentInvocationList, 0, newDelegateInvocationList, 0, currentInvocationListLength);
        Array<InvocationInfo>.Copy(otherInvocationList, 0, newDelegateInvocationList, currentInvocationListLength, otherInvocationListLength);

        return newDelegate;
    }
    private Delegate? RemoveImpl(Delegate other)
    {
        if (this == other)
        {
            return null;
        }

        var currentInvocationList = InvocationList;
        var currentInvocationListLength = currentInvocationList.Length;
        var otherInvocationList = other.InvocationList;
        var otherInvocationListLength = otherInvocationList.Length;
        Span<InvocationInfo> tempOtherInvocationList = Array.CanStackalloc<InvocationInfo>(otherInvocationListLength) ?
                                                       stackalloc InvocationInfo[otherInvocationListLength] :
                                                       new InvocationInfo[otherInvocationListLength];
        Span<InvocationInfo> tempInvocationList = Array.CanStackalloc<InvocationInfo>(currentInvocationListLength) ?
                                                  stackalloc InvocationInfo[currentInvocationListLength] :
                                                  new InvocationInfo[currentInvocationListLength];
        int currentIndex = 0;
        int availableOtherInvocations = otherInvocationListLength;

        for (int i = 0; i < otherInvocationListLength; i++)
        {
            tempOtherInvocationList[i] = otherInvocationList[i];
        }

        for (int i = currentInvocationListLength - 1; i >= 0; i--)
        {
            var currentInfo = currentInvocationList[i];
            bool skipDelegate = false;

            for (int o = 0; o < otherInvocationListLength; o++)
            {
                var otherInfo = otherInvocationList[o];

                if (otherInfo.RuntimeMethodInfo == 0)
                {
                    continue;
                }
                if (currentInfo.Target == otherInfo.Target && 
                    currentInfo.RuntimeMethodInfo == otherInfo.RuntimeMethodInfo)
                {
                    otherInvocationList[o] = null;
                    skipDelegate = true;
                    availableOtherInvocations--;
                    break;
                }
            }

            if (skipDelegate)
            {
                continue;
            }

            tempInvocationList[currentIndex] = currentInfo;
            currentIndex++;

            if (0 >= availableOtherInvocations)
            {
                break;
            }
        }

        if (currentIndex == currentInvocationListLength)
        {
            return this;
        }
        else if (currentIndex == 0)
        {
            return null;
        }

        var newDelegate = CreateNew(currentIndex);
        var newDelegateInvocationList = newDelegate.InvocationList;

        for (int i = 0; i < currentIndex; i++)
        {
            newDelegateInvocationList[i] = tempInvocationList[i];
        }

        return newDelegate;
    }
    private Delegate? RemoveAllImpl(Delegate other)
    {
        if (this == other)
        {
            return null;
        }

        var currentInvocationList = InvocationList;
        var currentInvocationListLength = currentInvocationList.Length;
        var otherInvocationList = other.InvocationList;
        var otherInvocationListLength = otherInvocationList.Length;
        Span<InvocationInfo> tempInvocationList = Array.CanStackalloc<InvocationInfo>(currentInvocationListLength) ?
                                                  stackalloc InvocationInfo[currentInvocationListLength] :
                                                  new InvocationInfo[currentInvocationListLength];
        int currentIndex = 0;

        for (int i = 0; i < currentInvocationListLength; i++)
        {
            var currentInfo = currentInvocationList[i];
            bool skipDelegate = false;

            for (int o = 0; o < otherInvocationListLength; o++)
            {
                var otherInfo = otherInvocationList[o];

                if (currentInfo.Target == otherInfo.Target && 
                    currentInfo.RuntimeMethodInfo == otherInfo.RuntimeMethodInfo)
                {
                    skipDelegate = true;
                    break;
                }
            }

            if (skipDelegate)
            {
                continue;
            }

            tempInvocationList[currentIndex] = currentInfo;
            currentIndex++;
        }

        if (currentIndex == currentInvocationListLength)
        {
            return this;
        }
        else if (currentIndex == 0)
        {
            return null;
        }

        var newDelegate = CreateNew(currentIndex);
        var newDelegateInvocationList = newDelegate.InvocationList;

        for (int i = 0; i < currentIndex; i++)
        {
            newDelegateInvocationList[i] = tempInvocationList[i];
        }

        return newDelegate;
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

            firstDelegate = firstDelegate.CombineImpl(currentDelegate);
        }

        return firstDelegate;
    }
    public static Delegate Combine(Delegate? a, Delegate? b)
    {
        if (a != null && b != null)
        {
            if (a.RuntimeMethodInfo != b.RuntimeMethodInfo)
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
    public static Delegate? Remove(Delegate a, Delegate? b)
    {
        ArgumentNullException.ThrowIfNull(a, nameof(a));

        if (b != null)
        {
            if (a.RuntimeMethodInfo != b.RuntimeMethodInfo)
            {
                throw new ArgumentException("Provided delegates are not instances of the same delegate type");
            }

            return a.RemoveImpl(b);
        }

        return a;
    }
    public static Delegate? RemoveAll(Delegate a, Delegate? b)
    {
        ArgumentNullException.ThrowIfNull(a, nameof(a));

        if (b != null)
        {
            if (a.RuntimeMethodInfo != b.RuntimeMethodInfo)
            {
                throw new ArgumentException("Provided delegates are not instances of the same delegate type");
            }

            return a.RemoveAllImpl(b);
        }
        
        return a;
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

    protected struct InvocationInfo
    {
        public object? Target;
        public nint RuntimeMethodInfo;
        public MethodInfo MethodInfo;
        public Delegate? Delegate;
    }
}