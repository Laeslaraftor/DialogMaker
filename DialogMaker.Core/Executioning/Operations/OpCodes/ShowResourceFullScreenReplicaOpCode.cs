using DialogMaker.Core.Common;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowResourceFullScreenReplicaOpCode() : OpCode(DialogByteCode.ShowResourceFullScreenReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = context.Resources.GetResource(args[0]) as ICharacter;
            var replica = ShowResourceReplicaOpCode.GetString(context, args[1]);
            var background = context.Resources.GetResource(args[2]);

            await context.Handler.ShowFullscreenReplica(character, background, replica, context.CancellationToken);
        }

        #endregion

        #region Статика

        public static readonly ShowFullScreenReplicaOpCode Instance = new();

        #endregion
    }
}
