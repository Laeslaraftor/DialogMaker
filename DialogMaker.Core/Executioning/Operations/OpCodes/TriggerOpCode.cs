using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class TriggerOpCode() : OpCode(DialogByteCode.Trigger)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var triggerId = context.Resources.GetVariable(args[0]).ToString();

            await context.Handler.HandleTrigger(triggerId);
        }

        #endregion

        #region Статика

        public static readonly TriggerOpCode Instance = new();

        #endregion
    }
}
