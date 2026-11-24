using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.Cast"/> operation
    /// </summary>
    public unsafe class DSharpCastInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* type)
        {
#if DEBUG
            if (CheckStackValues(instruction, ref context, 1))
            {
                return;
            }
#endif

            var lastValue = context.Stack.Peek();

            if (lastValue.ValueType == DSharpStackValueType.Null)
            {
                if (!type->IsValueType)
                {
                    *context.Callback = DSharpMethodExecutionCallback.Complete();
                    return;
                }

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
                    *context.Callback = context.ThrowExecutionException("Unable to cast null object");
                    return;
                }
                else if (!obj->Type->IsValueType)
                {
                    *context.Callback = context.ThrowExecutionException($"Cast available only for value types, got: {obj->Type->ToString()}");
                    return;
                }
                if (obj->Type == type)
                {
                    if (type->IsValueType && obj->IsReferenceObject)
                    {
                        context.Stack.Pop();
                        context.Stack.PushStructure(obj, true);
                    }
                    else if (obj->Type->IsValueType && !obj->IsReferenceObject && !type->IsValueType)
                    {
                        var boxed = context.ObjectsContainer.Box(obj);
                        context.Stack.Pop();
                        context.Stack.PushReference(boxed, true);
                    }

                    return;
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
                *context.Callback = context.ThrowExecutionException($"Unable to cast \"{lastValue.ValueType}\" to \"{type->ToString()}\"");
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.Cast"/> operation executor
        /// </summary>
        public static readonly DSharpCastInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
