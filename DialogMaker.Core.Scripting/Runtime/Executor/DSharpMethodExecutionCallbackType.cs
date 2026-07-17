namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Type of method execution callback
    /// </summary>
    public enum DSharpMethodExecutionCallbackType : byte
    {
        /// <summary>
        /// Method execution successfully completed
        /// </summary>
        ExecutionComplete,
        /// <summary>
        /// Method was retuned
        /// </summary>
        Returned,
        /// <summary>
        /// An exception was thrown and not handled
        /// </summary>
        UnhandledException,
        /// <summary>
        /// Method requested a call to another method and is awaiting the resumption of its own execution upon the latter's completion.
        /// </summary>
        RequiredCallingNextMethod
    }
}
