namespace DialogMaker.Core
{
    /// <summary>
    /// Атрибут, задающий типы
    /// </summary>
    /// <param name="types">Типы</param>
    public sealed class TypesAttribute(params Type[] types) : Attribute
    {
        /// <summary>
        /// Типы
        /// </summary>
        public Type[] Types { get; } = types;
    }
}