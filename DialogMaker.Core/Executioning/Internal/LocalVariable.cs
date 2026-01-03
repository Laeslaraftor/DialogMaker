using DialogMaker.Core.Common;
using System;

namespace DialogMaker.Core.Executioning.Internal
{
    internal struct LocalVariable : IVariable
    {
        public LocalVariable(OperandValue value)
        {
            Value = value;
        }

        public bool IsReadOnly { get; }
        public OperandValue Value { get; set; }

        #region Управление

        public readonly override bool Equals(object obj)
        {
            return obj is IVariable other &&
                   IsReadOnly == other.IsReadOnly &&
                   Value == other.Value;
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(IsReadOnly, Value);
        }
        public readonly override string ToString()
        {
            return Value.ToString();
        }

        #endregion
    }
}
