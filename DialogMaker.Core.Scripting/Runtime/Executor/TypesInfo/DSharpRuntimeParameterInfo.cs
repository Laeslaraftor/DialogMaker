using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
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
        /// Read runtime parameter information from unmanaged stream
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding parameter type by metadata token</param>
        /// <param name="stream">Unmanaged stream for reading parameter information</param>
        /// <returns>Runtime parameter information from stream</returns>
        public static DSharpRuntimeParameterInfo Read(DSharpRuntimeInformationProvider typesProvider, UnmanagedStream* stream)
        {
            var nameLength = stream->Read<int>();
            var name = stream->ReadArray<char>(nameLength);
            var typeToken = stream->Read<DSharpMetadataToken>();
            var mode = stream->Read<DSharpMethodParameterMode>();

            return new()
            {
                Name = name,
                Type = typesProvider.GetRuntimeInfo(typeToken),
                Mode = mode
            };
        }
        /// <summary>
        /// Create runtime parameter info from parameter information
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding runtime parameter type</param>
        /// <param name="parameter">Parameter for creating runtime information</param>
        /// <param name="builder">Memory builder for allocating memory for parameter name</param>
        /// <returns>Runtime parameter information from stream</returns>
        public static DSharpRuntimeParameterInfo Create(DSharpRuntimeInformationProvider typesProvider, IDSharpParameterInfo parameter, ref MemoryBuilder builder)
        {
            return new()
            {
                Name = builder.AllocateString(parameter.Name),
                Type = typesProvider.GetRuntimeInfo(parameter.Type),
                Mode = parameter.Mode
            };
        }
        /// <summary>
        /// Create unmanaged array of runtime parameter information
        /// </summary>
        /// <param name="typesProvider">Runtime types provider for finding runtime parameter type</param>
        /// <param name="parameters">Array of parameters for creating runtime information</param>
        /// <param name="builder">Memory builder for allocating memory for parameter name</param>
        /// <returns>Unmanaged array of runtime parameter information</returns>
        public static UnmanagedArray<DSharpRuntimeParameterInfo> Create(DSharpRuntimeInformationProvider typesProvider, IDSharpParameterInfo[] parameters, ref MemoryBuilder builder)
        {
            UnmanagedArray<DSharpRuntimeParameterInfo> result = builder.AllocateArray<DSharpRuntimeParameterInfo>(parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                result[i] = Create(typesProvider, parameters[i], ref builder);
            }

            return result;
        }

        #endregion
    }
}
