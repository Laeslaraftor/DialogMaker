using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class GotoIfTrueOpCode() : OpCode(DialogByteCode.GotoIfTrue)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var lastExpression = context.CurrentThread.Pop();
            
            if (lastExpression == true)
            {
                context.CurrentThread.Goto(args[0]);
            }
        }

        #endregion

        #region Статика

        public static readonly GotoIfTrueOpCode Instance = new();

        #endregion
    }
}
