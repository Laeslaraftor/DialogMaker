using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Class with base implementation for jump instructions
    /// </summary>
    public abstract class DSharpBaseJumpInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (IsNotValid(instruction, context, out var error))
            {
                return error;
            }
            if (CanJump(instruction, context))
            {
                context.InstructionIndex = *(uint*)instruction.Arguments[0];
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            stream.Read<uint>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream.ReadSafePointer<uint>();
        }

        /// <summary>
        /// Check jump availability
        /// </summary>
        /// <param name="instruction">Current executing instructions</param>
        /// <param name="context">Current execution context</param>
        /// <returns>Is jump available</returns>
        protected abstract bool CanJump(DSharpRuntimeInstruction instruction, DSharpExecutionContext context);
        /// <summary>
        /// Validate instruction and context
        /// </summary>
        /// <param name="instruction">Current executing instructions</param>
        /// <param name="context">Current execution context</param>
        /// <returns>Is execution available</returns>
        protected virtual bool IsNotValid(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            return CheckArguments(instruction, context, 1, out errorCallback);
        }

        #endregion
    }
}
