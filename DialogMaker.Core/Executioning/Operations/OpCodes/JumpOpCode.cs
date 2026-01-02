using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class JumpOpCode() : OpCode(DialogByteCode.Jump)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);
            context.CurrentThread.JumpTo(args[0]);
        }

        #endregion
		
		#region Статика
		
		public static readonly JumpOpCode Instance = new();
		
		#endregion
    }
}