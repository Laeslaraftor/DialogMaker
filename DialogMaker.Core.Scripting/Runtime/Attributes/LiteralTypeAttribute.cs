namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Attributes that sets literal type
    /// </summary>
    /// <param name="literalType">Literal type to set</param>
    public sealed class LiteralTypeAttribute(DSharpLiteralType literalType) : Attribute
    {
        public DSharpLiteralType LiteralType { get; } = literalType;
    }
}
