using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all variable math instruction executors that accept variable index as first argument and 
    /// boolean flag (push result into stack) as second argument
    /// </summary>
    public unsafe abstract class DSharpVariableMathInstructionExecutor : DSharpVariableInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            base.GetArgumentsCount(typesProvider, stream);
            stream->Read<bool>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            base.ReadArguments(typesProvider, stream, arguments);
            arguments[1] = stream->Read<bool>() ? 1 : 0;
        }
    }
}
