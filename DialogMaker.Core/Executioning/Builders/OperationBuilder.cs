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

        public override string ToString()
        {
            if (Arguments.Length == 0)
            {
                return Code.ToString();
            }

            string args = string.Empty;

            foreach (var arg in Arguments)
            {
                if (args != string.Empty)
                {
                    args += ", ";
                }

                args += arg.ToString();
            }

            return $"{Code}({args})";
        }

        #endregion
    }
}
