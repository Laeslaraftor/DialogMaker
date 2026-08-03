using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.New"/> operation
    /// </summary>
    public class DSharpNewInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpMetadataToken>
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken* member)
        {
            DSharpRuntimeTypeInfo* typeToInstantiate;
            DSharpRuntimeMethodInfo* constructor = null;

            if (member->Type == DSharpMetadataTokenType.TypeDefinition)
            {
                typeToInstantiate = (DSharpRuntimeTypeInfo*)member;

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
                constructor = (DSharpRuntimeMethodInfo*)member;
                typeToInstantiate = constructor->DeclaringType;
            }
            else
            {
                return context.ThrowExecutionException($"Got unexpected member for creating new instance: {member->Type}");
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

                return DSharpMethodExecutionCallback.InitializeObject(newInstance, constructor, args);
            }
            if (typeToInstantiate->Initializer != null)
            {
                return DSharpMethodExecutionCallback.Call(newInstance, typeToInstantiate->Initializer, default);
            }

            newInstance->IsInitialized = true;

            return DSharpMethodExecutionCallback.Complete();
        }

        protected override unsafe DSharpMetadataToken* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetMember(metadataToken);
        }

        protected override unsafe DSharpMetadataToken* RuntimeInformationHandler(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken* runtimeInfo)
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

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion
    }
}
