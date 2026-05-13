namespace DialogMaker.Core.Executioning
{
    public sealed class ImplementationAttribute(Type implementation) : Attribute
    {
        public Type Implementation { get; } = implementation;

        #region Управление

        public T GetInstance<T>()
        {
            if (Implementation == null)
            {
                throw new InvalidProgramException("Тип реализации опкода не указан!");
            }

            var property = Implementation.GetProperty("Instance");

            if (property != null && property.GetValue(null) is T result)
            {
                return result;
            }

            return (T)Activator.CreateInstance(Implementation);
        }

        #endregion
    }
}
