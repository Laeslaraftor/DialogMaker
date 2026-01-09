namespace DialogMaker.Core.Executioning
{
    public class MultiplyOpCode() : TwoVariablesOpCode(DialogByteCode.Multiply)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            value1.Multiply(value2);
            context.Resources.SetVariable(args[0], value1);
        }

        #endregion

        #region Статика

        public static readonly MultiplyOpCode Instance = new();
		
		#endregion
    }
}