using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all math instruction between field and variable
    /// </summary>
    public abstract unsafe class DSharpFieldVariableMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            stream->Read<uint>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = (nint)typesProvider.GetField(stream->Read<DSharpMetadataToken>());
            arguments[1] = (int)stream->Read<uint>();
        }
    }
}
