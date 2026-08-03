using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode.Instructions
{
    /// <summary>
    /// Base implementation of instruction executor that have method metadata token as single argument
    /// </summary>
    public abstract class DSharpMethodInstructionExecutor : DSharpMetadataTokenInstructionExecutor<DSharpRuntimeMethodInfo>
    {
        #region Controls

        protected override unsafe DSharpRuntimeMethodInfo* GetRuntimeInformation(DSharpRuntimeInformationProvider typesProvider, DSharpMetadataToken metadataToken)
        {
            return typesProvider.GetMethod(metadataToken);
        }

        #endregion
    }
}
