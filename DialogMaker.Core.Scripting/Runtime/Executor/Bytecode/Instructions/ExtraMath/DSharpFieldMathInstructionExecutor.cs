using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all field math instruction executors that accept field metadata token as first argument and 
    /// boolean flag (push result into stack) as second argument
    /// </summary>
    public unsafe abstract class DSharpFieldMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            stream->Read<bool>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = (nint)typesProvider.GetField(stream->Read<DSharpMetadataToken>());
            arguments[1] = stream->Read<bool>() ? 1 : 0;
        }
    }
}
