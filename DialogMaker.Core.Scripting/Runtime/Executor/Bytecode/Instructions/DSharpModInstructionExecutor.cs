using DialogMaker.Core.Scripting.Runtime.Numbers.MathExecutors;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Mod"/> operation
    /// </summary>
    public unsafe class DSharpModInstructionExecutor : DSharpMathInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override sealed DSharpNumber PerformMathOperation(ref DSharpNumber left, ref DSharpNumber right, ref DSharpExecutionContext context)
        {
            return DSharpNumberMathExecutor.Mod(ref left, ref right);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Mod"/> operation executor
        /// </summary>
        public static readonly DSharpModInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
