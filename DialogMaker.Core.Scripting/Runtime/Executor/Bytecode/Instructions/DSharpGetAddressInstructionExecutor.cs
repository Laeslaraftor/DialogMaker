using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.GetAddress"/> operation
    /// </summary>
    public unsafe class DSharpGetAddressInstructionExecutor : DSharpInstructionExecutor
    {
        #region Controls

        public override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            InstanceExecute(instruction, ref context);
        }

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
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
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (CheckArguments(instruction, ref context, 2))
            {
                return;
            }
#endif

            var member = (DSharpGetAddressMember)instruction->Arguments[0];
            nint valueAddress;

            if (member == DSharpGetAddressMember.Variable)
            {
                var index = (int)instruction->Arguments[1];
                var variable = context.LocalVariables->GetItemReference(index);

                if (variable->ParameterInfo.Type->IsValueType)
                {
                    var instance = variable->Buffer.ReadAsObject();

                    if (instance == null)
                    {
                        valueAddress = variable->Buffer.StackPointer + sizeof(DSharpObject);
                    }
                    else
                    {
                        valueAddress = (nint)DSharpObject.GetData(instance);
                    }
                }
                else
                {
                    valueAddress = variable->Buffer.StackPointer;
                }
            }
            else
            {
                var field = (DSharpRuntimeFieldInfo*)instruction->Arguments[0];

                if (field->IsStatic)
                {
                    valueAddress = (nint)field->GetDataPointer(null);
                }
                else
                {
                    var instance = GetInstance(ref context, 0, throwOnError: true);

                    if (instance == null)
                    {
                        return;
                    }

                    valueAddress = (nint)field->GetDataPointer(instance);
                }
            }

            context.Stack.PushStructure(context.TypesProvider.IntPtr, new UnmanagedArray<byte>((byte*)&valueAddress, sizeof(nint)));
        }

        #endregion
    }
}
