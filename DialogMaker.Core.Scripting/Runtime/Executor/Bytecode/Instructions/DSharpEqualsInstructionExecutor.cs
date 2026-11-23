using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Equals"/> operation
    /// </summary>
    public class DSharpEqualsInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, context, 2, out var error))
            {
                return error;
            }
#endif

            var left = context.Stack.Peek(1);
            var right = context.Stack.Peek();
            bool result = Equals(left, right);

            context.Stack.Push(result);

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            return 0;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Equals"/> operation executor
        /// </summary>
        public static readonly DSharpEqualsInstructionExecutor Instance = new();

        internal static unsafe bool Equals(DSharpStack.FrameInfo left, DSharpStack.FrameInfo right)
        {
            var leftType = left.ObjectType;
            var rightType = right.ObjectType;

            if (leftType != rightType &&
                leftType != null && rightType != null &&
                !leftType->ConverterInfo.IsNull && !rightType->ConverterInfo.IsNull)
            {
                var leftDecimal = left.ReadAsNumber();
                var rightDecimal = right.ReadAsNumber();

                return (leftDecimal == rightDecimal).Read<bool>();
            }

            return DSharpStack.FrameInfo.ValueEquals(ref left, ref right);
        }

        public unsafe static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
