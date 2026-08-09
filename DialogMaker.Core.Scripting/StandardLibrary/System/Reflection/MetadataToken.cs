namespace System.Reflection;

public struct MetadataToken : IEquatable<MetadataToken>
{
    public int Value => _value;
    public int AssemblyIndex => _assemblyIndex;
    public int Index => _value & 0x00FFFFFF;
    public MetadataTokenType Type => (MetadataTokenType)(_value & 0xFF000000);

    private readonly int _value;
    private readonly int _assemblyIndex;

    public override bool Equals(object? obj)
    {
        return Equals(obj as MetadataToken);
    }
    public bool Equals(MetadataToken other)
    {
        return _value == other._value && _assemblyIndex == other._assemblyIndex;
    }

    public static bool operator ==(MetadataToken l, MetadataToken r) => l.Equals(r);
    public static bool operator !=(MetadataToken l, MetadataToken r) => !l.Equals(r);
}