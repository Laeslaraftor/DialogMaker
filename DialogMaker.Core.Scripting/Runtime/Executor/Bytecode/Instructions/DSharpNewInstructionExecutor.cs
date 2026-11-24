using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.New"/> operation
    /// </summary>
    public unsafe class DSharpNewInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpMetadataToken>
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction*, ref DSharpExecutionContext, void> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpMetadataToken* member)
        {
            DSharpRuntimeTypeInfo* typeToInstantiate;
            DSharpRuntimeMethodInfo* constructor = null;

            if (member->Type == DSharpMetadataTokenType.TypeDefinition)
            {
                typeToInstantiate = context.ReplaceType((DSharpRuntimeTypeInfo*)member);

                for (int i = 0; i < typeToInstantiate->Constructors.Length; i++)
                {
                    var typeConstructor = typeToInstantiate->Constructors.GetItemReference(i);

                    if (typeConstructor->ParametersType.Length == 0)
                    {
                        constructor = typeConstructor;
                        break;
                    }
                }
            }
            else if (member->Type == DSharpMetadataTokenType.Method)
            {
                constructor = context.ReplaceMethod((DSharpRuntimeMethodInfo*)member);
                typeToInstantiate = constructor->DeclaringType;
            }
            else
            {
                *context.Callback = context.ThrowExecutionException($"Got unexpected member for creating new instance: {member->Type}");
                return;
            }

            DSharpObject* newInstance;

            if (typeToInstantiate->IsValueType)
            {
                var frame = context.Stack.PushStructure(typeToInstantiate);
                newInstance = (DSharpObject*)frame.StackPointer;
            }
            else
            {
                newInstance = context.ObjectsContainer.Create(typeToInstantiate);
                context.Stack.PushReference(newInstance);
            }
            if (constructor != null)
            {
                UnmanagedArray<DSharpExecutionLocalVariable> args = default;

                if (constructor->ParametersType.Length > 0)
                {
                    args = DSharpCallInstructionExecutor.CreateArguments(context, constructor, 1);
                }

                *context.Callback = DSharpMethodExecutionCallback.InitializeObject(newInstance, constructor, args);
                return;
            }
            if (typeToInstantiate->Initializer != null)
            {
                *context.Callback = DSharpMethodExecutionCallback.Call(newInstance, typeToInstantiate->Initializer, default);
                return;
            }

            newInstance->IsInitialized = true;
        }

        protected override DSharpMetadataToken* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetMember(metadataToken);
        }

        protected override DSharpMetadataToken* RuntimeInformationHandler(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, DSharpMetadataToken* runtimeInfo)
        {
            if (runtimeInfo->Type == DSharpMetadataTokenType.TypeDefinition)
            {
                return (DSharpMetadataToken*)context.GetType(*runtimeInfo);
            }

            return runtimeInfo;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of <see cref="DSharpBytecodeOperation.New"/> operation executor
        /// </summary>
        public static readonly DSharpNewInstructionExecutor Instance = new();

        private static void InstanceExecute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
            Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
