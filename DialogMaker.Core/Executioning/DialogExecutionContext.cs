namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogExecutionContext(IDialogExecutionThread thread, IDialogExecutingThreadManager threadManager, IDialogExecutionResources resources, IDialogExecutingHandler handler, CancellationToken cancellationToken)
    {
        public IDialogExecutionThread CurrentThread { get; } = thread;
        public IDialogExecutingThreadManager ThreadManager { get; } = threadManager;
        public IDialogExecutionResources Resources { get; } = resources;
        public IDialogExecutingHandler Handler { get; } = handler;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }
}
