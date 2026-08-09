namespace System.Reflection;

public enum MetadataTokenType
{
    None = 0,
    TypeDefinition = 0x02000000,
    Field = 0x04000000,
    Method = 0x06000000,
    Property = 0x17000000,
    Operator = 0x2A000000,
}