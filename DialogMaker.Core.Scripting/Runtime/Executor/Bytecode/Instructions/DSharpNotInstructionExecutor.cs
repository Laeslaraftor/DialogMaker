using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Not"/> operation
    /// </summary>
    public class DSharpNotInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override bool Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (!CheckStackValues(instruction, context, 1))
            {
                return false;
            }

            var value = context.Stack.Peek();

            if (value.ValueType != DSharpStackValueType.Bool)
            {
                context.ThrowExecutionException($"Unable to invert value because it is not boolean: {value.ValueType}");
                return false;
            }

            value.Write(value.Read<bool>());

            return true;
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            throw new NotImplementedException();
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Not"/> operation executor
        /// </summary>
        public static readonly DSharpNotInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
