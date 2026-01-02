using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutionResources
    {
        public IResourceItem GetResource(int index);
        public OperandValue GetVariable(int index);
        public void SetVariable(int index, OperandValue value);
    }
}
