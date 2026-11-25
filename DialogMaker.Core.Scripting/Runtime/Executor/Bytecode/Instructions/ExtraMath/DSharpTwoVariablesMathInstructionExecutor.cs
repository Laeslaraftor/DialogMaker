using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all math operations between two variables
    /// </summary>
    public abstract unsafe class DSharpTwoVariablesMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<uint>();
            stream->Read<uint>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = (int)stream->Read<uint>();
            arguments[1] = (int)stream->Read<uint>();
        }
    }
}
