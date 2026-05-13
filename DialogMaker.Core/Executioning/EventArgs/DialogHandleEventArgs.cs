namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogHandleEventArgs(IDialogExecutor dialogExecutor, CancellationToken cancellationToken)
    {
        public IDialogExecutor DialogExecutor { get; } = dialogExecutor;
        public CancellationToken CancellationToken { get; } = cancellationToken;
        public bool IsCancellationRequested => CancellationToken.IsCancellationRequested;

        public static implicit operator CancellationToken(DialogHandleEventArgs e) => e.CancellationToken;
    }
}
