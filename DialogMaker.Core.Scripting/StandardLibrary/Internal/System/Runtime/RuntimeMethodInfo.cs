using System.Reflection;
using System.Native;

namespace Internal.System.Runtime;

internal unsafe struct RuntimeMethodInfo
{
    public MetadataToken MetadataToken;
    public NativeArray<char> Name;
    public TypeAccess Access;
    public MethodType Type;
    public bool IsAbstract;
    public bool IsVirtual;
    public bool IsStatic;
    public bool IsExtern;
    public bool IsSealed;
    public RuntimeTypeInfo* DeclaringType;
    public RuntimeTypeInfo* ReturnType;
    public NativeArray<RuntimeMethodParameterInfo> Parameters;
}