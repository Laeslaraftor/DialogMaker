namespace DialogMaker.Core.Executioning.Internal
{
    internal class IsolatedDialogResources(DialogByteCodeData data, IDialogExecutionResources resources)
        : InternalDialogResources(data, resources)
    {
        #region Управление

        public override void SetVariable(int index, OperandValue value)
        {
            SetVariable(index, value, true);
        }
        public void PushGlobalVariables()
        {
            foreach (var globalVariable in UsedGlobalVariables)
            {
                if (Variables.TryGetValue(globalVariable, out var value))
                {
                    Resources.SetVariable(globalVariable, value);
                }
            }
        }

        public void Clear()
        {
            foreach (var globalVariable in UsedGlobalVariables)
            {
                var value = Resources.GetVariable(globalVariable);

                if (!Variables.TryAdd(globalVariable, value))
                {
                    Variables[globalVariable] = value;
                }
            }
        }

        #endregion
    }
}
