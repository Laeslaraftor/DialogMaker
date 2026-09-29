using System.Reflection;
using System.Native;

namespace Internal.System.Runtime;

internal struct RuntimeMethodInfo
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
    public Pointer<RuntimeTypeInfo> DeclaringType;
    public Pointer<RuntimeTypeInfo> ReturnType;
    public NativeArray<RuntimeMethodParameterInfo> Parameters;
}