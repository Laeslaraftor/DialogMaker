using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all math instruction between variable and field
    /// </summary>
    public abstract unsafe class DSharpVariableFieldMathInstructionExecutor : DSharpVariableInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            base.GetArgumentsCount(typesProvider, stream);
            stream->Read<DSharpMetadataToken>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            base.ReadArguments(typesProvider, stream, arguments);
            arguments[1] = (nint)typesProvider.GetField(stream->Read<DSharpMetadataToken>());
        }
    }
}
