using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of math instruction that takes last 2 numbers from stack,
    /// performs math operation and add result to stack
    /// </summary>
    public abstract class DSharpMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override bool Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (!CheckStackValues(instruction, context, 2))
            {
                return false;
            }

            var left = context.Stack.Peek(1);
            var right = context.Stack.Peek(0);

            if (!IsValid(instruction, context, left.ValueType, right.ValueType))
            {
                return false;
            }
            if (left.ValueType == DSharpStackValueType.Bool &&
                right.ValueType == DSharpStackValueType.Bool)
            {
                var value = PerformMathOperation(left.Read<bool>(), right.Read<bool>());
                context.Stack.Push(value);
                return true;
            }

            var leftValue = left.ReadAsDecimal();

            if (leftValue == null)
            {
                context.ThrowExecutionException($"Unable to perform math operation: unsupported left value {left.ValueType}");
                return false;
            }

            var rightValue = right.ReadAsDecimal();

            if (rightValue == null)
            {
                context.ThrowExecutionException($"Unable to perform math operation: unsupported right value {right.ValueType}");
                return false;
            }

            var resultValue = PerformMathOperation(leftValue.Value, rightValue.Value);
            var bigger = GetBigger(left, right);

            if (!context.Stack.Push(bigger.ValueType, resultValue))
            {
                context.ThrowExecutionException($"Unable to perform math operation: unsupported result value {resultValue} with type {bigger.ValueType}");
                return false;
            }

            return true;
        }

        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream)
        {
            return 0;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, ref UnmanagedStream stream, UnmanagedArray<nint> arguments)
        {
        }

        /// <summary>
        /// Perform math operation between two values
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <returns>Result of math operation</returns>
        protected abstract decimal PerformMathOperation(decimal left, decimal right);
        /// <summary>
        /// Perform math operation between two boolean values
        /// </summary>
        /// <param name="left">Left boolean value</param>
        /// <param name="right">Right boolean value</param>
        /// <returns>Result of math operation</returns>
        protected abstract bool PerformMathOperation(bool left, bool right);
        protected abstract bool CanPerform(DSharpStackValueType left, DSharpStackValueType right);

        private bool IsValid(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, DSharpStackValueType left, DSharpStackValueType right)
        {
            if (left == DSharpStackValueType.Null)
            {
                context.ThrowExecutionException("Unable to perform math operation: left value is null");
                return false;
            }
            if (right == DSharpStackValueType.Null)
            {
                context.ThrowExecutionException("Unable to perform math operation: right value is null");
                return false;
            }
            if (right == DSharpStackValueType.Reference ||
                right == DSharpStackValueType.Structure)
            {
                context.ThrowExecutionException($"Unable to perform math operation: right value \"{right}\" is not supported");
                return false;
            }
            if (!CanPerform(left, right))
            {
                context.ThrowExecutionException($"{instruction.Operation} can not be performed between {left} and {right}");
                return false;
            }

            return true;
        }
        private DSharpStack.FrameInfo GetBigger(DSharpStack.FrameInfo left, DSharpStack.FrameInfo right)
        {
            if (right.Size > left.Size)
            {
                return right;
            }

            return left;
        }
    }
}
