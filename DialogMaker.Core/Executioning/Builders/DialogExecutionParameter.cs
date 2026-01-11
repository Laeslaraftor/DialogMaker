using DialogMaker.Core.Common;
using DialogMaker.Core.Editor.Nodes;
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
        public DialogExecutionParameter(INode node)
        {
            Value = node;
        }
        public DialogExecutionParameter(int value, bool isRawNumber)
        {
            if (isRawNumber)
            {
                Value = value;
            }
            else
            {
                Value = new OperandValue(value);
            }
        }

        public Guid Id { get; } = Guid.NewGuid();
        public object? Value { get; }

        #region Управление

        public readonly int AddToContext(CodeCompileContext context)
        {
            var contextBuilder = context.ContextBuilder;

            if (Value == null)
            {
                return -1;
            }
            if (Value is int number)
            {
                return number;
            }
            else if (Value is OperandValue value)
            {
                return contextBuilder.AddVariable(new LocalVariable(Id.ToString(), value));
            }
            if (Value is IVariable variable)
            {
                return contextBuilder.AddVariable(variable);
            }
            else if (Value is IResourceItem item)
            {
                return contextBuilder.AddResource(item);
            }
            else if (Value is INode node)
            {
                if (context.NodesInfo.TryGetValue(node, out var nodeInfo))
                {
                    return nodeInfo.Index;
                }

                throw new ArgumentException($"Контекст компиляции не содержит информации об узле {node}", nameof(context));
            }
            else if (Value is OperationBuilder operation)
            {
                return operation.Index;
                //return contextBuilder.AddVariable(new LocalVariable(operationIndex));
            }
            else if (Value is DialogSectionBuilder section)
            {
                int sectionIndex = section.Index;

                if (sectionIndex == -1)
                {
                    throw new InvalidOperationException($"Недопустимый индекс сегмента кода ({sectionIndex}). Возможно, этот сегмент был удалён");
                }

                return sectionIndex;
                //return contextBuilder.AddVariable(new LocalVariable(sectionIndex));
            }

            throw new InvalidOperationException($"Невозможно добавить значение в контекст, так как оно либо пустое, либо имеет недопустимый тип. Значение: {Value}");
        }

        public bool Equals(DialogExecutionParameter other)
        {
            return Value?.Equals(other.Value) == true;
        }
        public override bool Equals(object obj)
        {
            return obj is DialogExecutionParameter other &&
                   Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value);
        }
        public override string ToString()
        {
            if (Value is OperationBuilder operation)
            {
                return $"i:{operation.Index}";
            }
            if (Value is OperandValue operand && 
                operand.Type == DialogVariableType.String)
            {
                return $"\"{operand}\"";
            }

            return Value?.ToString() ?? string.Empty;
        }

        #endregion

        #region Операторы

        public static bool operator ==(DialogExecutionParameter p1, DialogExecutionParameter p2) => p1.Equals(p2);
        public static bool operator !=(DialogExecutionParameter p1, DialogExecutionParameter p2) => !p1.Equals(p2);

        #endregion

        #region Статика

        public static readonly DialogExecutionParameter Empty = new(new OperandValue(0));

        #endregion
    }
}
