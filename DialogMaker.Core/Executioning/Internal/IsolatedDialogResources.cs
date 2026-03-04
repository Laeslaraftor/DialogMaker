using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning.Internal
{
    internal class IsolatedDialogResources(DialogByteCodeData data, IDialogExecutionResources resources)
        : InternalDialogResources(data, resources)
    {
        #region Управление

        public override void SetValue(int index, OperandValue value)
        {
            SetVariable(index, value, true);
        }
        public override void SetValue(int index, IResourceItem resource)
        {
            SetVariable(index, resource, true);
        }
        public void PushGlobalVariables()
        {
            foreach (var globalVariable in UsedGlobalVariables)
            {
                if (Items.TryGetValue(globalVariable, out var value) &&
                    value is IVariable variable)
                {
                    Resources.SetValue(globalVariable, variable.Value);
                }
            }
        }

        #endregion
    }
}
