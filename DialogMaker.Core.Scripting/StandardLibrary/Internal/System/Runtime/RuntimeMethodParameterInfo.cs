using System.Native;

namespace Internal.System.Runtime;

internal struct RuntimeMethodParameterInfo
{
    public NativeArray<char> Name;
    public Pointer<RuntimeTypeInfo> Type;
    public DSharpMethodParameterMode Mode;
}