namespace DialogMaker.Core.Executioning
{
    public class DivideOpCode() : TwoVariablesOpCode(DialogByteCode.Divide)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            value1.Divide(value2);
            context.Resources.SetVariable(args[0], value1);
        }

        #endregion

        #region Статика

        public static readonly DivideOpCode Instance = new();
		
		#endregion
    }
}