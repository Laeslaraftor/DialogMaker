namespace DialogMaker.Core.Executioning
{
    public class StackToVariableOpCode() : OpCode(DialogByteCode.StackToVariable)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var value = context.CurrentThread.Pop();
            context.Resources.SetValue(args[0], value);
        }

        #endregion

        #region Статика

        public static readonly StackToVariableOpCode Instance = new();

        #endregion
    }
}
