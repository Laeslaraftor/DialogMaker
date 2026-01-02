using DialogMaker.Core.Common;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowFullScreenReplicaOpCode() : OpCode(DialogByteCode.ShowFullScreenReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = context.Resources.GetResource(args[0]) as ICharacter;
            var replica = context.Resources.GetVariable(args[1]).ToString();
            var background = context.Resources.GetResource(args[2]);

            await context.Handler.ShowFullscreenReplica(character, background, replica);
        }

        #endregion
		
		#region Статика
		
		public static readonly ShowFullScreenReplicaOpCode Instance = new();
		
		#endregion
    }
}