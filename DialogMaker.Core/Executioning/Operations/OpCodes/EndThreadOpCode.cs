using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class EndThreadOpCode() : OpCode(DialogByteCode.EndThread)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            context.CurrentThread.StopThread();
        }

        #endregion
		
		#region Статика
		
		public static readonly EndThreadOpCode Instance = new();
		
		#endregion
    }
}