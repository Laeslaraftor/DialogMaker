using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of instruction executor that have property metadata token as single argument
    /// </summary>
    public abstract class DSharpPropertyInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpRuntimePropertyInfo>
    {
        #region Controls

        protected override unsafe DSharpRuntimePropertyInfo* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetProperty(metadataToken);
        }

        #endregion
    }
}
