using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.As"/> operation
    /// </summary>
    public class DSharpAsInstructionExecutor : DSharpTypeInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* type)
        {
            if (CheckStackValues(instruction, context, 1, out var error))
            {
                return error;
            }

            var lastValue = context.Stack.Peek();

            if (lastValue.ValueType == DSharpStackValueType.Null)
            {
                if (!type->IsValueType)
                {
                    return DSharpMethodExecutionCallback.Complete();
                }

                context.Stack.Pop();
                context.Stack.PushStructure(type);
            }
            else if (lastValue.ValueType != DSharpStackValueType.Structure &&
                     lastValue.ValueType != DSharpStackValueType.Reference)
            {
                return context.ThrowExecutionException($"Unable to cast \"{lastValue.ValueType}\"");
            }

            var obj = lastValue.ReadAsObject();

            if (obj == null)
            {
                PushNullOrEmpty(context.Stack, type);
                return DSharpMethodExecutionCallback.Complete();
            }
            else if (obj->Type->IsValueType &&
                     obj->Type->BuildInValueTypeIndex != -1 &&
                     type->BuildInValueTypeIndex != -1)
            {
                decimal decimalValue;

                if (obj->Type == context.TypesProvider.Boolean)
                {
                    decimalValue = lastValue.ReadAsBoolean() ? 1 : 0;
                }
                else
                {
                    decimalValue = lastValue.ReadAsDecimal().GetValueOrDefault();
                }

                context.Stack.Pop();
                context.Stack.Push(type, decimalValue);
            }
            else
            {
                if (!obj->Type->IsValueType &&
                    type->IsInheritFrom(obj->Type))
                {
                    return DSharpMethodExecutionCallback.Complete();
                }

                var convertOperator = FindConvertOperator(obj->Type, type);

                if (convertOperator == null)
                {
                    convertOperator = FindConvertOperator(type, obj->Type);
                }
                if (convertOperator == null)
                {
                    PushNullOrEmpty(context.Stack, type);
                    return DSharpMethodExecutionCallback.Complete();
                }

                return DSharpCallInstructionExecutor.Call(instruction, ref context, convertOperator, false, false, null, 1);
            }

            PushNullOrEmpty(context.Stack, type);
            return DSharpMethodExecutionCallback.Complete();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.As"/> operation executor
        /// </summary>
        public static readonly DSharpAsInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        private static unsafe void PushNullOrEmpty(DSharpStack stack, DSharpRuntimeTypeInfo* type)
        {
            stack.Pop();

            if (type->IsValueType)
            {
                stack.PushStructure(type);
                return;
            }

            stack.PushNull();
        }
        private static unsafe DSharpRuntimeMethodInfo* FindConvertOperator(DSharpRuntimeTypeInfo* searchType, DSharpRuntimeTypeInfo* convertType)
        {
            for (int i = 0; i < searchType->Methods.Length; i++)
            {
                var method = searchType->Methods.GetItemReference(i);

                if (method->MethodType != DSharpMethodType.Operator ||
                    method->ParametersType.Length != 1 ||
                    !convertType->IsInheritFrom(method->ParametersType[0].Type) ||
                    method->ReturnType != searchType)
                {
                    continue;
                }

                return method;
            }

            return null;
        }

        #endregion
    }
}
