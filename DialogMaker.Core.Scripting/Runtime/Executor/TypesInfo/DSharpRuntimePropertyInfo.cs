using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime information about property
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimePropertyInfo
    {
        /// <summary>
        /// Returns <c>true</c> when current method can be overriden
        /// </summary>
        public bool CanBeOverriden => DeclaringType->ObjectType == DSharpObjectType.Interface ||
                                      (!IsSealed && (IsAbstract || IsVirtual));

        /// <summary>
        /// Property metadata token
        /// </summary>
        public DSharpMetadataToken MetadataToken;
        /// <summary>
        /// Property name
        /// </summary>
        public UnmanagedArray<char> Name;
        /// <summary>
        /// Is property abstract
        /// </summary>
        public bool IsAbstract;
        /// <summary>
        /// Is property virtual
        /// </summary>
        public bool IsVirtual;
        /// <summary>
        /// Is property static
        /// </summary>
        public bool IsStatic;
        /// <summary>
        /// Is property sealed
        /// </summary>
        public bool IsSealed;
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
        /// <summary>
        /// Interfaces declarations that implemented by current property
        /// </summary>
        public UnmanagedArray<Pointer<DSharpRuntimePropertyInfo>> ImplementedProperties;

        public readonly override string ToString()
        {
            if (Name.Length == 0)
            {
                return "Nameless field";
            }

            return new((ReadOnlySpan<char>)Name);
        }

        #region Static

        /// <summary>
        /// Get size that requires for structure with information about specified property
        /// </summary>
        /// <param name="property">Property to calculate size</param>
        /// <returns>Size of structure with information about specified property</returns>
        public static int GetSize(IDSharpPropertyInfo property)
        {
            var implementations = property.GetImplementedProperties();

            return sizeof(DSharpRuntimePropertyInfo) +
                   property.Name.Length * sizeof(char) +
                   implementations.Length * sizeof(Pointer<DSharpRuntimePropertyInfo>);
        }

        #endregion
    }
}
