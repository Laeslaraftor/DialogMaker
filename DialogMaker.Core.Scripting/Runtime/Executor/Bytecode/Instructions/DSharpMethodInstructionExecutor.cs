using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of instruction executor that have method metadata token as single argument
    /// </summary>
    public unsafe class DSharpMethodInstructionExecutor : DSharpCallingInstructionExecutor<DSharpRuntimeMethodInfo>
    {
        #region Controls

        public override delegate*<DSharpRuntimeInstruction, ref DSharpExecutionContext, DSharpMethodExecutionCallback> GetExecutorPointer()
        {
            return &InstanceExecute;
        }

        protected override DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, CallingExecutionParameters parameters)
        {
            return Call(instruction, ref context, parameters);
        }

        protected override DSharpRuntimeMethodInfo* ReplaceMember(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* member)
        {
            return context.ReplaceMethod(member);
        }
        protected override DSharpRuntimeMethodInfo* GetMember(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetMethod(metadataToken);
        }

        protected override int GetArgumentsCount(DSharpRuntimeMethodInfo* member)
        {
            return member->ParametersType.Length;
        }
        protected override bool IsStatic(DSharpRuntimeMethodInfo* member)
        {
            return member->IsStatic;
        }

        #endregion

        #region Static

        /// <summary>
        /// Global instance of method instructions executor
        /// </summary>
        public static readonly DSharpMethodInstructionExecutor Instance = new();

        internal static DSharpMethodExecutionCallback Call(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, CallingExecutionParameters parameters, uint extraScopeOffset = 0)
        {
            var instance = parameters.Instance;
            var method = parameters.Member;

            if (instance != null &&
                parameters.CallingType == DSharpMethodCallingType.Virtual)
            {
                if (instance->Type->TryGetOverridenMethod(method, out var endPointMethod))
                {
                    method = endPointMethod;
                }
                else if (method->DeclaringType->ObjectType == DSharpObjectType.Interface ||
                         method->IsAbstract)
                {
                    var methods = instance->Type->Methods;
                    return context.ThrowExecutionException($"Unable to find end-point method for \"{method->ToString()}\"");
                }
            }

            var argumentsInfo = CreateArguments(context, method, parameters.GenericParameters, 0);

            if (!method->IsExtern && method->Bytecode.IsNull)
            {
                return context.ThrowExecutionException($"Trying to call method \"{method->ToString()}\" without implementation");
            }

            return DSharpMethodExecutionCallback.Call(instance, method, argumentsInfo.GenericParameters, argumentsInfo.Arguments, extraScopeOffset);
        }

        private static DSharpMethodExecutionCallback InstanceExecute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            return Instance.Execute(instruction, ref context);
        }

        #endregion

    }
}
