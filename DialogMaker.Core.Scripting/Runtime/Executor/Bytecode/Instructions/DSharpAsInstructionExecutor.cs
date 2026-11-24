using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.As"/> operation
    /// </summary>
    public unsafe class DSharpAsInstructionExecutor : DSharpTypeInstructionExecutor
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
                    return;
                }

                context.Stack.Pop();
                context.Stack.PushStructure(type);
            }
            else if (lastValue.ValueType != DSharpStackValueType.Structure &&
                     lastValue.ValueType != DSharpStackValueType.Reference)
            {
                *context.Callback = context.ThrowExecutionException($"Unable to cast \"{lastValue.ValueType}\"");
                return;
            }

            var obj = lastValue.ReadAsObject();

            if (obj == null)
            {
                PushNullOrEmpty(context.Stack, type);
                return;
            }
            else if (obj->Type->IsInheritFrom(type))
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
                    return;
                }

                var convertOperator = FindConvertOperator(obj->Type, type);

                if (convertOperator == null)
                {
                    convertOperator = FindConvertOperator(type, obj->Type);
                }
                if (convertOperator == null)
                {
                    PushNullOrEmpty(context.Stack, type);
                    return;
                }

                DSharpCallInstructionExecutor.Call(instruction, ref context, new()
                {
                    CallingType = DSharpMethodCallingType.Default,
                    Member = convertOperator
                }, 1);

                return;
            }

            PushNullOrEmpty(context.Stack, type);
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.As"/> operation executor
        /// </summary>
        public static readonly DSharpAsInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        private static void PushNullOrEmpty(DSharpStack stack, DSharpRuntimeTypeInfo* type)
        {
            stack.Pop();

            if (type->IsValueType)
            {
                stack.PushStructure(type);
                return;
            }

            stack.PushNull();
        }
        private static DSharpRuntimeMethodInfo* FindConvertOperator(DSharpRuntimeTypeInfo* searchType, DSharpRuntimeTypeInfo* convertType)
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
