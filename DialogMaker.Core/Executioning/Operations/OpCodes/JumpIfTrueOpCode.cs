namespace DialogMaker.Core.Executioning
{
    public class JumpIfTrueOpCode() : OpCode(DialogByteCode.JumpIfTrue)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var lastExpressionResult = context.CurrentThread.Pop();

            if (lastExpressionResult == true)
            {
                context.CurrentThread.JumpTo(args[0]);
            }
        }

        #endregion

        #region Статика

        public static readonly JumpIfTrueOpCode Instance = new();

        #endregion
    }
}