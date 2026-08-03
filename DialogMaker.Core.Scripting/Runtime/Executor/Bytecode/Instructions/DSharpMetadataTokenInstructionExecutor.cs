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
        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }

            var runtimeInfo = (T*)instruction.Arguments[0];
            runtimeInfo = RuntimeInformationHandler(instruction, ref context, runtimeInfo);

            return Execute(instruction, ref context, runtimeInfo);
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
        protected abstract unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, T* runtimeInfo);
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
        protected virtual unsafe T* RuntimeInformationHandler(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, T* runtimeInfo)
        {
            return runtimeInfo;
        }

        #region Static

        internal static unsafe DSharpMethodExecutionCallback CallAccessor(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimePropertyInfo* property, DSharpPropertyAccessor accessorType, bool isInstance, bool isBase)
        {
            DSharpObject* instance = null;
            DSharpRuntimeMethodInfo* accessor = GetAccessor(property, accessorType);

            uint parametersOffset = 0;

            if (isInstance)
            {
                parametersOffset = 1;
                instance = GetInstance(context, 0, out var error);

                if (instance == null)
                {
                    return error;
                }
                if (!isBase && property->CanBeOverriden)
                {
                    if (instance->Type->OverridenProperties.TryGetValue(property, out var endPointProperty))
                    {
                        property = endPointProperty;
                        accessor = GetAccessor(endPointProperty, accessorType);
                    }
                    else if (property->DeclaringType->ObjectType == DSharpObjectType.Interface ||
                             property->IsAbstract)
                    {
                        return context.ThrowExecutionException($"Unable to find end-point method for \"{property->ToString()}\"");
                    }
                }
            }
            else if (accessor == null)
            {
                return context.ThrowExecutionException($"Unable to get value from property \"{property->ToString()}\" because it have not getter");
            }

            var args = DSharpCallInstructionExecutor.CreateArguments(context, accessor, parametersOffset);

            return DSharpMethodExecutionCallback.Call(instance, accessor, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static DSharpRuntimeMethodInfo* GetAccessor(DSharpRuntimePropertyInfo* property, DSharpPropertyAccessor accessorType)
        {
            return accessorType == DSharpPropertyAccessor.Getter ? property->Getter : property->Setter;
        }

        #endregion
    }
}
