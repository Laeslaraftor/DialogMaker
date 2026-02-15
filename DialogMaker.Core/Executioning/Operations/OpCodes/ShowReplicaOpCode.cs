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
            CheckArgs(context, args, 3);

            var character = GetCharacter(context, args[0]);
            var listener = GetCharacter(context, args[1]);
            var replica = context.Resources.GetVariable(args[2]).ToString();
            ResourceString text = new(args[2], replica);

            await DispatchHandler(context, h => h.ShowReplica(character, listener, text, context.CancellationToken));
        }

        #endregion

        #region Статика

        public static readonly ShowReplicaOpCode Instance = new();

        internal static ICharacter? GetCharacter(DialogExecutionContext context, int index)
        {
            if (index == -1)
            {
                return null;
            }

            return context.Resources.GetResource(index) as ICharacter;
        }

		#endregion
    }
}