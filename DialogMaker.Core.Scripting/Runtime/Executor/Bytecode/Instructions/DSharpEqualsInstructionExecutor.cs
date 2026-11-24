using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Equals"/> operation
    /// </summary>
    public class DSharpEqualsInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        public override unsafe delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Equals(ref DSharpStack.FrameInfo left, ref DSharpStack.FrameInfo right)
        {
            var leftType = left.ObjectType;
            var rightType = right.ObjectType;

            if (leftType != rightType &&
                leftType != null && rightType != null &&
                !leftType->ConverterInfo.IsNull && !rightType->ConverterInfo.IsNull)
            {
                var leftDecimal = left.ReadAsNumber();
                var rightDecimal = right.ReadAsNumber();

                return (leftDecimal == rightDecimal)._bool;
            }

            return DSharpStack.FrameInfo.ValueEquals(ref left, ref right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 2))
            {
                return;
            }
#endif

            var left = context.Stack.Peek(1);
            var right = context.Stack.Peek();
            bool result = Equals(ref left, ref right);

            context.Stack.Push(result);
        }

        #endregion
    }
}
