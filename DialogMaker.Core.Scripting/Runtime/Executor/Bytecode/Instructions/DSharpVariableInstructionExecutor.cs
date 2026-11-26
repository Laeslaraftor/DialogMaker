using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all instruction executors that accepts variable index as first argument
    /// </summary>
    public unsafe abstract class DSharpVariableInstructionExecutor : DSharpInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<uint>();
            return 1;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = (int)stream->Read<uint>();
        }
    }
}
