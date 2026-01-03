using DialogMaker.Core.Common;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class ShowChoiceOpCode() : OpCode(DialogByteCode.ShowChoice)
    {
        #region Управление

        public override async Task Execute(DialogExecutionContext context, params int[] args)
        {
            CheckArgs(context, args, 3);

            var character = context.Resources.GetResource(args[0]) as ICharacter;

            if (context.Resources.GetResource(args[1]) is not IStringCollection strings)
            {
                throw new DialogExecutionException($"Не удалось получить список строк для отображения вариантов ответа");
            }

            var answer = await context.Handler.ShowChoice(character, strings, context.CancellationToken);

            if (!context.CancellationToken.IsCancellationRequested)
            {
                context.Resources.SetVariable(args[2], answer);
            }
        }

        #endregion

        #region Статика

        public static readonly ShowChoiceOpCode Instance = new();

        #endregion
    }
}