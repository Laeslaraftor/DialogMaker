using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of math instruction that takes last 2 numbers from stack,
    /// performs math operation and add result to stack
    /// </summary>
    public unsafe abstract class DSharpMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, context, 2, out var error))
            {
                return error;
            }
#endif

            var left = context.Stack.Peek(1).ReadAsNumber();
            var right = context.Stack.Peek(0).ReadAsNumber();
            var resultValue = PerformMathOperation(left, right, ref context, out var mathError);

            if (mathError != null)
            {
                return mathError.Value;
            }

            context.Stack.Push(resultValue);

            return DSharpMethodExecutionCallback.Complete();
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        /// <summary>
        /// Perform math operation between two values
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>Result of math operation</returns>
        protected abstract DSharpNumber PerformMathOperation(in DSharpNumber left, in DSharpNumber right, ref DSharpExecutionContext context, out DSharpMethodExecutionCallback? error);
    }
}
