using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime information about property
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimePropertyInfo
    {
        /// <summary>
        /// Property metadata token
        /// </summary>
        public DSharpMetadataToken MetadataToken;
        /// <summary>
        /// Is property static
        /// </summary>
        public bool IsStatic;
        /// <summary>
        /// Type that declares current property
        /// </summary>
        public DSharpRuntimeTypeInfo* DeclaringType;
        /// <summary>
        /// Type of value that contains in property
        /// </summary>
        public DSharpRuntimeTypeInfo* PropertyType;
        /// <summary>
        /// Getter method of current property.
        /// It can be null
        /// </summary>
        public DSharpRuntimeMethodInfo* Getter;
        /// <summary>
        /// Setter method of current property.
        /// It can be null
        /// </summary>
        public DSharpRuntimeMethodInfo* Setter;
        /// <summary>
        /// Property that overriden by current property.
        /// It can be null
        /// </summary>
        public DSharpRuntimePropertyInfo* Overrides;
    }
}
