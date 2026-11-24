using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of math instruction that takes last 2 numbers from stack,
    /// performs math operation and add result to stack
    /// </summary>
    public unsafe abstract class DSharpMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 2))
            {
                return;
            }
#endif

            var stack = context.Stack;
            var left = stack.FastNumberPeek(1);
            var right = stack.FastNumberPeek();
            var resultValue = PerformMathOperation(ref left, ref right, ref context);

            if (resultValue._type == DSharpLiteralType.Null)
            {
				*context.Callback = context.ThrowDivideByZeroException();
                return;
            }

            context.Stack.Push(&resultValue);
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        /// <summary>
        /// Perform math operation between two values
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>Result of math operation</returns>
        protected abstract DSharpNumber PerformMathOperation(ref DSharpNumber left, ref DSharpNumber right, ref DSharpExecutionContext context);
    }
}
