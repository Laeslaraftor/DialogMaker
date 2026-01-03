namespace DialogMaker.Core.Executioning.Builders
{
    public class OperationBuilder
    {
        public OperationBuilder(DialogSectionBuilder builder, DialogByteCode code)
        {
            SectionBuilder = builder;
            Code = code;
            Arguments = Operation.CreateArguments<DialogExecutionParameter>(code);
        }

        public DialogSectionBuilder SectionBuilder { get; }
        public DialogByteCode Code { get; }
        public DialogExecutionParameter[] Arguments { get; }

        #region Управление

        public int GetCodeIndex()
        {
            return SectionBuilder.GetOperationByteCodeIndex(this);
        }

        #endregion
    }
}
