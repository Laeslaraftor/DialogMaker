namespace DialogMaker.Core.Attributes
{
    /// <summary>
    /// Атрибут, задающий тип ресурса
    /// </summary>
    /// <param name="type">Тип ресурса</param>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ResourceTypeAttribute(Type type) : Attribute
    {
        /// <summary>
        /// Тип ресурса
        /// </summary>
        public Type Type { get; } = type;
        /// <summary>
        /// Является ли тип предназначенным для разработки
        /// </summary>
        public bool IsDev { get; set; }
    }
}
