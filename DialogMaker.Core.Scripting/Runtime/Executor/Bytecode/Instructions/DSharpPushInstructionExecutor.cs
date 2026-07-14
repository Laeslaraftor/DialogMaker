using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Push"/> operation
    /// </summary>
    public class DSharpPushInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe bool Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (!CheckArguments(instruction, context, 1))
            {
                return false;
            }

            var type = (DSharpLiteralType)(*(byte*)instruction.Arguments[0]);
            var valuePointer = instruction.Arguments[0] + sizeof(DSharpLiteralType);

            if (type == DSharpLiteralType.String)
            {
                // create new instance of string
            }

            context.Stack.Push(type, valuePointer);
            return false;
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, bool> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            DSharpLiteralValue.Read(stream);
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream.ReadSafePointer<DSharpLiteralType>();
            stream.Position -= sizeof(DSharpLiteralType);
            DSharpLiteralValue.Read(stream);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Push"/> operation executor
        /// </summary>
        public static readonly DSharpPushInstructionExecutor Instance = new();

        private static bool InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
