using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Throw"/> operation
    /// </summary>
    public class DSharpThrowInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (context.Stack.Count == 0)
            {
                return DSharpMethodExecutionCallback.Throw(null);
            }

            var lastValue = context.Stack.Peek();
            DSharpObject* exceptionInstance = null;

            if (lastValue.ValueType == DSharpStackValueType.Reference)
            {
                exceptionInstance = (DSharpObject*)lastValue.ReadReference();

                if (exceptionInstance != null &&
                    !exceptionInstance->Type->IsInheritFrom(context.TypesProvider.Exception))
                {
                    return context.ThrowExecutionException($"Throwing available only for exceptions, but got \"{exceptionInstance->Type->ToString()}\"");
                }
            }

            return DSharpMethodExecutionCallback.Throw(exceptionInstance);
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return -1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Throw"/> operation executor
        /// </summary>
        public static readonly DSharpThrowInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
