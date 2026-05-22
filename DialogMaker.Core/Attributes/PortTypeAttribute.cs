namespace DialogMaker.Core
{
    /// <summary>
    /// Атрибут, задающий тип данных порта узла
    /// </summary>
    /// <param name="type">Тип данных порта узла</param>
    public sealed class PortTypeAttribute(DialogNodePortType type) : Attribute
    {
        /// <summary>
        /// Тип данных порта узла
        /// </summary>
        public DialogNodePortType PortType { get; } = type;
    }
}
