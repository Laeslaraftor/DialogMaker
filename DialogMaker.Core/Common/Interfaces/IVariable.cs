namespace DialogMaker.Core.Common
{
    public interface IVariable
    {
        public bool IsReadOnly { get; }
        public OperandValue Value { get; set; }
    }
}
