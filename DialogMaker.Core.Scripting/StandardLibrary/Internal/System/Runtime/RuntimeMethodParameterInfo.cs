using System.Native;

namespace Internal.System.Runtime;

internal unsafe struct RuntimeMethodParameterInfo
{
    public NativeArray<char> Name;
    public RuntimeTypeInfo* Type;
    public DSharpMethodParameterMode Mode;
}