namespace DialogMaker.Core.Scripting.Tokens
{
    /// <summary>
    /// Атрибут, указывающий парсер
    /// </summary>
    /// <param name="parserType"></param>
    public sealed class ParserAttribute(Type parserType) : Attribute
    {
        /// <summary>
        /// Тип парсера
        /// </summary>
        public Type Type { get; } = parserType;
    }
}
