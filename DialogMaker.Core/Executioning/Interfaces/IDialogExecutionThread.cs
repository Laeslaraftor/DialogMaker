namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutionThread
    {
        public int CurrentSection { get; }

        public void JumpTo(int sectionId);
        public void Goto(int instructionPosition);
        public void StartThread(int sectionId);
        public void StopThread();
        public void StopExecuting();

        public void Push(OperandValue value);
        public OperandValue Pop();
    }
}
