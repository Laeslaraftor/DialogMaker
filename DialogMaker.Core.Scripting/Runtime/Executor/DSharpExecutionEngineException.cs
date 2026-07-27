namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    public unsafe class DSharpExecutionEngineException : DSharpException
    {
        public DSharpExecutionEngineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public DSharpExecutionEngineException(string message, DSharpObject* dSharpException)
            : base(message)
        {
            DSharpException = dSharpException;
        }
        public DSharpExecutionEngineException(string message)
            : base(message)
        {
        }

        public DSharpObject* DSharpException { get; }
    }
}
