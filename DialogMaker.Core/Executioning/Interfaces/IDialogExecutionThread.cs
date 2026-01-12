namespace DialogMaker.Core.Executioning
{
    public interface IDialogExecutionThread
    {
        public int PreviousSection { get; }
        public int CurrentSection { get; }

        public void JumpTo(int sectionId);
        public void Goto(int instructionPosition);
        public void StartThread(DialogPosition position);
        public void Stop();

        public void Push(OperandValue value);
        public OperandValue Pop();
    }
}
