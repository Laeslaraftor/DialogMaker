namespace Internal.System.Runtime;

using System.Native;
using System;
using System.Reflection;

public struct RuntimeTypeInfo
{
    public MetadataToken MetadataToken;
    public int ObjectType;
    public int Size;
    public int BuildInValueTypeIndex;
    public nint Converter;
    public bool IsGeneric;
    public NativeArray<char> Name;
}