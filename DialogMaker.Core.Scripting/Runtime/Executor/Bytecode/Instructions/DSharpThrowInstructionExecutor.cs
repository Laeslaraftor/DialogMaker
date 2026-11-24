using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Throw"/> operation
    /// </summary>
    public unsafe class DSharpThrowInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return -1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Throw"/> operation executor
        /// </summary>
        public static readonly DSharpThrowInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            if (context.Stack.Count == 0)
            {
                *context.Callback = DSharpMethodExecutionCallback.Throw(null);
                return;
            }

            var lastValue = context.Stack.Peek();
            DSharpObject* exceptionInstance = null;

            if (lastValue.ValueType == DSharpStackValueType.Reference)
            {
                exceptionInstance = (DSharpObject*)lastValue.ReadReference();

#if DEBUG
                if (exceptionInstance != null &&
                    !exceptionInstance->Type->IsInheritFrom(context.TypesProvider.Exception))
                {
                    *context.Callback = context.ThrowExecutionException($"Throwing available only for exceptions, but got \"{exceptionInstance->Type->ToString()}\"");
                    return;
                }
#endif
            }

            *context.Callback = DSharpMethodExecutionCallback.Throw(exceptionInstance);
        }

        #endregion
    }
}
