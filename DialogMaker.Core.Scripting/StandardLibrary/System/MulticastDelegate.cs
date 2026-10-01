using System.Collections.Generic;
using System.Reflection;

namespace System;

public class MulticastDelegate : Delegate
{
    public bool HasSingleTarget => _invocationList.Count == 0;
    public override MethodInfo? MethodInfo => _invocationList.Count > 0 ? _invocationList[0].MethodInfo : null;
    public override object? Target => _invocationList.Count > 0 ? _invocationList[0].Target : null;

    private IEnumerator<Delegate> InvocationEnumerator
    {
        get
        {
            field ??= _invocationList.GetEnumerator();
            return field;
        }
    }
    private readonly HashSet<Delegate> _invocationList = new();

    public void Clear() => _invocationList.Clear();
    public bool Add(Delegate @delegate)
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return _invocationList.Add(@delegate);
    }
    public bool Remove(Delegate @delegate)
    {
        if (@delegate == null)
        {
            throw new ArgumentNullException(nameof(@delegate));
        }

        return _invocationList.Remove(@delegate);
    }
    public override object? DynamicInvoke(params object?[]? parameters)
    {
        for (int i = 0; i < _invocationList.Count; i++)
        {
            var site = _invocationList[i];
            site.DynamicInvoke(parameters);
        }

        return null;
    }

    public static MulticastDelegate operator +(MulticastDelegate multicastDelegate, Delegate @delegate)
    {
        multicastDelegate.Add(@delegate);
        return multicastDelegate;
    }
    public static MulticastDelegate operator -(MulticastDelegate multicastDelegate, Delegate @delegate)
    {
        multicastDelegate.Remove(@delegate);
        return multicastDelegate;
    }
}