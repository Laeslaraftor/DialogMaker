using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Executor of <see cref="DSharpBytecodeOperation.New"/> operation
    /// </summary>
    public class DSharpNewInstructionExecutor : DSharpMetadataTokenInstructionExecutor
    {
        #region Controls

        public override unsafe delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken)
        {
            DSharpMetadataToken* member;

            try
            {
                member = context.TypesProvider.GetMember(metadataToken);
            }
            catch (Exception error)
            {
                return context.ThrowExecutionException(error);
            }

            DSharpRuntimeTypeInfo* typeToInstantiate;
            DSharpRuntimeMethodInfo* constructor = null;

            if (member->Type == DSharpMetadataTokenType.TypeDefinition)
            {
                typeToInstantiate = (DSharpRuntimeTypeInfo*)member;
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
                return DSharpMethodExecutionCallback.InitializeObject(newInstance, constructor, default);
            }
            if (typeToInstantiate->Initializer != null)
            {
                return DSharpMethodExecutionCallback.Call(newInstance, typeToInstantiate->Initializer, new(0, 0));
            }

            return DSharpMethodExecutionCallback.Complete();
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
