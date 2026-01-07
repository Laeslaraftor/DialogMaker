namespace DialogMaker.Core.Common
{
    public interface IVariable : IResourceItem
    {
        public bool IsReadOnly { get; }
        public OperandValue Value { get; set; }
    }
}
