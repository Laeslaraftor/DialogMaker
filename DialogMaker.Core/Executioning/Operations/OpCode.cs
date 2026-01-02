using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public abstract class OpCode(DialogByteCode code)
    {
        public DialogByteCode Code { get; } = code;

        #region Управление

        public abstract Task Execute(DialogExecutionContext context, params int[] args);

        protected void CheckArgs(DialogExecutionContext context, int[] args, int requestedLength)
        {
            if (args.Length != requestedLength)
            {
                throw new DialogExecutionException($"Не удалось выполнить операцию {Code} в секции {context.CurrentThread.CurrentSection}. Требуется аргументов: {requestedLength}, получено: {requestedLength}");
            }
        }

        #endregion
    }
}
