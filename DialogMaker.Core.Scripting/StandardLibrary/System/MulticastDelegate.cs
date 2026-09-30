using System.Collections.Generic;
using System.Reflection;

namespace System;

public class MulticastDelegate : Delegate
{
    public bool HasSingleTarget => _invocationList.Count == 0;
    public override MethodInfo? MethodInfo => _invocationList.Count > 0 ? _invocationList[0].MethodInfo : null;
    public override object? Target => _invocationList.Count > 0 ? _invocationList[0].Target : null;

    private readonly List<Delegate> _invocationList = new();

    public void Clear() => _invocationList.Clear();
    public override object? DynamicInvoke(params object?[]? parameters)
    {
        for (int i = 0; i < _invocationList.Count; i++)
        {
            var site = _invocationList[i];
            site.DynamicInvoke(parameters);
        }

        return null;
    }
}