namespace System.Reflection;

public struct MetadataToken
{
    public int Value => _value;
    public int AssemblyIndex => _assemblyIndex;
    //public int Index => _value & 0x00FFFFFF;
    //public DSharpMetadataTokenType Type => (DSharpMetadataTokenType)(_value & 0xFF000000);

    private readonly int _value;
    private readonly int _assemblyIndex;
}