namespace DialogMaker.Core
{
    /// <summary>
    /// Атрибут, задающий допустимые типы значений
    /// </summary>
    /// <param name="allowedTypes">Допустимые типы значений</param>
    public sealed class AllowedTypesAttribute(AllowedObjectValues allowedTypes) : Attribute
    {
        /// <summary>
        /// Допустимые типы значений
        /// </summary>
        public AllowedObjectValues AllowedTypes { get; } = allowedTypes;
    }
}
