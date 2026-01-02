using DialogMaker.Core.Common;
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

            await context.Handler.ShowReplica(character, replica);
        }

        #endregion

        #region Статика

        public static readonly ShowReplicaOpCode Instance = new();

		#endregion
    }
}