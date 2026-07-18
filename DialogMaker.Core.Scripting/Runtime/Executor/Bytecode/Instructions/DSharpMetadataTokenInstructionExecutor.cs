using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base executor for instruction that requires metadata token in arguments
    /// </summary>
    public abstract class DSharpMetadataTokenInstructionExecutor : DSharpInstructionExecutor
    {
        public override unsafe DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context)
        {
            if (CheckArguments(instruction, context, 1, out var error))
            {
                return error;
            }

            var metadataToken = *(DSharpMetadataToken*)instruction.Arguments[0];

            return Execute(instruction, ref context, metadataToken);
        }

        public unsafe override int GetArgumentsCount(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            stream->Read<DSharpMetadataToken>();
            return 1;
        }
        public unsafe override void ReadArguments(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream, UnmanagedArray<nint> arguments)
        {
            arguments[0] = stream->ReadSafePointer<DSharpMetadataToken>();
        }

        /// <summary>
        /// Execute instruction with metadata token as single parameter
        /// </summary>
        /// <param name="instruction">Executing instruction information</param>
        /// <param name="context">Execution context</param>
        /// <param name="metadataToken">Metadata token from instruction arguments</param>
        /// <returns>Is successfully executed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract DSharpMethodExecutionCallback Execute(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpMetadataToken metadataToken);
    }
}
