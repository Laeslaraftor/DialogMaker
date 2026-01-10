using DialogMaker.Core.Common;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DialogMaker.Core.Executioning
{
    public class TriggerOpCode() : OpCode(DialogByteCode.Trigger)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var triggerId = context.Resources.GetVariable(args[0]).ToString();

            await DispatchHandler(context, h => h.HandleTrigger(triggerId, context.CancellationToken));
        }

        #endregion

        #region Статика

        public static readonly TriggerOpCode Instance = new();

        #endregion
    }
}
