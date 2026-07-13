using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.Bytecode
{
    /// <summary>
    /// Runtime information about parameter
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeParameterInfo
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public UnmanagedArray<char> Name;
        /// <summary>
        /// Type of parameter
        /// </summary>
        public DSharpRuntimeTypeInfo* Type;
        /// <summary>
        /// Parameter mode
        /// </summary>
        public DSharpMethodParameterMode Mode;

        #region Static

        /// <summary>
        /// Read parameter info from unmanaged stream
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <returns>Parameter information from stream</returns>
        public static DSharpRuntimeParameterInfo Read(DSharpRuntimeTypesProvider typesProvider, ref UnmanagedStream stream)
        {
            var nameLength = stream.Read<int>();
            var name = stream.ReadArray<char>(nameLength);
            var typeToken = stream.Read<DSharpMetadataToken>();
            var mode = stream.Read<DSharpMethodParameterMode>();

            return new()
            {
                Name = name,
                Type = typesProvider.GetRuntimeInfo(typeToken),
                Mode = mode
            };
        }

        #endregion
    }
}
