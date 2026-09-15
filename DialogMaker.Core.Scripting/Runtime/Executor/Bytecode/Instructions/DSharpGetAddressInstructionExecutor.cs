using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.GetAddress"/> operation
    /// </summary>
    public class DSharpGetAddressInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public unsafe override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 2, out var error))
            {
                return error;
            }

            var member = (DSharpGetAddressMember)instruction.Arguments[0];

            if (member == DSharpGetAddressMember.Variable)
            {
                var index = (int)instruction.Arguments[1];
                var variable = context.LocalVariables->GetItemReference(index);

                if (variable->ParameterInfo.Type->IsValueType)
                {
                    if (variable->Buffer.ValueType == DSharpStackValueType.Reference)
                    {
                        var obj = variable->Buffer.ReadAsObject();
                        context.Stack.PushReference(obj, true, true);
                    }
                    else
                    {
                        context.Stack.PushReference(variable->Buffer.StackPointer, true);
                    }
                }
                else
                {
                    var field = (DSharpRuntimeFieldInfo*)instruction.Arguments[0];
                    void* fieldPointer;

                    if (field->IsStatic)
                    {
                        fieldPointer = field->GetDataPointer(null);
                    }
                    else
                    {
                        var instance = GetInstance(context, 0, out error);

                        if (instance == null)
                        {
                            return error;
                        }

                        fieldPointer = field->GetDataPointer(instance);
                    }

                    context.Stack.PushReference((nint)fieldPointer, true);
                }
            }

            return DSharpMethodExecutionCallback.Complete();
        }

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            var member = stream->Read<DSharpGetAddressMember>();

            if (member == DSharpGetAddressMember.Variable)
            {
                stream->Read<int>();
            }
            else if (member == DSharpGetAddressMember.Field)
            {
                stream->Read<DSharpMetadataToken>();
            }

            return 2;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var member = stream->Read<DSharpGetAddressMember>();
            arguments[0] = (int)member;

            if (member == DSharpGetAddressMember.Variable)
            {
                arguments[1] = stream->Read<int>();
            }
            else if (member == DSharpGetAddressMember.Field)
            {
                var fieldToken = stream->Read<DSharpMetadataToken>();
                arguments[1] = (nint)typesProvider.GetField(fieldToken);
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.GetAddress"/> operation executor
        /// </summary>
        public static readonly DSharpGetAddressInstructionExecutor Instance = new();

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
