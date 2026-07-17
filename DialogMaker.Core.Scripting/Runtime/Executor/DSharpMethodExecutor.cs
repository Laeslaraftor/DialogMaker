using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# method executor
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpMethodExecutor
    {
        /// <summary>
        /// Stack scope that contains current method executor
        /// include self
        /// </summary>
        public DSharpStack.Scope Scope;
        /// <summary>
        /// Previous method executor
        /// </summary>
        public DSharpMethodExecutor* PreviousExecutor;
        /// <summary>
        /// Current object instance
        /// </summary>
        public DSharpObject* ObjectInstance;
        /// <summary>
        /// Current executing method
        /// </summary>
        public DSharpRuntimeMethodInfo* MethodInfo;
        /// <summary>
        /// Current executing method bytecode
        /// </summary>
        public DSharpRuntimeBytecode* Bytecode;
        /// <summary>
        /// Method local variables
        /// </summary>
        public UnmanagedArray<DSharpExecutionLocalVariable> LocalVariables;
        /// <summary>
        /// Method calling arguments
        /// </summary>
        public UnmanagedArray<DSharpExecutionLocalVariable> Arguments;
        /// <summary>
        /// Execution generic parameter
        /// </summary>
        public UnmanagedArray<DSharpRuntimeTypeInfo> GenericParameters;
        /// <summary>
        /// List of registered catch blocks
        /// </summary>
        public UnmanagedList<DSharpCatchBlock> CatchBlocks;
        /// <summary>
        /// Scopes that created by bytecode
        /// </summary>
        public UnmanagedList<DSharpStack.Scope> LocalScopes;
        /// <summary>
        /// Current executing instruction index
        /// </summary>
        public uint InstructionIndex;
        /// <summary>
        /// Is unhandled exception exists
        /// </summary>
        public bool HaveUnhandledException;
        /// <summary>
        /// Exception that currently unhandled
        /// </summary>
        public DSharpObject* UnhandledException;

        /// <summary>
        /// Execute current method starts with specified instruction index
        /// </summary>
        /// <param name="objectsContainer">Object instances container</param>
        /// <param name="thread">Current thread</param>
        /// <returns>Method execution callback that contains result of method execution</returns>
        public DSharpMethodExecutionCallback Execute(DSharpObjectsContainer objectsContainer, DSharpThread thread)
        {
            int scopesCount = Math.Max(1, (int)Bytecode->ScopesCount);
            Span<DSharpExecutionScope> scopes = stackalloc DSharpExecutionScope[scopesCount];
            DSharpExecutionContext context = new(objectsContainer, thread, ObjectInstance, MethodInfo)
            {
                InstructionIndex = InstructionIndex
            };
            DSharpMethodExecutionCallback? result = null;

            while (context.InstructionIndex < Bytecode->Instructions.Length)
            {
                var instruction = Bytecode->Instructions[(int)context.InstructionIndex];
                var callback = instruction.Execute(ref context);

                if (callback.Type != DSharpMethodExecutionCallbackType.ExecutionComplete)
                {
                    result = callback;
                    break;
                }

                context.InstructionIndex++;
            }

            InstructionIndex = context.InstructionIndex;

            return result ?? DSharpMethodExecutionCallback.Complete();
        }
        /// <summary>
        /// Try find catch block for specified exception
        /// </summary>
        /// <param name="exception">Exception for searching catch block that can handle it</param>
        /// <param name="result">Catch block that found</param>
        /// <returns>Is catch block found</returns>
        public readonly bool TryFindCatchBlockForException(DSharpObject* exception, out DSharpCatchBlock result)
        {
            for (int i = CatchBlocks.Count - 1; i >= 0; i--)
            {
                var catchBlock = CatchBlocks[i];

                if (exception == null)
                {
                    if (catchBlock.ExceptionType == null)
                    {
                        result = catchBlock;
                        return true;
                    }

                    continue;
                }

                var exceptionInstanceType = exception->Type;

                if (exceptionInstanceType == catchBlock.ExceptionType ||
                    exceptionInstanceType->IsInheritFrom(catchBlock.ExceptionType))
                {
                    result = catchBlock;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
