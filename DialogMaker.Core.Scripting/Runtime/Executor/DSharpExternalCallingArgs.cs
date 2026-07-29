using DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# external method calling arguments
    /// </summary>
    public unsafe struct DSharpExternalCallingArgs(DSharpObject* instance, 
                                                            DSharpRuntimeMethodInfo* runtimeMethodInfo,
                                                            UnmanagedArray<DSharpRuntimeTypeInfo> genericParameter,
                                                            UnmanagedArray<DSharpExecutionLocalVariable> arguments,
                                                            DSharpStack stack,
                                                            IDSharpAssembly assembly)
    {
        /// <summary>
        /// Object instance that contains called method
        /// </summary>
        public DSharpObject* Instance { get; } = instance;
        /// <summary>
        /// Runtime information about called method
        /// </summary>
        public DSharpRuntimeMethodInfo* RuntimeMethodInfo { get; } = runtimeMethodInfo;
        /// <summary>
        /// Called method generic parameters
        /// </summary>
        public UnmanagedArray<DSharpRuntimeTypeInfo> GenericParameter { get; } = genericParameter;
        /// <summary>
        /// Calling arguments
        /// </summary>
        public UnmanagedArray<DSharpExecutionLocalVariable> Arguments { get; } = arguments;
        /// <summary>
        /// Current thread stack
        /// </summary>
        public DSharpStack Stack { get; } = stack;
        /// <summary>
        /// Managed information about called method
        /// </summary>
        public IDSharpMethodInfo MethodInfo
        {
            get
            {
                field ??= (IDSharpMethodInfo)Assembly.GetType(RuntimeMethodInfo->MetadataToken);
                return field;
            }
        }
        /// <summary>
        /// Current assembly
        /// </summary>
        public IDSharpAssembly Assembly { get; } = assembly;
    }
}
