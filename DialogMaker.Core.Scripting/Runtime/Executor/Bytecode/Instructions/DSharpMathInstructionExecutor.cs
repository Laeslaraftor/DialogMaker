using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of math instruction that takes last 2 numbers from stack,
    /// performs math operation and add result to stack
    /// </summary>
    public abstract class DSharpMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckStackValues(instruction, context, 2, out var error))
            {
                return error;
            }

            var left = context.Stack.Peek(1);
            var right = context.Stack.Peek(0);

            if (IsNotValidOrCantPerform(instruction, context, left.ValueType, right.ValueType, out var valuesError))
            {
                return valuesError;
            }
            if (left.ValueType == DSharpStackValueType.Bool &&
                right.ValueType == DSharpStackValueType.Bool)
            {
                var value = PerformMathOperation(left.Read<bool>(), right.Read<bool>());
                context.Stack.Push(value);

                return DSharpMethodExecutionCallback.Complete();
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

            var resultValue = PerformMathOperation(leftValue.Value, rightValue.Value);
            var bigger = GetBigger(left, right);

            if (!context.Stack.Push(bigger.ValueType, resultValue))
            {
                return context.ThrowExecutionException($"Unable to perform math operation: unsupported result value {resultValue} with type {bigger.ValueType}");
            }

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

        private bool IsNotValidOrCantPerform(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, DSharpStackValueType left, DSharpStackValueType right, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            if (IsNotValid(instruction, context, left, right, out errorCallback))
            {
                return false;
            }
            if (!CanPerform(left, right))
            {
                errorCallback = context.ThrowExecutionException($"{instruction.Operation} can not be performed between {left} and {right}");
                return true;
            }

            errorCallback = default;
            return false;
        }
        private DSharpStack.FrameInfo GetBigger(DSharpStack.FrameInfo left, DSharpStack.FrameInfo right)
        {
            if (right.Size > left.Size)
            {
                return right;
            }

            return left;
        }

        #region Static

        internal static bool IsNotValid(DSharpRuntimeInstruction instruction, DSharpExecutionContext context, DSharpStackValueType left, DSharpStackValueType right, [NotNullWhen(true)] out DSharpMethodExecutionCallback errorCallback)
        {
            if (left == DSharpStackValueType.Null)
            {
                errorCallback = context.ThrowExecutionException("Unable to perform math operation: left value is null");
                return true;
            }
            if (right == DSharpStackValueType.Null)
            {
                errorCallback = context.ThrowExecutionException("Unable to perform math operation: right value is null");
                return true;
            }
            if (right == DSharpStackValueType.Reference ||
                right == DSharpStackValueType.Structure)
            {
                errorCallback = context.ThrowExecutionException($"Unable to perform math operation: right value \"{right}\" is not supported");
                return true;
            }

            errorCallback = default;
            return false;
        }

        #endregion
    }
}
