using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of instruction executor that have field metadata token as single argument
    /// </summary>
    public abstract class DSharpFieldInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpRuntimeFieldInfo>
    {
        #region Controls

        protected override unsafe DSharpRuntimeFieldInfo* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetField(metadataToken);
        }

        #endregion
    }
}
