namespace DialogMaker.Core.Executioning
{
    public interface IJoinController
    {
        public bool IsCompleted { get; }
        public bool IsBusy { get; }
        public IJoinOperationInfo JoinInfo { get; }

        public bool CanJoin(DialogExecutionContext context);
        public Task<bool> Join(DialogExecutionContext context);
        public void Clear();
    }
}
