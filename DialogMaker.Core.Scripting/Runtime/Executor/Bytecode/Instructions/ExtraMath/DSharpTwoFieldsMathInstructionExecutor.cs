using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base class of all math operations between two fields
    /// </summary>
    public abstract unsafe class DSharpTwoFieldsMathInstructionExecutor : DSharpInstructionExecutor
    {
        public override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            stream->Read<DSharpMetadataToken>();
            return 2;
        }
        public override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            var leftFieldToken = stream->Read<DSharpMetadataToken>();
            var rightFieldToken = stream->Read<DSharpMetadataToken>();

            arguments[0] = (nint)typesProvider.GetField(leftFieldToken);
            arguments[1] = (nint)typesProvider.GetField(rightFieldToken);
        }
    }
}
