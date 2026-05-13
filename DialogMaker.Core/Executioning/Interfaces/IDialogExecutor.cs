namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutor
    {
        public event EventHandler? Disposed;
        public event EventHandler<DialogExecutorHandleEventArgs>? DialogHandled;

        public bool IsRunning { get; }
        public bool IsPaused { get; }
        public IDialogExecutingHandler? Handler { get; }

        public void PushIsolatedVariablesToResources();
        public void Start(bool isolated);
        public void Stop();
        public void Pause();
        public void Resume();
    }
}
