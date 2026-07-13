using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Runtime bytecode information
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeBytecode
    {
        /// <summary>
        /// Bytecode parameters
        /// </summary>
        public UnmanagedArray<DSharpRuntimeParameterInfo> Parameters;
        /// <summary>
        /// Bytecode instructions
        /// </summary>
        public UnmanagedArray<DSharpRuntimeInstruction> Instructions;

        #region Static

        /// <summary>
        /// Parse runtime bytecode from raw data
        /// </summary>
        /// <param name="typesProvider">Types provider for getting runtime type information</param>
        /// <param name="bytecode">Raw bytecode data</param>
        /// <returns>Runtime bytecode</returns>
        public static DSharpRuntimeBytecode* Parse(DSharpRuntimeTypesProvider typesProvider, UnmanagedArray<byte> bytecode)
        {
            UnmanagedStream stream = bytecode.ToStream();
            int parametersCount = stream.Read<int>();
            Span<DSharpRuntimeParameterInfo> parameters = stackalloc DSharpRuntimeParameterInfo[parametersCount];

            for (int i = 0; i < parametersCount; i++)
            {
                parameters[i] = DSharpRuntimeParameterInfo.Read(typesProvider, ref stream);
            }

            return null;
        }

        #endregion
    }
}
