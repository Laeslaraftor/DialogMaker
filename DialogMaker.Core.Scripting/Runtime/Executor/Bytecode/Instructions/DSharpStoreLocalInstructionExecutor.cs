using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.StoreLocal"/> operation
    /// </summary>
    public class DSharpStoreLocalInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error) ||
                CheckStackValues(instruction, context, 1, out error))
            {
                return error;
            }

            uint variableIndex = *(uint*)instruction.Arguments[0];
            var variables = context.LocalVariables;

            if (variableIndex >= variables->Length)
            {
                return context.ThrowExecutionException($"Invalid local variable index \"{variableIndex}\" when available variables \"{variables->Length}\"");
            }

            var variable = variables->GetItemReference((int)variableIndex);
            var stackValue = context.Stack.Peek();
            var objectReference = (DSharpObject*)stackValue.ReadReference();

            //Unboxing references to stack
            if (stackValue.ValueType == DSharpStackValueType.Reference &&
                !objectReference->IsReferenceObject &&
                context.ObjectsContainer.Unbox(objectReference, variable->Buffer))
            {
                variable->Buffer.ValueType = DSharpStackValueType.Structure;
            }
            else if (!variable->ParameterInfo->Type->IsValueType &&
                stackValue.ValueType == DSharpStackValueType.Structure)
            {
                var boxedValue = (nint)context.ObjectsContainer.Box((DSharpObject*)stackValue.StackPointer);
                variable->Buffer.ValueType = DSharpStackValueType.Reference;
                variable->Buffer.Write(boxedValue);
            }
            else
            {
                variable->Buffer.ValueType = stackValue.ValueType;
                variable->Buffer.Write(stackValue);
            }

            variable->Buffer.IsNumber = stackValue.IsNumber;

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<uint>();
            return 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<uint>();
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.StoreLocal"/> operation executor
        /// </summary>
        public static readonly DSharpStoreLocalInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
