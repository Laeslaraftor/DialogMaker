using DialogMaker.Core.Scripting.Runtime.Executor;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Method execution local variable
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DSharpExecutionLocalVariable
    {
        /// <summary>
        /// Runtime information about parameter that represents current variable
        /// </summary>
        public DSharpRuntimeParameterInfo ParameterInfo;
        /// <summary>
        /// Allocated buffer for variable
        /// </summary>
        public DSharpStack.FrameInfo Buffer;
    }
}
