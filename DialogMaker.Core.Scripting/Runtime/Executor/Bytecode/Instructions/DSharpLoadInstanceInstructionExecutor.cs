using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.LoadInstance"/> operation
    /// </summary>
    public unsafe class DSharpLoadInstanceInstructionExecutor : DSharpInstructionExecutor
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
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.LoadInstance"/> operation executor
        /// </summary>
        public static readonly DSharpLoadInstanceInstructionExecutor Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (context.CurrentMethod->IsStatic)
            {
                *context.Callback = context.ThrowExecutionException("Unable to load current instance from static method");
                return;
            }
            if (context.ObjectInstance == null)
            {
                *context.Callback = context.ThrowExecutionException("Unable to load current instance when it not provided");
                return;
            }
#endif

            var obj = context.ObjectInstance;

            if (obj->Type->ConverterInfo.IsNumber)
            {
                context.Stack.PushStructure(obj, true);
            }
            else
            {
                // Force push reference to current instance.
                // If current type is value type and push it as structure
                // it counts as creating new instance and because of this
                // makes impossible to set fields values to current instance.
                // Force pushing reference not make value types boxing,
                // it just push pointer to other stack frame (with current instance)
                context.Stack.PushReference(obj, true);
            }
        }

        #endregion
    }
}
