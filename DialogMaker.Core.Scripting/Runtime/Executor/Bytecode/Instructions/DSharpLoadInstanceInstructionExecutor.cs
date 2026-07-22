using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadInstance"/> operation
    /// </summary>
    public class DSharpLoadInstanceInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (context.CurrentMethod->IsStatic)
            {
                return context.ThrowExecutionException("Unable to load current instance from static method");
            }
            if (context.ObjectInstance == null)
            {
                return context.ThrowExecutionException("Unable to load current instance when it not provided");
            }
            if (context.ObjectInstance->Type->IsValueType)
            {
                context.Stack.PushStructure(context.ObjectInstance, false);
            }
            else
            {
                context.Stack.PushReference(context.ObjectInstance);
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadInstance"/> operation executor
        /// </summary>
        public static readonly DSharpLoadInstanceInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
