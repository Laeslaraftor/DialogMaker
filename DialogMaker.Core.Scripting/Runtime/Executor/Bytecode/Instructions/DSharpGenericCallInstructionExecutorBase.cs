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
            var methodToken = *(DSharpMetadataToken*)instruction.Arguments[0];
            UnmanagedArray<Pointer<DSharpMetadataToken>> genericParameters;

            if (instruction.Arguments.Length > 1)
            {
                genericParameters = instruction.Arguments.Slice(1).Cast<Pointer<DSharpMetadataToken>>();
            }
            else
            {
                genericParameters = default;
            }

            return Execute(instruction, ref context, methodToken, genericParameters);
        }

        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            var methodToken = stream->Read<DSharpMetadataToken>();
            var method = typesProvider.GetMethod(methodToken);
            int replacesCount = stream->Read<int>() * 2;

            for (int i = 0; i < replacesCount; i++)
            {
                stream->Read<DSharpMetadataToken>();
            }

            return replacesCount + 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var methodToken = stream->ReadSafePointer<DSharpMetadataToken>();
            var replacesCount = stream->Read<int>() * 2;
            var method = typesProvider.GetMethod(*(DSharpMetadataToken*)methodToken);
            arguments[0] = methodToken;

            for (int i = 1; i < replacesCount + 1; i++)
            {
                arguments[i] = stream->ReadSafePointer<DSharpMetadataToken>();
            }
        }

        protected abstract DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken methodToken, UnmanagedArray<Pointer<DSharpMetadataToken>> genericParameters);
    }
}
