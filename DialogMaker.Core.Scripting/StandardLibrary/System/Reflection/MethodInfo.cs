using System.Native;
using Internal.System.Runtime;

namespace System.Reflection;

public unsafe class MethodInfo : MemberInfo
{
    internal MethodInfo(Pointer<RuntimeMethodInfo> runtimeMethodInfo)
    {
        RuntimeMethodInfo = runtimeMethodInfo;
        _methodInfo = runtimeMethodInfo[0];
    }

    internal Pointer<RuntimeMethodInfo> RuntimeMethodInfo { get; }
    public override string Name
    {
        get
        {
            field ??= new(_methodInfo.Name.ToSpan());
            return field;
        }
    }
    public override Type? DeclaringType
    {
        get
        {
            if (field == null && _methodInfo.DeclaringType != null)
            {
                field = new(_methodInfo.DeclaringType[0]);
            }

            return field;
        }
    }
    public Type? ReturnType
    {
        get
        {
            if (field == null && _methodInfo.ReturnType != null)
            {
                field = new(_methodInfo.ReturnType[0]);
            }

            return field;
        }
    }

    private readonly RuntimeMethodInfo _methodInfo;

    public object? Invoke(object? instance, object?[]? parameters)
    {
        return Delegate.Invoke(RuntimeMethodInfo, instance, parameters);
    }
}