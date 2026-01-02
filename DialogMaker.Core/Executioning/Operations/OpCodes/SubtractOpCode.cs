using System;

namespace DialogMaker.Core.Executioning
{
    public class SubtractOpCode() : TwoVariablesOpCode(DialogByteCode.Subtract)
    {
        #region Управление

        protected override void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2)
        {
            value1.Subtract(value2);    
            context.Resources.SetVariable(args[0], value1);
        }

        #endregion

        #region Статика

        public static readonly SubtractOpCode Instance = new();
		
		#endregion
    }
}