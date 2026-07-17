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
                context.ThrowExecutionException("Unable to load current instance from static method");
                return false;
            }
            if (context.ObjectInstance == null)
            {
                context.ThrowExecutionException("Unable to load current instance when it not provided");
                return false;
            }
            if (context.ObjectInstance->Type->IsValueType)
            {
                var frame = context.Stack.PushStructure(context.ObjectInstance->TotalSize);
                DSharpObject.Copy(context.ObjectInstance, (DSharpObject*)frame.StackPointer);
            }
            else
            {
                context.Stack.PushReference(context.ObjectInstance);
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
