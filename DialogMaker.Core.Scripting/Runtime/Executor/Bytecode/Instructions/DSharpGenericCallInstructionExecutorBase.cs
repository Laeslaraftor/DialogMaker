using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of generic call instructions
    /// </summary>
    public abstract class DSharpGenericCallInstructionExecutorBase : DSharpInstructionExecutor
    {
        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            var methodInfo = (DSharpRuntimeMethodInfo*)instruction.Arguments[0];
            UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> genericParameters;

            if (instruction.Arguments.Length > 1)
            {
                genericParameters = instruction.Arguments.Slice(1).Cast<Pointer<DSharpRuntimeTypeInfo>>();
            }
            else
            {
                genericParameters = default;
            }

            return Execute(instruction, ref context, methodInfo, genericParameters);
        }

        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            int replacesCount = stream->Read<int>() * 2;

            for (int i = 0; i < replacesCount; i++)
            {
                stream->Read<DSharpMetadataToken>();
            }

            return replacesCount + 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var methodToken = stream->Read<DSharpMetadataToken>();
            var replacesCount = stream->Read<int>() * 2;
            arguments[0] = (nint)typesProvider.GetMethod(methodToken);

            for (int i = 1; i < replacesCount + 1; i++)
            {
                var typeToken = stream->Read<DSharpMetadataToken>();
                arguments[i] = (nint)typesProvider.GetRuntimeInfo(typeToken);
            }
        }

        protected unsafe abstract DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeMethodInfo* methodToken, UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> genericParameters);
    }
}
