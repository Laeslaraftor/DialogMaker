using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class StartThreadOpCode() : OpCode(DialogByteCode.StartThread)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);
            context.CurrentThread.StartThread(new(args[0], 0));
        }

        #endregion
		
		#region Статика
		
		public static readonly StartThreadOpCode Instance = new();
		
		#endregion
    }
}