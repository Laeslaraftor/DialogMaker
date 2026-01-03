using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;

namespace DialogMaker.Core.Executioning.Builders
{
    public readonly struct DialogExecutionParameter : IEquatable<DialogExecutionParameter>
    {
        public DialogExecutionParameter(OperandValue value)
        {
            Value = value;
        }
        public DialogExecutionParameter(IResourceItem item)
        {
            Value = item;
        }
        public DialogExecutionParameter(OperationBuilder operation)
        {
            Value = operation;
        }
        public DialogExecutionParameter(IVariable variable)
        {
            Value = variable; 
        }
        public DialogExecutionParameter(DialogSectionBuilder section)
        {
            Value = section;
        }

        public object Value { get; }

        #region Управление

        public readonly int AddToContext(DialogExecutionContextBuilder contextBuilder)
        {
            if (Value is OperandValue value)
            {
                return contextBuilder.AddVariable(new LocalVariable(value));
            }
            if (Value is IVariable variable)
            {
                return contextBuilder.AddVariable(variable);
            }
            else if (Value is IResourceItem item)
            {
                return contextBuilder.AddResource(item);
            }
            else if (Value is OperationBuilder operation)
            {
                int operationIndex = operation.GetCodeIndex();
                return contextBuilder.AddVariable(new LocalVariable(operationIndex));
            }
            else if (Value is DialogSectionBuilder section)
            {
                int sectionIndex = section.Index;

                if (sectionIndex == -1)
                {
                    throw new InvalidOperationException($"Недопустимый индекс сегмента кода ({sectionIndex}). Возможно, этот сегмент был удалён");
                }

                return contextBuilder.AddVariable(new LocalVariable(sectionIndex));
            }

            throw new InvalidOperationException($"Невозможно добавить значение в контекст, так как оно либо пустое, либо имеет недопустимый тип. Значение: {Value}");
        }

        public bool Equals(DialogExecutionParameter other)
        {
            return Value.Equals(other.Value);
        }
        public override bool Equals(object obj)
        {
            return obj is DialogExecutionParameter other &&
                   Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion
    }
}
