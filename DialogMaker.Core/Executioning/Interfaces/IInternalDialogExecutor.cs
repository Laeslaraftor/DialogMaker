using System;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    internal interface IInternalDialogExecutor
    {
        public Task HandleDialog(Func<IDialogExecutingHandler, Task> handler, CancellationToken cancellationToken);
        public Task<T?> HandleDialog<T>(Func<IDialogExecutingHandler, Task<T>> handler, CancellationToken cancellationToken, T? defaultValue = default);
        public void InvokeDialogHandled(Action<IDialogExecutingHandler> handler);
    }
}
