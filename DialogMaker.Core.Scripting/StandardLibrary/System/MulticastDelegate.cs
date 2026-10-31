using System.Collections.Generic;
using System.Reflection;

namespace System;

public sealed class MulticastDelegate : Delegate
{
    public MulticastDelegate(params Delegate[] delegates)
    {
        if (delegates.Length == 0)
        {
            throw new ArgumentException("Empty delegates array", nameof(delegates));
        }

        _delegates = new(delegates);
    }

    public override MethodInfo MethodInfo => _delegates[_delegates.Count - 1].MethodInfo;
    public override object? Target => _delegates[_delegates.Count - 1].Target;

    private readonly List<Delegate> _delegates;

    public override Delegate[] GetInvocationList()
    {
        return _delegates.ToArray();
    }

    protected override nint GetRuntimeMethodInfo()
    {
        return _delegates[_delegates.Count - 1].GetRuntimeMethodInfo();
    }
    protected override Delegate CombineImpl(Delegate d)
    {
        _delegates.Add(d);
        return this;
    }
    protected override Delegate RemoveImpl(Delegate d)
    {
        _delegates.Remove(d);

        if (_delegates.Count == 1)
        {
            return _delegates[0];
        }

        return this;
    }
}