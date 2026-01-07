using System;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogExecutorHandleEventArgs(DialogExecutor executor, Action<IDialogExecutingHandler> handler)
    {
        public DialogExecutor DialogExecutor { get; } = executor;

        private readonly Action<IDialogExecutingHandler> _handler = handler;

        #region Управление

        public void Handle(IDialogExecutingHandler handler) => _handler(handler);

        #endregion
    }
}
