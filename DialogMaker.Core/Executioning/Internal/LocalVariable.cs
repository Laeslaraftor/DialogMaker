using DialogMaker.Core.Common;
using System;

namespace DialogMaker.Core.Executioning.Internal
{
    internal struct LocalVariable(OperandValue value) : IVariable
    {
        public LocalVariable(string id, OperandValue value)
            : this(value)
        {
            Id = id;
        }

        public readonly DialogResourceType ResourceType => DialogResourceType.Variable;
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsReadOnly { get; }
        public readonly bool IsSeparated => true;
        public OperandValue Value { get; set; } = value;

        #region Управление

        public ResourcePath GetPath()
        {
            throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
        }
        public readonly DialogItemReference CreateReference()
        {
            return DialogItemReference.Create(this);
        }
        public readonly IVariable ToVariable() => this;

        public readonly override bool Equals(object obj)
        {
            return obj is IVariable other &&
                   Id == other.Id &&
                   IsReadOnly == other.IsReadOnly &&
                   Value == other.Value;
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Id, IsReadOnly, Value);
        }
        public readonly override string ToString()
        {
            return Value.ToString();
        }

        #endregion
    }
}
