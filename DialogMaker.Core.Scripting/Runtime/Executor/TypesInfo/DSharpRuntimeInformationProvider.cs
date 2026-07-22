using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;
using System.Reflection;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime types provides. It builds and stores runtime information about D# types
    /// </summary>
    /// <param name="assembly">Assembly that contains providing types information</param>
    /// <param name="memoryManager">Memory manager for allocating memory for runtime information</param>
    public unsafe class DSharpRuntimeInformationProvider(IDSharpAssembly assembly, DSharpVmMemoryManager memoryManager) : Disposable
    {
        /// <summary>
        /// Assembly that contains providing types information
        /// </summary>
        public IDSharpAssembly Assembly { get; } = assembly;
        public DSharpRuntimeTypeInfo* Object
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Object);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* String
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.String);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Byte
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Byte);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* SByte
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.SignedByte);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Int16
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Short);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* UInt16
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.UnsignedShort);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Int32
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Int);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* UInt32
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.UnsignedInt);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Int64
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Long);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* UInt64
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.UnsignedLong);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Single
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Single);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Double
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Double);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Decimal
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Decimal);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* IntPtr
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.NativeInt);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* UIntPtr
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.NativeUnsignedInt);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Char
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Char);
                }

                return field;
            }
        }
        public DSharpRuntimeTypeInfo* Boolean
        {
            get
            {
                if (field == null)
                {
                    field = GetRuntimeInfo(DSharpBuildInTypes.Boolean);
                }

                return field;
            }
        }

        // DSharpRuntimeTypeInfo**
        private readonly Dictionary<DSharpMetadataToken, Pointer<Pointer<DSharpRuntimeTypeInfo>>> _types = [];
        private readonly Dictionary<DSharpMetadataToken, Pointer<DSharpRuntimeMethodInfo>> _methods = [];
        private readonly Dictionary<DSharpMetadataToken, Pointer<DSharpRuntimePropertyInfo>> _properties = [];
        private readonly Dictionary<DSharpMetadataToken, Pointer<DSharpRuntimeFieldInfo>> _fields = [];
        private readonly DSharpVmMemoryManager _memoryManager = memoryManager;

        #region Controls

        /// <summary>
        /// Get runtime information about specified type
        /// </summary>
        /// <param name="type">Type for getting runtime information</param>
        /// <returns>Runtime information about specified type</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public DSharpRuntimeTypeInfo* GetRuntimeInfo(IDSharpType type)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DSharpRuntimeInformationProvider));
            }
            if (!_types.TryGetValue(type.MetadataToken, out var typeReference))
            {
                var pointer = (DSharpRuntimeTypeInfo**)_memoryManager.Allocate(DSharpMemoryBlockType.TypeInformation, sizeof(nint));
                typeReference = pointer;
                _types.Add(type.MetadataToken, typeReference);

                *pointer = CreateTypeInfo(type, pointer);
            }

            return *typeReference.AsPointer();
        }
        /// <summary>
        /// Get runtime information about specified type
        /// </summary>
        /// <param name="metadataToken">Metadata token of type for getting runtime information</param>
        /// <returns>Runtime information about specified type</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public DSharpRuntimeTypeInfo* GetRuntimeInfo(DSharpMetadataToken metadataToken)
        {
            var type = (IDSharpType)Assembly.GetType(metadataToken);
            return GetRuntimeInfo(type);
        }
        /// <summary>
        /// Get runtime information about specified type
        /// </summary>
        /// <param name="buildInTypeInfo">Information about build-in type</param>
        /// <returns>Runtime information about specified type</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public DSharpRuntimeTypeInfo* GetRuntimeInfo(DSharpBuildInTypeInfo buildInTypeInfo)
        {
            var type = Assembly.GetType(buildInTypeInfo);
            return GetRuntimeInfo(type);
        }
        /// <summary>
        /// Get runtime bytecode for specified method
        /// </summary>
        /// <param name="methodInfo">Method for finding and parsing bytecode</param>
        /// <returns>Runtime bytecode</returns>
        /// <exception cref="InvalidOperationException">Unable to get bytecode for extern method</exception>
        public DSharpRuntimeBytecode* GetRuntimeBytecode(DSharpRuntimeMethodInfo* methodInfo)
        {
            if (methodInfo->ParsedBytecode != null)
            {
                return methodInfo->ParsedBytecode;
            }
            if (methodInfo->IsExtern)
            {
                throw new InvalidOperationException("Unable to get bytecode for extern method");
            }

            methodInfo->ParsedBytecode = DSharpRuntimeBytecode.Parse(_memoryManager, this, methodInfo->Bytecode);

            return methodInfo->ParsedBytecode;
        }
        public DSharpRuntimeMethodInfo* GetMethod(DSharpMetadataToken metadataToken)
        {
            return GetMember(metadataToken, _methods, t => t.AsPointer()->Methods);
        }
        public DSharpRuntimePropertyInfo* GetProperty(DSharpMetadataToken metadataToken)
        {
            return GetMember(metadataToken, _properties, t => t.AsPointer()->Properties);
        }
        public DSharpRuntimeFieldInfo* GetField(DSharpMetadataToken metadataToken)
        {
            return GetMember(metadataToken, _fields, t => t.AsPointer()->Fields);
        }
        public DSharpMetadataToken* GetMember(DSharpMetadataToken metadataToken)
        {
            if (metadataToken.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                return (DSharpMetadataToken*)GetRuntimeInfo(metadataToken);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Method)
            {
                return (DSharpMetadataToken*)GetMethod(metadataToken);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Property)
            {
                return (DSharpMetadataToken*)GetProperty(metadataToken);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Field)
            {
                return (DSharpMetadataToken*)GetField(metadataToken);
            }

            throw new ArgumentException($"Invalid metadata token: {metadataToken}");
        }

        private T* GetMember<T>(DSharpMetadataToken metadataToken, Dictionary<DSharpMetadataToken, Pointer<T>> members, Func<Pointer<DSharpRuntimeTypeInfo>, UnmanagedArray<T>> membersSelector)
            where T : unmanaged
        {
            if (members.TryGetValue(metadataToken, out var memberPointer))
            {
                return memberPointer;
            }

            var member = Assembly.GetType(metadataToken);

            if (member is IDSharpType typeMember)
            {
                throw new ArgumentException($"Specified metadata token return invalid member: \"{member}\"");
            }

            var typeInfo = GetRuntimeInfo(member.DeclaringType);
            var unmanagedMembers = membersSelector(typeInfo);

            for (int i = 0; i < unmanagedMembers.Length; i++)
            {
                var tokenPointer = (DSharpMetadataToken*)unmanagedMembers.GetItemReference(i);

                if (*tokenPointer == metadataToken)
                {
                    return (T*)tokenPointer;
                }
            }

            throw new InvalidOperationException($"Unable to find \"{member}\" in \"{member.DeclaringType}\"");
        }
        private DSharpRuntimeTypeInfo* CreateTypeInfo(IDSharpType type, DSharpRuntimeTypeInfo** reference)
        {
            var constructors = type.GetConstructors();
            var genericParameters = type.GetGenericParameters();
            var genericTypes = type.GetGenericTypes();
            IDSharpType[] generics;
            IDSharpType[] interfaces = [.. type.GetInterfaces()];
            IDSharpType baseType = type.GetBaseTypes().FirstOrDefault(t => t.ObjectType == DSharpObjectType.Class) ?? type.Assembly.ObjectType;

            List<IDSharpFieldInfo> allFields = [..type.GetAllFields(true), ..type.GetAllFields(false)];
            List<IDSharpFieldInfo> fields = [];
            List<IDSharpPropertyInfo> properties = [];
            List<IDSharpMethodInfo> methods = [];
            Dictionary<IDSharpMemberInfo, int> memberInfoSize = [];
            Pointer<DSharpRuntimeTypeInfo>[] runtimeGenericParameters;
            Pointer<DSharpRuntimeTypeInfo>[] runtimeInterfaces;

            if (genericParameters.Length > 0)
            {
                generics = genericParameters;
            }
            else if (genericTypes.Length > 0)
            {
                generics = genericTypes;
            }
            else
            {
                generics = [];
            }

            foreach (var constructor in constructors)
            {
                var size = DSharpRuntimeMethodInfo.GetSize(constructor);
                memberInfoSize.Add(constructor, size);
            }

            void AddMembers(IDSharpType type)
            {
                foreach (var field in type.GetFields())
                {
                    fields.Add(field);
                    var size = DSharpRuntimeFieldInfo.GetSize(field);
                    memberInfoSize.Add(field, size);
                }
                foreach (var property in type.GetProperties())
                {
                    properties.Add(property);
                    var size = DSharpRuntimePropertyInfo.GetSize(property);
                    memberInfoSize.Add(property, size);
                }
                foreach (var indexer in type.GetIndexers())
                {
                    properties.Add(indexer);
                    var size = DSharpRuntimePropertyInfo.GetSize(indexer);
                    memberInfoSize.Add(indexer, size);
                }
                foreach (var method in type.GetMethods())
                {
                    methods.Add(method);
                    var size = DSharpRuntimeMethodInfo.GetSize(method);
                    memberInfoSize.Add(method, size);
                }
            }

            AddMembers(type);

            int instanceSize = type.GetSize(true, false);
            int staticSize = type.GetSize(false, false);
            int infoSize = staticSize +
                           generics.Length * sizeof(Pointer<DSharpRuntimeTypeInfo>) +
                           interfaces.Length * sizeof(Pointer<DSharpRuntimeTypeInfo>) +
                           allFields.Count * sizeof(UnmanagedDictionary<Pointer<DSharpRuntimeFieldInfo>, UnmanagedArray<byte>>) +
                           memberInfoSize.Values.Sum() +
                           type.Name.Length * sizeof(char) +
                           ((type.Namespace?.Length ?? 0) * sizeof(char));

            var info = _memoryManager.Allocate<DSharpRuntimeTypeInfo>(DSharpMemoryBlockType.TypeInformation, infoSize);
            *reference = info;
            infoSize += sizeof(DSharpRuntimeTypeInfo);
            MemoryBuilder builder = new((nint)info, infoSize);
            builder.Allocate(sizeof(DSharpRuntimeTypeInfo));

            info->MetadataToken = type.MetadataToken;
            info->ObjectType = type.ObjectType;
            info->Size = instanceSize;
            info->IsGeneric = type.IsGeneric;
            info->Name = builder.AllocateString(type.Name);
            info->Namespace = builder.AllocateString(type.Namespace);
            info->Constructors = builder.AllocateArray<DSharpRuntimeMethodInfo>(constructors.Length);
            info->Methods = builder.AllocateArray<DSharpRuntimeMethodInfo>(methods.Count);
            info->Properties = builder.AllocateArray<DSharpRuntimePropertyInfo>(properties.Count);
            info->Fields = builder.AllocateArray<DSharpRuntimeFieldInfo>(fields.Count);
            info->FieldsOffset = builder.AllocateDictionary<Pointer<DSharpRuntimeFieldInfo>, int>(allFields.Count);
            info->StaticFieldsData = builder.AllocateArray<byte>(staticSize);
            info->IsStaticInitializerCalled = false;

            runtimeGenericParameters = CreateTypes(generics);
            runtimeInterfaces = CreateTypes(interfaces);

            info->GenericParameters = builder.AllocateArray(runtimeGenericParameters);
            info->BaseType = GetRuntimeInfo(baseType);
            info->Interfaces = builder.AllocateArray(runtimeInterfaces);

            if (DSharpBuildInTypes.TryGetValueTypeIndex(type, out var valueTypeIndex))
            {
                info->BuildInValueTypeIndex = valueTypeIndex;
            }
            else
            {
                info->BuildInValueTypeIndex = -1;
            }
            if (DSharpBuildInTypes.TryGetInfo(type, out var buildInTypeInfo))
            {
                info->Converter = buildInTypeInfo.Converter;
            }

            for (int i = 0; i < constructors.Length; i++)
            {
                CreateMethodInfo(info, constructors[i], info->Constructors.GetItemReference(i), ref builder);
            }
            for (int i = 0; i < methods.Count; i++)
            {
                CreateMethodInfo(info, methods[i], info->Methods.GetItemReference(i), ref builder);
            }
            for (int i = 0; i < properties.Count; i++)
            {
                CreatePropertyInfo(info, properties[i], info->Properties.GetItemReference(i), ref builder);
            }

            int fieldOffset = sizeof(DSharpObject);
            int staticFieldOffset = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                var fieldInfo = info->Fields.GetItemReference(i);
                CreateFieldInfo(info, fields[i], fieldInfo, ref builder);

                int fieldSize = fieldInfo->FieldType->ItemSize;

                if (fieldInfo->FieldType->IsValueType)
                {
                    fieldSize += sizeof(DSharpObject);
                }

                if (fieldInfo->IsStatic)
                {
                    info->FieldsOffset.Add(fieldInfo, staticFieldOffset);
                    staticFieldOffset += fieldSize;
                }
                else
                {
                    info->FieldsOffset.Add(fieldInfo, fieldOffset);
                    fieldOffset += fieldSize;
                }
            }
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var propertyInfo = info->Properties.GetItemReference(i);
                var implementations = property.GetImplementedProperties();
                var implementedProperties = builder.AllocateArray<Pointer<DSharpRuntimePropertyInfo>>(implementations.Length);

                propertyInfo->PropertyType = GetRuntimeInfo(property.PropertyType);
                propertyInfo->ImplementedProperties = implementedProperties;

                for (int m = 0; m < implementations.Length; m++)
                {
                    implementedProperties[m] = GetMethod(implementations[m].MetadataToken);
                }
                if (property.OverrideProperty != null)
                {
                    propertyInfo->Overrides = GetProperty(property.OverrideProperty.MetadataToken);
                }
            }
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var methodInfo = info->Methods.GetItemReference(i);
                var implementations = method.GetImplementedMethods();
                var implementedMethods = builder.AllocateArray<Pointer<DSharpRuntimeMethodInfo>>(implementations.Length);
                var methodParameters = method.GetParameters();
                var methodGenericParameters = method.GetGenericParameters();

                methodInfo->ReturnType = method.ReturnType == null ? null : GetRuntimeInfo(method.ReturnType);
                methodInfo->ImplementedMethods = implementedMethods;
                methodInfo->ParametersType = DSharpRuntimeParameterInfo.Create(this, methodParameters, ref builder);
                methodInfo->GenericTypes = builder.AllocateArray(methodGenericParameters, t => (nint)GetRuntimeInfo(t));

                for (int m = 0; m < implementations.Length; m++)
                {
                    implementedMethods[m] = GetMethod(implementations[m].MetadataToken);
                }
                if (method.OverrideMethod != null)
                {
                    methodInfo->Overrides = GetMethod(method.OverrideMethod.MetadataToken);
                }
            }
            for (int i = 0; i < constructors.Length; i++)
            {
                var constructor = constructors[i];
                var constructorInfo = info->Constructors.GetItemReference(i);
                var constructorParameters = constructor.GetParameters();

                constructorInfo->ReturnType = constructor.ReturnType == null ? null : GetRuntimeInfo(constructor.ReturnType);
                constructorInfo->ParametersType = DSharpRuntimeParameterInfo.Create(this, constructorParameters, ref builder);
            }

            return info;
        }
        private void CreateMethodInfo(DSharpRuntimeTypeInfo* type, IDSharpMethodInfo method, DSharpRuntimeMethodInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = method.MetadataToken;
            info->Name = builder.AllocateString(method.Name);
            info->IsAbstract = method.IsAbstract;
            info->IsVirtual = method.IsVirtual;
            info->IsStatic = method.IsStatic;
            info->IsExtern = method.IsExtern;
            info->DeclaringType = type;
            info->MethodType = method.MethodType;
            info->ParsedBytecode = null;

            var bytecode = method.Bytecode;

            if (bytecode == null)
            {
                info->Bytecode = new(null, 0);
            }
            else
            {
                info->Bytecode = builder.AllocateArray<byte>(bytecode.Size);
                bytecode.CopyTo(info->Bytecode);
            }

            if (method.MethodType == DSharpMethodType.Initializer)
            {
                if (method.IsStatic)
                {
                    type->StaticInitializer = info;
                }
                else
                {
                    type->Initializer = info;
                }
            }
            else if (method.MethodType == DSharpMethodType.Finalizer)
            {
                type->Finalizer = info;
            }

            _methods.Add(method.MetadataToken, info);
        }
        private void CreatePropertyInfo(DSharpRuntimeTypeInfo* type, IDSharpPropertyInfo property, DSharpRuntimePropertyInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = property.MetadataToken;
            info->Name = builder.AllocateString(property.Name);
            info->IsAbstract = property.IsAbstract;
            info->IsVirtual = property.IsVirtual;
            info->IsStatic = property.IsStatic;
            info->DeclaringType = type;

            if (property.Getter != null && type->TryGetMethod(property.Getter.MetadataToken, out var getter))
            {
                info->Getter = getter;
            }
            if (property.Setter != null && type->TryGetMethod(property.Setter.MetadataToken, out var setter))
            {
                info->Setter = setter;
            }

            _properties.Add(property.MetadataToken, info);
        }
        private void CreateFieldInfo(DSharpRuntimeTypeInfo* type, IDSharpFieldInfo field, DSharpRuntimeFieldInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = field.MetadataToken;
            info->Name = builder.AllocateString(field.Name);
            info->IsStatic = field.IsStatic;
            info->DeclaringType = type;
            info->FieldType = GetRuntimeInfo(field.FieldType);

            _fields.Add(field.MetadataToken, info);
        }

        private Pointer<DSharpRuntimeTypeInfo>[] CreateTypes(IDSharpType[] types)
        {
            if (types.Length == 0)
            {
                return [];
            }

            var result = new Pointer<DSharpRuntimeTypeInfo>[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                result[i] = GetRuntimeInfo(types[i]);
            }

            return result;
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!_memoryManager.IsDisposed)
            {
                foreach (var typeReference in _types.Values)
                {
                    var typeInfo = (DSharpRuntimeTypeInfo*)*typeReference.AsPointer();

                    for (int i = 0; i < typeInfo->Methods.Length; i++)
                    {
                        var method = typeInfo->Methods[i];

                        if (method.ParsedBytecode != null)
                        {
                            _memoryManager.Free(method.ParsedBytecode);
                        }
                    }

                    _memoryManager.Free(typeInfo);
                    _memoryManager.Free(typeReference);
                }
            }

            _types.Clear();
            _methods.Clear();
            _properties.Clear();
            _fields.Clear();
        }

        #endregion
    }
}
