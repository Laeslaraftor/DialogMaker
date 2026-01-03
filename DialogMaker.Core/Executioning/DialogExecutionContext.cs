using DialogMaker.Core.Common;
using System.Threading;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogExecutionContext(IDialogExecutionThread thread, IDialogExecutionResources resources, IDialogExecutingHandler handler, CancellationToken cancellationToken)
    {
        public IDialogExecutionThread CurrentThread { get; } = thread;
        public IDialogExecutionResources Resources { get; } = resources;
        public IDialogExecutingHandler Handler { get; } = handler;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }
}
