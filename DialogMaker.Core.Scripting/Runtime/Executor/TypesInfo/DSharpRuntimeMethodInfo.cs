using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime information about method
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeMethodInfo
    {
        /// <summary>
        /// Method metadata token
        /// </summary>
        public DSharpMetadataToken MetadataToken;
        /// <summary>
        /// Type of method
        /// </summary>
        public DSharpMethodType MethodType;
        /// <summary>
        /// Is method abstract
        /// </summary>
        public bool IsAbstract;
        /// <summary>
        /// Is method virtual
        /// </summary>
        public bool IsVirtual;
        /// <summary>
        /// Is method static
        /// </summary>
        public bool IsStatic;
        /// <summary>
        /// Is method external
        /// </summary>
        public bool IsExtern;
        /// <summary>
        /// Type that declares current method
        /// </summary>
        public DSharpRuntimeTypeInfo* DeclaringType;
        /// <summary>
        /// Type that returns by method.
        /// This can be null
        /// </summary>
        public DSharpRuntimeTypeInfo* ReturnType;
        /// <summary>
        /// Array of parameters type
        /// </summary>
        public UnmanagedArray<nint> ParametersType;
        /// <summary>
        /// Array of generic types
        /// </summary>
        public UnmanagedArray<nint> GenericTypes;
        /// <summary>
        /// Method that was overriden by current method.
        /// It can be null
        /// </summary>
        public DSharpRuntimeMethodInfo* Overrides;
        /// <summary>
        /// Method bytecode array. 
        /// It can be empty if method is extern or abstract
        /// </summary>
        public UnmanagedArray<byte> Bytecode;
        /// <summary>
        /// Pointer to parsed bytecode that ready to execute
        /// </summary>
        public DSharpRuntimeBytecode* ParsedBytecode;

        #region Static

        /// <summary>
        /// Get size that requires for structure with information about specified method
        /// </summary>
        /// <param name="method">Method to calculate size</param>
        /// <returns>Size of structure with information about specified method</returns>
        public static int GetSize(IDSharpMethodInfo method)
        {
            var parameters = method.GetParameters();
            var genericParameters = method.GetGenericParameters();
            var bytecodeSize = method.Bytecode?.Size ?? 0;

            return sizeof(DSharpRuntimeMethodInfo) +
                   bytecodeSize +
                   parameters.Length * sizeof(nint) +
                   genericParameters.Length * sizeof(nint);
        }

        #endregion
    }
}
