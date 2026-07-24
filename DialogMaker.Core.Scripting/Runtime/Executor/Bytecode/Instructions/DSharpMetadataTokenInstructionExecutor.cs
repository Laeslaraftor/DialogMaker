using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base executor for instruction that requires metadata token in arguments
    /// </summary>
    public abstract class DSharpMetadataTokenInstructionExecutor : DSharpInstructionExecutor
    {
        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }

            var metadataToken = *(DSharpMetadataToken*)instruction.Arguments[0];

            return Execute(instruction, ref context, metadataToken);
        }

        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            return 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<DSharpMetadataToken>();
        }

        /// <summary>
        /// Execute instruction with metadata token as single parameter
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <param name="metadataToken">Metadata token from instruction arguments</param>
        /// <returns>Is successfully executed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken);

        #region Static

        internal static unsafe DSharpMethodExecutionCallback CallAccessor(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken, DSharpPropertyAccessor accessorType, bool isInstance, bool isBase)
        {
            DSharpObject* instance = null;
            DSharpRuntimePropertyInfo* property;

            try
            {
                property = context.TypesProvider.GetProperty(metadataToken);
            }
            catch (Exception exception)
            {
                return context.ThrowExecutionException(exception);
            }

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
