using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;

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
        private readonly Dictionary<DSharpMetadataToken, nint> _types = [];
        private readonly Dictionary<DSharpMetadataToken, nint> _staticMethods = [];
        private readonly Dictionary<DSharpMetadataToken, nint> _staticProperties = [];
        private readonly Dictionary<DSharpMetadataToken, nint> _staticFields = [];
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
                typeReference = _memoryManager.Allocate(DSharpMemoryBlockType.TypeInformation, sizeof(nint));
                _types.Add(type.MetadataToken, typeReference);

                *(DSharpRuntimeTypeInfo**)typeReference = CreateTypeInfo(type, (DSharpRuntimeTypeInfo**)typeReference);
            }

            return *(DSharpRuntimeTypeInfo**)typeReference;
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
        public DSharpRuntimeMethodInfo* GetStaticMethod(DSharpMetadataToken metadataToken)
        {
            if (_staticMethods.TryGetValue(metadataToken, out var address))
            {
                return (DSharpRuntimeMethodInfo*)address;
            }
            if (Assembly.GetType(metadataToken) is not IDSharpMethodInfo methodInfo)
            {
                throw new InvalidOperationException($"Unable to find method for token: {metadataToken}");
            }

            var typeInfo = GetRuntimeInfo(methodInfo.DeclaringType);

            if (typeInfo->TryGetMethod(metadataToken, out var runtimeMethod))
            {
                return runtimeMethod;
            }

            throw new InvalidOperationException($"Unable to get runtime information about method \"{methodInfo}\"");
        }
        public DSharpRuntimePropertyInfo* GetStaticProperty(DSharpMetadataToken metadataToken)
        {
            if (_staticProperties.TryGetValue(metadataToken, out var address))
            {
                return (DSharpRuntimePropertyInfo*)address;
            }
            if (Assembly.GetType(metadataToken) is not IDSharpPropertyInfo propertyInfo)
            {
                throw new InvalidOperationException($"Unable to find property for token: {metadataToken}");
            }

            var typeInfo = GetRuntimeInfo(propertyInfo.DeclaringType);

            if (typeInfo->TryGetProperty(metadataToken, out var runtimeProperty))
            {
                return runtimeProperty;
            }

            throw new InvalidOperationException($"Unable to get runtime information about property \"{propertyInfo}\"");
        }
        public DSharpRuntimeFieldInfo* GetStaticField(DSharpMetadataToken metadataToken)
        {
            if (_staticProperties.TryGetValue(metadataToken, out var address))
            {
                return (DSharpRuntimeFieldInfo*)address;
            }
            if (Assembly.GetType(metadataToken) is not IDSharpFieldInfo fieldInfo)
            {
                throw new InvalidOperationException($"Unable to find field for token: {metadataToken}");
            }

            var typeInfo = GetRuntimeInfo(fieldInfo.DeclaringType);

            if (typeInfo->TryGetField(metadataToken, out var runtimeField))
            {
                return runtimeField;
            }

            throw new InvalidOperationException($"Unable to get runtime information about field \"{fieldInfo}\"");
        }

        private DSharpRuntimeTypeInfo* CreateTypeInfo(IDSharpType type, DSharpRuntimeTypeInfo** reference)
        {
            var constructors = type.GetConstructors();
            var genericParameters = type.GetGenericParameters();
            var genericTypes = type.GetGenericTypes();
            IDSharpType[] generics;
            IDSharpType[] baseTypes = type.GetBaseTypes();

            List<IDSharpFieldInfo> fields = [];
            List<IDSharpPropertyInfo> properties = [];
            List<IDSharpMethodInfo> methods = [];
            Dictionary<IDSharpMemberInfo, int> memberInfoSize = [];
            Pointer<DSharpRuntimeTypeInfo>[] runtimeGenericParameters;
            Pointer<DSharpRuntimeTypeInfo>[] runtimeBaseTypes;

            Pointer<DSharpRuntimeTypeInfo>[] CreateTypes(IDSharpType[] types)
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
                    memberInfoSize.Add(field, sizeof(DSharpRuntimeFieldInfo));
                }
                foreach (var property in type.GetProperties())
                {
                    properties.Add(property);
                    memberInfoSize.Add(property, sizeof(DSharpRuntimePropertyInfo));
                }
                foreach (var method in type.GetMethods())
                {
                    methods.Add(method);
                    var size = DSharpRuntimeMethodInfo.GetSize(method);
                    memberInfoSize.Add(method, size);
                }
            }
            void ParseType(IDSharpType type)
            {
                foreach (var baseType in type.GetBaseTypes().Where(t => t.ObjectType != DSharpObjectType.Interface))
                {
                    ParseType(baseType);
                }

                AddMembers(type);
            }

            ParseType(type);

            int staticSize = type.GetSize(false, false);
            int infoSize = staticSize +
                           generics.Length * sizeof(Pointer<DSharpRuntimeTypeInfo>) +
                           baseTypes.Length * sizeof(Pointer<DSharpRuntimeTypeInfo>) +
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
            info->Size = type.GetSize(true, false);
            info->IsGeneric = type.IsGeneric;
            info->Name = builder.AllocateString(type.Name);
            info->Namespace = builder.AllocateString(type.Namespace);
            info->Constructors = builder.AllocateArray<DSharpRuntimeMethodInfo>(constructors.Length);
            info->Methods = builder.AllocateArray<DSharpRuntimeMethodInfo>(methods.Count);
            info->Properties = builder.AllocateArray<DSharpRuntimePropertyInfo>(properties.Count);
            info->Fields = builder.AllocateArray<DSharpRuntimeFieldInfo>(fields.Count);
            info->StaticFieldsData = builder.AllocateArray<byte>(staticSize);

            runtimeGenericParameters = CreateTypes(generics);
            runtimeBaseTypes = CreateTypes(baseTypes);

            info->GenericParameters = builder.AllocateArray(runtimeGenericParameters);
            info->BaseTypes = builder.AllocateArray(runtimeBaseTypes);

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
                var field = info->Fields.GetItemReference(i);
                CreateFieldInfo(info, fields[i], field, ref builder);

                int fieldSize = field->FieldType->ItemSize;

                if (field->IsStatic)
                {
                    field->Offset = staticFieldOffset;
                    staticFieldOffset += fieldSize;
                }
                else
                {
                    field->Offset = fieldOffset;
                    fieldOffset += fieldSize;
                }
            }
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                if (property.OverrideProperty == null ||
                    !info->TryGetProperty(property.MetadataToken, out var overridenProperty))
                {
                    continue;
                }

                info->Properties.GetItemReference(i)->Overrides = overridenProperty;
            }
            for (int i = 0; i < methods.Count; i++)
            {
                var method = methods[i];

                if (method.OverrideMethod == null ||
                    !info->TryGetMethod(method.MetadataToken, out var overridenMethod))
                {
                    continue;
                }

                info->Methods.GetItemReference(i)->Overrides = overridenMethod;
            }

            return info;
        }
        private void CreateMethodInfo(DSharpRuntimeTypeInfo* type, IDSharpMethodInfo method, DSharpRuntimeMethodInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = method.MetadataToken;
            info->IsAbstract = method.IsAbstract;
            info->IsVirtual = method.IsVirtual;
            info->IsStatic = method.IsStatic;
            info->IsExtern = method.IsExtern;
            info->DeclaringType = type;
            info->MethodType = method.MethodType;
            info->ReturnType = method.ReturnType == null ? null : GetRuntimeInfo(method.ReturnType);
            info->ParsedBytecode = null;

            var parameters = method.GetParameters();
            var genericParameters = method.GetGenericParameters();

            info->ParametersType = DSharpRuntimeParameterInfo.Create(this, parameters, ref builder);
            info->GenericTypes = builder.AllocateArray(genericParameters, t => (nint)GetRuntimeInfo(t));

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

            if (method.IsStatic)
            {
                _staticMethods.Add(method.MetadataToken, (nint)info);
            }
        }
        private void CreatePropertyInfo(DSharpRuntimeTypeInfo* type, IDSharpPropertyInfo property, DSharpRuntimePropertyInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = property.MetadataToken;
            info->IsAbstract = property.IsAbstract;
            info->IsVirtual = property.IsVirtual;
            info->IsStatic = property.IsStatic;
            info->DeclaringType = type;
            info->PropertyType = GetRuntimeInfo(property.PropertyType);

            if (property.Getter != null && type->TryGetMethod(property.Getter.MetadataToken, out var getter))
            {
                info->Getter = getter;
            }
            if (property.Setter != null && type->TryGetMethod(property.Setter.MetadataToken, out var setter))
            {
                info->Setter = setter;
            }

            if (property.IsStatic)
            {
                _staticProperties.Add(property.MetadataToken, (nint)info);
            }
        }
        private void CreateFieldInfo(DSharpRuntimeTypeInfo* type, IDSharpFieldInfo field, DSharpRuntimeFieldInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = field.MetadataToken;
            info->IsStatic = field.IsStatic;
            info->DeclaringType = type;
            info->FieldType = GetRuntimeInfo(field.FieldType);

            if (field.IsStatic)
            {
                _staticFields.Add(field.MetadataToken, (nint)info);
            }
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
                    var typeInfo = *(DSharpRuntimeTypeInfo**)typeReference;

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
            _staticMethods.Clear();
            _staticProperties.Clear();
            _staticFields.Clear();
        }

        #endregion
    }
}
