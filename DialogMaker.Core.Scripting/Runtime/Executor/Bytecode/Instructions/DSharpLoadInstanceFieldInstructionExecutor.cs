using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadInstanceField"/> operation
    /// </summary>
    public class DSharpLoadInstanceFieldInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (context.ObjectInstance == null)
            {
                context.ThrowExecutionException("Unable to load current instance from static method");
                return false;
            }
            if (context.ObjectInstance->Type->IsValueType)
            {
                var frame = context.Stack.PushStructure(context.ObjectInstance->Type->Size);
                frame.Write(0, *context.ObjectInstance);
            }
            else
            {
                context.Stack.PushReference((nint)context.ObjectInstance);
            }

            return true;
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadInstanceField"/> operation executor
        /// </summary>
        public static readonly DSharpLoadInstanceFieldInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
