namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutingThreadManager
    {
        public IDialogExecutor DialogExecutor { get; }
        public bool IsRunning { get; }
        public bool IsPaused { get; }

        public DialogExecutionJoinController GetJoinController(DialogExecutionContext context, IJoinOperationInfo joinInfo);
        public DialogExecutionIntersectController GetIntersectController(DialogExecutionContext context, IJoinOperationInfo joinInfo);
        public void StartThread(DialogPosition position);
        public void StartThread(IDialogExecutionThread source, DialogPosition position);
        public void Stop();
    }
}
