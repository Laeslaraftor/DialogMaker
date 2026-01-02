namespace DialogMaker.Core.Executioning
{
    public class EqualsOpCode() : TwoVariablesOpCode(DialogByteCode.Equals)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            context.CurrentThread.Push(value1 == value2);
        }

        #endregion

        #region Статика

        public static readonly EqualsOpCode Instance = new();
		
		#endregion
    }
}