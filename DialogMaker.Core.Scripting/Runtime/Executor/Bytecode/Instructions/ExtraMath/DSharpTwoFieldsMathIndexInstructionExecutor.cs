using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all math operations between fields instruction executors with index
    /// </summary>
    public abstract class DSharpTwoFieldsMathIndexInstructionExecutor : DSharpTwoFieldsMathInstructionExecutor
    {
        public override unsafe int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            base.GetArgumentsCount(typesProvider, stream);
            stream->Read<int>();
            return 3;
        }
        public override unsafe void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            base.ReadArguments(typesProvider, stream, arguments);
            arguments[2] = stream->Read<int>();
        }
    }
}
