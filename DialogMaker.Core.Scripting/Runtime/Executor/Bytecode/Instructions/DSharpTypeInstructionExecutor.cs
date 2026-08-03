using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of instruction executor that have type metadata token as single argument
    /// </summary>
    public abstract class DSharpTypeInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpRuntimeTypeInfo>
    {
        #region Controls

        protected override unsafe DSharpRuntimeTypeInfo* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetRuntimeInfo(metadataToken);
        }

        protected override unsafe DSharpRuntimeTypeInfo* RuntimeInformationHandler(DSharpRuntimeInstruction instruction, ref DSharpExecutionContext context, DSharpRuntimeTypeInfo* runtimeInfo)
        {
            return context.ReplaceType(runtimeInfo);
        }

        #endregion
    }
}
