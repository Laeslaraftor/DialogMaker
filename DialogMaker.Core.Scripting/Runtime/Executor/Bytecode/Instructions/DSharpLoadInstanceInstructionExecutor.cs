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
           
            // Force push reference to current instance.
            // If current type is value type and push it as structure
            // it counts as creating new instance and because of this
            // makes impossible to set fields values to current instance.
            // Force pushing reference not make value types boxing,
            // it just push pointer to other stack frame (with current instance)
            context.Stack.PushReference(context.ObjectInstance, true);

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
