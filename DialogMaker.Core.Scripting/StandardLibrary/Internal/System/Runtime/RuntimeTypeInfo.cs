namespace Internal.System.Runtime;

using System.Native;
using System;
using System.Reflection;

public struct RuntimeTypeInfo
{
    public TypeToken MetadataToken;
    public int ObjectType;
    public int Size;
    public int BuildInValueTypeIndex;
    public nint Converter;
    public bool IsGeneric;
    public NativeArray<char> Name;
    public NativeArray<char> Namespace;
    public Pointer<RuntimeTypeInfo> BaseType;
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