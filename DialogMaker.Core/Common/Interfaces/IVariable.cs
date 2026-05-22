namespace DialogMaker.Core.Common
{
    /// <summary>
    /// Интерфейс ресурса переменной
    /// </summary>
    public interface IVariable : IResourceItem
    {
        /// <summary>
        /// Является ли переменной доступной только для чтения
        /// </summary>
        public bool IsReadOnly { get; }
        /// <summary>
        /// Значение переменной
        /// </summary>
        public OperandValue Value { get; set; }
    }
}
