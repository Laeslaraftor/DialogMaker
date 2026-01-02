namespace DialogMaker.Core.Executioning
{
    public class NotEqualsOpCode() : TwoVariablesOpCode(DialogByteCode.NotEquals)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            context.CurrentThread.Push(value1 != value2);
        }

        #endregion

        #region Статика

        public static readonly NotEqualsOpCode Instance = new();
		
		#endregion
    }
}