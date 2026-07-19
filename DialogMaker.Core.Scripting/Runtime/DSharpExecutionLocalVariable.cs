using DialogMaker.Core.Scripting.Runtime.Executor;
using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Method execution local variable
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpExecutionLocalVariable
    {
        /// <summary>
        /// Runtime information about parameter that represents current variable
        /// </summary>
        public DSharpRuntimeParameterInfo* ParameterInfo;
        /// <summary>
        /// Allocated buffer for variable
        /// </summary>
        public DSharpStack.FrameInfo Buffer;

        #region Static

        /// <summary>
        /// Create local variable from parameter info
        /// </summary>
        /// <param name="stack">Stack for allocating memory for variable values</param>
        /// <param name="parameterInfo">Information about variable</param>
        /// <returns>Created local variable</returns>
        public static DSharpExecutionLocalVariable Create(DSharpStack stack, DSharpRuntimeParameterInfo* parameterInfo)
        {
            if (parameterInfo->Type->IsValueType)
            {
                stack.PushStructure(parameterInfo->Type);
            }
            else
            {
                stack.PushReference(IntPtr.Zero);
            }

            return new()
            {
                ParameterInfo = parameterInfo,
                Buffer = stack.Peek()
            };
        }

        #endregion
    }
}
