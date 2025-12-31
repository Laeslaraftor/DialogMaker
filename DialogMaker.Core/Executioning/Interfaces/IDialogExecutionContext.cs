using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutionContext
    {
        public IResourceItem GetResource(int index);
        public string GetString(int index);
        public OperandValue GetVariable(int index);
        public void SetVariable(int index, OperandValue value);
    }
}
