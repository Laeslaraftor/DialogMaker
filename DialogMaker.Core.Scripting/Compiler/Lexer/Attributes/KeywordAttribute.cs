namespace DialogMaker.Core.Scripting.Compiler.Lexer
{
    internal sealed class KeywordAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}
