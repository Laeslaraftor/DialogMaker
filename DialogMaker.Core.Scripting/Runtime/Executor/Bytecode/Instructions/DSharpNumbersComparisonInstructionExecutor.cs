using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of number comparison instruction executor
    /// </summary>
    public abstract class DSharpNumbersComparisonInstructionExecutor : DSharpInstructionExecutor
    {
        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckStackValues(instruction, context, 2, out var error))
            {
                return error;
            }

            var left = context.Stack.Peek(1);
            var right = context.Stack.Peek(0);

            if (DSharpMathInstructionExecutor.IsNotValid(instruction, context, left.ValueType, right.ValueType, out var valuesError))
            {
                return valuesError;
            }

            var leftValue = left.ReadAsDecimal();

            if (leftValue == null)
            {
                return context.ThrowExecutionException($"Unable to perform math operation: unsupported left value {left.ValueType}");
            }

            var rightValue = right.ReadAsDecimal();

            if (rightValue == null)
            {
                return context.ThrowExecutionException($"Unable to perform math operation: unsupported right value {right.ValueType}");
            }

            var resultValue = Compare(leftValue.Value, rightValue.Value);
            context.Stack.Push(resultValue);

            return DSharpMethodExecutionCallback.Complete();
        }

        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        /// <summary>
        /// Compare two numbers
        /// </summary>
        /// <param name="left">Left number</param>
        /// <param name="right">Right number</param>
        /// <returns>Comparison result</returns>
        protected abstract bool Compare(decimal left, decimal right);
    }
}
