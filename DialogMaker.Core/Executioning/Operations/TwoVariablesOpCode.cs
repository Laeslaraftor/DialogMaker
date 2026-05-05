namespace DialogMaker.Core.Executioning
{
    public abstract class TwoVariablesOpCode(DialogByteCode code) : OpCode(code)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);

            var value1 = context.Resources.GetVariable(args[0]);
            var value2 = context.Resources.GetVariable(args[1]);

            Execute(context, args, value1, value2);
        }

        protected abstract void Execute(DialogExecutionContext context, int[] args, OperandValue value1, OperandValue value2);

        #endregion
    }
}
