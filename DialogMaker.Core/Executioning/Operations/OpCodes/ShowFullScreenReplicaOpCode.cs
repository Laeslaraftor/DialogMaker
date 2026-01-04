using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowFullScreenReplicaOpCode() : OpCode(DialogByteCode.ShowFullScreenReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = ShowReplicaOpCode.GetCharacter(context, args[0]);
            var replica = context.Resources.GetVariable(args[1]).ToString();
            var background = context.Resources.GetResource(args[2]);
            ResourceString text = new(args[1], replica);

            await context.Handler.ShowFullscreenReplica(character, background, text, context.CancellationToken);
        }

        #endregion
		
		#region Статика
		
		public static readonly ShowFullScreenReplicaOpCode Instance = new();
		
		#endregion
    }
}