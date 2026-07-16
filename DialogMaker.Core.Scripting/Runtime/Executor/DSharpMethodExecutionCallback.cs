using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// Method execution callback
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpMethodExecutionCallback
    {
        /// <summary>
        /// Callback type
        /// </summary>
        public DSharpMethodExecutionCallbackType Type;
        /// <summary>
        /// Next method that required to execute
        /// </summary>
        public DSharpRuntimeMethodInfo* NextMethod;
        /// <summary>
        /// Object instance for executing next method.
        /// It may be null if next method is static
        /// </summary>
        public DSharpObject* ObjectInstance;
        /// <summary>
        /// Generic parameter for next calling method
        /// </summary>
        public UnmanagedArray<DSharpRuntimeTypeInfo> CallingGenericParameters;
        /// <summary>
        /// Index on that starts arguments for executing next method.
        /// It contains -1 if no arguments provided
        /// </summary>
        public int CallingArgumentsStartStackIndex;
        /// <summary>
        /// Unhandled exception thrown by called method
        /// </summary>
        public DSharpObject* UnhandledException;
    }
}
