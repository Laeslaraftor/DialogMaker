using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Cast"/> operation
    /// </summary>
    public class DSharpCastInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            DSharpRuntimeTypeInfo* type;

            try
            {
                type = context.GetType(metadataToken);
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

            var lastValue = context.Stack.Peek();

            if (lastValue.ValueType == DSharpStackValueType.Null)
            {
                context.Stack.Pop();
                context.Stack.PushStructure(type);
            }
            else if (lastValue.ValueType == DSharpStackValueType.Structure ||
                     lastValue.ValueType == DSharpStackValueType.Reference)
            {
                var obj = lastValue.ReadAsObject();
                decimal decimalValue;

                if (obj == null)
                {
                    return context.ThrowExecutionException("Unable to cast null object");
                }
                else if (!obj->Type->IsValueType)
                {
                    return context.ThrowExecutionException($"Cast available only for value types, got: {obj->Type->ToString()}");
                }
                if (obj->Type == context.TypesProvider.Boolean)
                {
                    decimalValue = lastValue.ReadAsBoolean() ? 1 : 0;
                }
                else
                {
                    decimalValue = lastValue.ReadAsDecimal().GetValueOrDefault();
                }

                context.Stack.Pop(0);
                context.Stack.Push(type, decimalValue);
            }
            else
            {
                return context.ThrowExecutionException($"Unable to cast \"{lastValue.ValueType}\"");
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Cast"/> operation executor
        /// </summary>
        public static readonly DSharpCastInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
