using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class StartThread2OpCode() : OpCode(DialogByteCode.StartThread2)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);
            context.CurrentThread.StartThread(new(args[0], args[1]));
        }

        #endregion

        #region Статика

        public static readonly StartThread2OpCode Instance = new();

        #endregion
    }
}
