using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class GotoOpCode() : OpCode(DialogByteCode.Goto)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            context.CurrentThread.Goto(args[0]);
        }

        #endregion

        #region Статика

        public static readonly GotoOpCode Instance = new();

        #endregion
    }
}
