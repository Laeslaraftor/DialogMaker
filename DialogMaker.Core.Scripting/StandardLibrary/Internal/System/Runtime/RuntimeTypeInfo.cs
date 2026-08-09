namespace Internal.System.Runtime;

using System.Native;
using System;
using System.Reflection;

internal struct RuntimeTypeInfo
{
    public MetadataToken MetadataToken;
    public ObjectType ObjectType;
    public int Size;
    public int BuildInValueTypeIndex;
    public nint Converter;
    public bool IsGeneric;
    public NativeArray<char> Name;
    public NativeArray<char> Namespace;
    public Pointer<RuntimeTypeInfo> BaseType;
    public Pointer<RuntimeTypeInfo> DeclaringType;
    public NativeArray<Pointer<RuntimeTypeInfo>> GenericParameters;
    public NativeArray<Pointer<RuntimeTypeInfo>> Intefaces;
    public NativeArray<nint> Constructors;
    public NativeArray<nint> Methods;
    public NativeArray<nint> Properties;
    public NativeArray<nint> Fields;
    public nint Finalizer;
    public nint Initializer;
    public nint StaticInitializer;
}