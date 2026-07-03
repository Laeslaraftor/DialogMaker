namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    /// <summary>
    /// Attribute that sets keyword value
    /// </summary>
    /// <param name="name">Keyword value</param>
    public sealed class KeywordAttribute(string name) : Attribute
    {
        /// <summary>
        /// Keyword value
        /// </summary>
        public string Name { get; } = name;
    }
}
