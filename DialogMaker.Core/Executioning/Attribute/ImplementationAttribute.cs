using System;

namespace DialogMaker.Core.Executioning
{
    public sealed class ImplementationAttribute(Type implementation) : Attribute
    {
        public Type Implementation { get; } = implementation;

        #region Управление

        public OpCode GetInstance()
        {
            if (Implementation == null)
            {
                throw new InvalidProgramException("Тип реализации опкода не указан!");
            }

            var property = Implementation.GetProperty("Instance");

            if (property != null && property.GetValue(null) is OpCode result)
            {
                return result;
            }

            return (OpCode)Activator.CreateInstance(Implementation);
        }

        #endregion
    }
}
