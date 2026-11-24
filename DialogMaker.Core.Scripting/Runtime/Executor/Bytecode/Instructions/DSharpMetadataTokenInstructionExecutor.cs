using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base executor for instruction that requires metadata token in arguments
    /// </summary>
    public abstract class DSharpMetadataTokenInstructionExecutor<T> : DSharpInstructionExecutor
        where T : unmanaged
    {
        public override unsafe void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context)
        {
#if DEBUG
            if (1 > instruction->Arguments.Length)
            {
                *context.Callback = context.ThrowExecutionException($"{instruction->Operation} instruction requires at least 1 argument, got {instruction->Arguments.Length}");
                return;
            }
#endif

            var runtimeInfo = (T*)instruction->Arguments[0];
            runtimeInfo = RuntimeInformationHandler(instruction, ref context, runtimeInfo);

            Execute(instruction, ref context, runtimeInfo);
        }

        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            return 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var metadataToken = stream->Read<DSharpMetadataToken>();
            arguments[0] = (nint)GetRuntimeInformation(typesProvider, metadataToken);
        }

        /// <summary>
        /// Execute instruction with metadata token as single parameter
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <param name="runtimeInfo">Runtime information with metadata token from instruction arguments</param>
        /// <returns>Is successfully executed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract unsafe void Execute(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, T* runtimeInfo);
        /// <summary>
        /// Get runtime information with specified metadata token
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="metadataToken">Metadata token for getting runtime information</param>
        /// <returns>Runtime information with specified metadata token</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract unsafe T* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken);

        /// <summary>
        /// Runtime information handler
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <param name="runtimeInfo"></param>
        /// <returns>New runtime information</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual unsafe T* RuntimeInformationHandler(DSharpRuntimeInstruction* instruction, ref DSharpExecutionContext context, T* runtimeInfo)
        {
            return runtimeInfo;
        }

        #region Static

        internal static unsafe void CallAccessor(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimePropertyInfo* property, DSharpPropertyAccessor accessorType, bool isInstance, bool isBase)
        {
            property = context.ReplaceProperty(property);
            DSharpObject* instance = null;
            DSharpRuntimeMethodInfo* accessor = GetAccessor(property, accessorType);
            uint parametersCount = property->GetterParametersCount;

            if (accessorType == DSharpPropertyAccessor.Setter)
            {
                parametersCount = property->SetterParametersCount;
            }

            if (isInstance)
            {
                instance = GetInstance(ref context, parametersCount, throwOnError: true);

                if (instance == null)
                {
                    return;
                }
                if (!isBase && property->CanBeOverriden)
                {
                    if (instance->Type->TryGetOverridenProperty(property, out var endPointProperty))
                    {
                        property = endPointProperty;
                        accessor = GetAccessor(endPointProperty, accessorType);
                    }
                    else if (property->DeclaringType->ObjectType == DSharpObjectType.Interface ||
                             property->IsAbstract)
                    {
                        *context.Callback = context.ThrowExecutionException($"Unable to find end-point property for \"{property->ToString()}\"");
                        return;
                    }
                }
            }

            if (accessor == null)
            {
                *context.Callback = context.ThrowExecutionException($"Unable to get value from property \"{property->ToString()}\" because it have not getter");
                return;
            }

            var args = DSharpCallInstructionExecutor.CreateArguments(context, accessor, 0);

            *context.Callback = DSharpMethodExecutionCallback.Call(instance, accessor, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static DSharpRuntimeMethodInfo* GetAccessor(DSharpRuntimePropertyInfo* property, DSharpPropertyAccessor accessorType)
        {
            return accessorType == DSharpPropertyAccessor.Getter ? property->Getter : property->Setter;
        }

        #endregion
    }
}
