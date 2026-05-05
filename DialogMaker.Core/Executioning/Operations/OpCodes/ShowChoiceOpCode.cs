using DialogMaker.Core.Common;

namespace DialogMaker.Core.Executioning
{
    public class ShowChoiceOpCode() : OpCode(DialogByteCode.ShowChoice)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 4);

            var character = ShowReplicaOpCode.GetCharacter(context, args[0]);
            var listener = ShowReplicaOpCode.GetCharacter(context, args[1]);

            if (context.Resources.GetResource(args[2]) is not IStringCollection strings)
            {
                throw new DialogExecutionException($"Не удалось получить список строк для отображения вариантов ответа");
            }

            var answer = await DispatchHandler(context, h => h.ShowChoice(character, listener, strings, context.CancellationToken));

            if (!context.CancellationToken.IsCancellationRequested)
            {
                context.Resources.SetValue(args[3], answer);
            }
        }

        #endregion

        #region Статика

        public static readonly ShowChoiceOpCode Instance = new();

        #endregion
    }
}