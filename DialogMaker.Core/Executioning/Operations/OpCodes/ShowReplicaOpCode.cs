using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowReplicaOpCode() : OpCode(DialogByteCode.ShowReplica)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 2);

            var character = context.Resources.GetResource(args[0]) as ICharacter;
            var replica = context.Resources.GetVariable(args[1]).ToString();
            ResourceString text = new(args[1], replica);

            await context.Handler.ShowReplica(character, text, context.CancellationToken);
        }

        #endregion

        #region Статика

        public static readonly ShowReplicaOpCode Instance = new();

		#endregion
    }
}