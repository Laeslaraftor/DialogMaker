using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreLocal"/> operation
    /// </summary>
    public unsafe class DSharpStoreLocalInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, context, 1, out var error) ||
                CheckStackValues(instruction, context, 1, out error))
            {
                return error;
            }
#endif

            uint variableIndex = *(uint*)instruction->Arguments[0];
            var variables = context.LocalVariables;

#if DEBUG
            if (variableIndex >= variables->Length)
            {
                return context.ThrowExecutionException($"Invalid local variable index \"{variableIndex}\" when available variables \"{variables->Length}\"");
            }
#endif

            var variable = variables->GetItemReference((int)variableIndex);
            var stackValue = context.Stack.PeekOnlyValues();
            var objectReference = (DSharpObject*)stackValue.ReadReference();

            // Unboxing references to stack
            if (stackValue.ValueType == DSharpStackValueType.Reference &&
                objectReference != null && !objectReference->IsReferenceObject &&
                context.ObjectsContainer.Unbox(objectReference, variable->Buffer))
            {
                variable->Buffer.ValueType = DSharpStackValueType.Structure;
            }
            else if (!variable->ParameterInfo.Type->IsValueType &&
                     stackValue.ValueType == DSharpStackValueType.Structure)
            {
                var boxedValue = (nint)context.ObjectsContainer.Box((DSharpObject*)stackValue.StackPointer);
                variable->Buffer.ValueType = DSharpStackValueType.Reference;
                variable->Buffer.Write(boxedValue);
            }
            else
            {
                variable->Buffer.ValueType = stackValue.ValueType;
                variable->Buffer.Write(ref stackValue);
            }

            variable->Buffer.Mode = stackValue.Mode;

            return DSharpMethodExecutionCallback.Complete();
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<uint>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<uint>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreLocal"/> operation executor
        /// </summary>
        public static readonly DSharpStoreLocalInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
