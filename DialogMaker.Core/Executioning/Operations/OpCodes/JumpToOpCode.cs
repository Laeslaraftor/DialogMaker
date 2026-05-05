namespace DialogMaker.Core.Executioning
{
    public class JumpToOpCode() : OpCode(DialogByteCode.JumpTo)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);
            context.CurrentThread.JumpTo(args[0]);
            context.CurrentThread.Goto(args[1]);
        }

        #endregion

        #region Статика

        public static readonly JumpToOpCode Instance = new();

        #endregion
    }
}
