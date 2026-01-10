using System;
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
        internal static async Task DispatchHandler(DialogExecutionContext context, Func<IDialogExecutingHandler, Task> handler)
        {
            await DialogExecutor.DispatchHandler(context.Handler, handler);
        }
        internal static async Task<T?> DispatchHandler<T>(DialogExecutionContext context, Func<IDialogExecutingHandler, Task<T>> handler)
        {
            return await DialogExecutor.DispatchHandler(context.Handler, handler);
        }

        #endregion
    }
}
