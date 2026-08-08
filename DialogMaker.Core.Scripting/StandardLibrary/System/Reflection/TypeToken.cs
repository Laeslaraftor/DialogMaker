using System.Diagnostics.CodeAnalysis;

namespace System.Reflection;

public struct TypeToken : IEquatable<TypeToken>
{
    public int Value => _value;
    public int AssemblyIndex => _assemblyIndex;
    //public int Index => _value & 0x00FFFFFF;
    //public DSharpMetadataTokenType Type => (DSharpMetadataTokenType)(_value & 0xFF000000);

    private readonly int _value;
    private readonly int _assemblyIndex;

    public override bool Equals(object? obj)
    {
        return Equals(obj as TypeToken);
    }
    public bool Equals(TypeToken other)
    {
        var otherValue = other._value;
        var otherIndex = other._assemblyIndex;

        return _value == otherValue && _assemblyIndex == otherIndex;
    }

    public static bool operator ==(TypeToken l, TypeToken r) => l.Equals(r);
    public static bool operator !=(TypeToken l, TypeToken r) => !l.Equals(r);
}