using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DialogMaker.Core.Executioning
{
    public class ShowResourceFullScreenReplicaOpCode() : OpCode(DialogByteCode.ShowResourceFullScreenReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = ShowReplicaOpCode.GetCharacter(context, args[0]);
            var replica = ShowResourceReplicaOpCode.GetString(context, args[1]);
            var background = context.Resources.GetResource(args[2]);

            await DispatchHandler(context, h => h.ShowFullscreenReplica(character, background, replica, context.CancellationToken));
        }

        #endregion

        #region Статика

        public static readonly ShowFullScreenReplicaOpCode Instance = new();

        #endregion
    }
}
