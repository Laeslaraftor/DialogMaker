namespace DialogMaker.Core.Executioning
{
    public class AddOpCode() : TwoVariablesOpCode(DialogByteCode.Add)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            value1.Add(value2);
            context.Resources.SetValue(args[0], value1);
        }

        #endregion

        #region Статика

        public static readonly AddOpCode Instance = new();

        #endregion
    }
}