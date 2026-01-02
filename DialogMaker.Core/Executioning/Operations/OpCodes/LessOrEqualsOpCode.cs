namespace DialogMaker.Core.Executioning
{
    public class LessOrEqualsOpCode() : TwoVariablesOpCode(DialogByteCode.LessOrEquals)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            context.CurrentThread.Push(value1 <= value2);
        }

        #endregion

        #region Статика

        public static readonly LessOrEqualsOpCode Instance = new();
		
		#endregion
    }
}