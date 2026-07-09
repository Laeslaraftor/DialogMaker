namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Base class of D# instruction executor
    /// </summary>
    public abstract class DSharpInstructionExecutor
    {
        /// <summary>
        /// Execute instruction
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <returns></returns>
        public abstract Task Execute(DSharpRuntimeInstruction instruction, DSharpExecutionContext context);
    }
}
