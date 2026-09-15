using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of instruction executor that have property metadata token as single argument
    /// </summary>
    public unsafe abstract class DSharpPropertyInstructionExecutor : DSharpCallingInstructionExecutor<DSharpRuntimePropertyInfo>
    {
        #region Controls

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, CallingExecutionParameters parameters)
        {
            var property = parameters.Member;
            var instance = parameters.Instance;
            DSharpRuntimeMethodInfo* accessor = GetAccessor(property);

            if (instance != null)
            {
                instance = GetInstance(context, parameters.ArgumentsCount, out var error);

                if (instance == null)
                {
                    return error;
                }
                if (parameters.CallingType == DSharpMethodCallingType.Virtual)
                {
                    if (instance->Type->TryGetOverridenProperty(property, out var endPointProperty))
                    {
                        property = endPointProperty;
                        accessor = GetAccessor(endPointProperty);
                    }
                    else if (property->DeclaringType->ObjectType == DSharpObjectType.Interface ||
                             property->IsAbstract)
                    {
                        return context.ThrowExecutionException($"Unable to find end-point property for \"{property->ToString()}\"");
                    }
                }
            }

            if (accessor == null)
            {
                return context.ThrowExecutionException($"Trying to access property \"{property->ToString()}\" without implementation");
            }

            var args = DSharpMethodInstructionExecutor.CreateArguments(context, accessor, 0);

            return DSharpMethodExecutionCallback.Call(instance, accessor, args);
        }

        protected override DSharpRuntimePropertyInfo* ReplaceMember(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimePropertyInfo* member)
        {
            return context.ReplaceProperty(member);
        }
        protected override DSharpRuntimePropertyInfo* GetMember(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetProperty(metadataToken);
        }
        protected override bool IsStatic(DSharpRuntimePropertyInfo* member)
        {
            return member->IsStatic;
        }
        protected override int GetArgumentsCount(DSharpRuntimePropertyInfo* member)
        {
            var accessor = GetAccessor(member);

            if (accessor != null)
            {
                return accessor->ParametersType.Length;
            }

            return 0;
        }
        protected abstract DSharpRuntimeMethodInfo* GetAccessor(DSharpRuntimePropertyInfo* property);

        #endregion
    }
}
