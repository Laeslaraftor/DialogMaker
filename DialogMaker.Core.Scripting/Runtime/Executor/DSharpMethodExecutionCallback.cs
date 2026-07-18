using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;
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
        /// Arguments for next calling method
        /// </summary>
        public UnmanagedArray<DSharpExecutionLocalVariable> CallingArguments;
        /// <summary>
        /// Unhandled exception thrown by called method
        /// </summary>
        public DSharpObject* UnhandledException;

        #region Operators

        public static implicit operator bool(DSharpMethodExecutionCallback callback) => callback.Type == DSharpMethodExecutionCallbackType.ExecutionComplete;

        #endregion

        #region Static

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpMethodExecutionCallback Complete() => new()
        {
            Type = DSharpMethodExecutionCallbackType.ExecutionComplete,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpMethodExecutionCallback Return() => new()
        {
            Type = DSharpMethodExecutionCallbackType.Returned,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpMethodExecutionCallback Throw(DSharpObject* exception) => new()
        {
            Type = DSharpMethodExecutionCallbackType.UnhandledException,
            UnhandledException = exception
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpMethodExecutionCallback Call(DSharpObject* objectInstance, DSharpRuntimeMethodInfo* nextMethod, UnmanagedArray<DSharpExecutionLocalVariable> arguments)
        {
            return Call(objectInstance, nextMethod, default, arguments);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DSharpMethodExecutionCallback Call(DSharpObject* objectInstance, DSharpRuntimeMethodInfo* nextMethod, UnmanagedArray<DSharpRuntimeTypeInfo> genericParameters, UnmanagedArray<DSharpExecutionLocalVariable> arguments) => new()
        {
            Type = DSharpMethodExecutionCallbackType.RequiredCallingNextMethod,
            NextMethod = nextMethod,
            ObjectInstance = objectInstance,
            CallingGenericParameters = genericParameters,
            CallingArguments = arguments
        };

        #endregion
    }
}
