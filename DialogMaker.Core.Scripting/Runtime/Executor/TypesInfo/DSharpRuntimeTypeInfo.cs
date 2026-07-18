using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime information about type
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpRuntimeTypeInfo
    {
        /// <summary>
        /// Size of item as field value.
        /// Reference types (classes and interfaces) have size same to <c>nint</c>,
        /// value types return size from <see cref="Size"/>
        /// </summary>
        public readonly int ItemSize
        {
            get
            {
                if (ObjectType == DSharpObjectType.Class ||
                    ObjectType == DSharpObjectType.Interface)
                {
                    return sizeof(nint);
                }

                return Size;
            }
        }
        /// <summary>
        /// Is value type
        /// </summary>
        public readonly bool IsValueType => ObjectType == DSharpObjectType.Struct ||
                                            ObjectType == DSharpObjectType.Enum;

        /// <summary>
        /// Type metadata token
        /// </summary>
        public DSharpMetadataToken MetadataToken;
        /// <summary>
        /// Type of object
        /// </summary>
        public DSharpObjectType ObjectType;
        /// <summary>
        /// Size of this type. It is sum of fields types size
        /// </summary>
        public int Size;
        /// <summary>
        /// Index of build-in type. If it not build-in type it's contains -1
        /// </summary>
        public int BuildInValueTypeIndex;
        /// <summary>
        /// Is type generic. Generic types used it generic parameters in methods
        /// </summary>
        public bool IsGeneric;
        /// <summary>
        /// Type name
        /// </summary>
        public UnmanagedArray<char> Name;
        /// <summary>
        /// Type namespace
        /// </summary>
        public UnmanagedArray<char> Namespace;
        /// <summary>
        /// Array of generic parameters
        /// </summary>
        public UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> GenericParameters;
        /// <summary>
        /// Types that inherited or implemented by current type
        /// </summary>
        public UnmanagedArray<Pointer<DSharpRuntimeTypeInfo>> BaseTypes;
        /// <summary>
        /// Array of constructors
        /// </summary>
        public UnmanagedArray<DSharpRuntimeMethodInfo> Constructors;
        /// <summary>
        /// Array of all methods includes inherited
        /// </summary>
        public UnmanagedArray<DSharpRuntimeMethodInfo> Methods;
        /// <summary>
        /// Array of properties includes inherited
        /// </summary>
        public UnmanagedArray<DSharpRuntimePropertyInfo> Properties;
        /// <summary>
        /// Array of fields includes inherited
        /// </summary>
        public UnmanagedArray<DSharpRuntimeFieldInfo> Fields;
        /// <summary>
        /// Reserved space for static fields values
        /// </summary>
        public UnmanagedArray<byte> StaticFieldsData;

        #region Controls

        /// <summary>
        /// Check is current type inherit from specified type
        /// </summary>
        /// <param name="type">Base type</param>
        /// <returns>Is current type inherit from specified type</returns>
        public readonly bool IsInheritFrom(DSharpRuntimeTypeInfo* type)
        {
            for (int i = 0; i < BaseTypes.Length; i++)
            {
                DSharpRuntimeTypeInfo* baseType = BaseTypes[i];

                if (baseType == type || baseType->IsInheritFrom(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to get constructor by metadata token
        /// </summary>
        /// <param name="metadataToken">Constructor metadata token</param>
        /// <param name="result">Constructor with same metadata token</param>
        /// <returns>Is constructor found</returns>
        public readonly bool TryGetConstructor(DSharpMetadataToken metadataToken, out DSharpRuntimeMethodInfo* result)
        {
            return TryGetMember(Constructors, metadataToken, out result);
        }
        /// <summary>
        /// Try to get field by metadata token
        /// </summary>
        /// <param name="metadataToken">Field metadata token</param>
        /// <param name="result">Field with same metadata token</param>
        /// <returns>Is field found</returns>
        public readonly bool TryGetField(DSharpMetadataToken metadataToken, out DSharpRuntimeFieldInfo* result)
        {
            return TryGetMember(Fields, metadataToken, out result);
        }
        /// <summary>
        /// Try to get property by metadata token
        /// </summary>
        /// <param name="metadataToken">Property metadata token</param>
        /// <param name="result">Property with same metadata token</param>
        /// <returns>Is property found</returns>
        public readonly bool TryGetProperty(DSharpMetadataToken metadataToken, out DSharpRuntimePropertyInfo* result)
        {
            return TryGetMember(Properties, metadataToken, out result);
        }
        /// <summary>
        /// Try to get method by metadata token
        /// </summary>
        /// <param name="metadataToken">Method metadata token</param>
        /// <param name="result">Method with same metadata token</param>
        /// <returns>Is method found</returns>
        public readonly bool TryGetMethod(DSharpMetadataToken metadataToken, out DSharpRuntimeMethodInfo* result)
        {
            return TryGetMember(Methods, metadataToken, out result);
        }

        public readonly override string ToString()
        {
            if (Name.Length == 0)
            {
                return string.Empty;
            }

            string name = new(Name);

            if (Namespace.Length > 0)
            {
                name = new string(Namespace) + "." + name;
            }

            return name;
        }

        private readonly bool TryGetMember<T>(UnmanagedArray<T> members, DSharpMetadataToken metadataToken, out T* result)
            where T : unmanaged
        {
            for (int i = 0; i < members.Length; i++)
            {
                var member = members.GetItemReference(i);

                if (*(DSharpMetadataToken*)member == metadataToken)
                {
                    result = member;
                    return true;
                }
            }

            result = default;
            return false;
        }

        #endregion
    }
}
