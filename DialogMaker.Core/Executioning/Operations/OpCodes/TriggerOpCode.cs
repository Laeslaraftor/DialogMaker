using DialogMaker.Core.Executioning.Internal;

namespace DialogMaker.Core.Executioning
{
    public class TriggerOpCode() : OpCode(DialogByteCode.Trigger)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 1);

            var resource = context.Resources.GetResource(args[0]);

            if (resource is not TriggerMetadata metadata)
            {
                throw new DialogExecutionException("Не удалось получить метаданные события");
            }

            var trigger = Trigger.Create(metadata, context);

            await DispatchHandler(context, h => h.HandleTrigger(trigger, context.CancellationToken));
        }

        #endregion

        #region Статика

        public static readonly TriggerOpCode Instance = new();

        #endregion
    }
}
