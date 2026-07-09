using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime types provides. It builds and stores runtime information about D# types
    /// </summary>
    /// <param name="assembly">Assembly that contains providing types information</param>
    public unsafe class DSharpRuntimeTypesProvider(IDSharpAssembly assembly) : Disposable
    {
        /// <summary>
        /// Assembly that contains providing types information
        /// </summary>
        public IDSharpAssembly Assembly { get; } = assembly;

        private readonly Dictionary<DSharpMetadataToken, nint> _types = [];
        private readonly Dictionary<DSharpMetadataToken, nint> _methods = [];
        private readonly Dictionary<DSharpMetadataToken, nint> _properties = [];
        private readonly Dictionary<DSharpMetadataToken, nint> _fields = [];

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
                throw new ObjectDisposedException(nameof(DSharpRuntimeTypesProvider));
            }

            if (!_types.TryGetValue(type.MetadataToken, out var runtimeType))
            {
                runtimeType = (nint)CreateTypeInfo(type);
                _types.Add(type.MetadataToken, runtimeType);
            }

            return (DSharpRuntimeTypeInfo*)runtimeType;
        }

        private DSharpRuntimeTypeInfo* CreateTypeInfo(DSharpMetadataToken metadataToken)
        {
            var type = (IDSharpType)Assembly.GetType(metadataToken);
            return CreateTypeInfo(type);
        }
        private DSharpRuntimeTypeInfo* CreateTypeInfo(IDSharpType type)
        {
            var constructors = type.GetConstructors();
            var genericParameters = type.GetGenericParameters();
            var genericTypes = type.GetGenericTypes();

            List<IDSharpFieldInfo> fields = [];
            List<IDSharpPropertyInfo> properties = [];
            List<IDSharpMethodInfo> methods = [];
            Dictionary<IDSharpMemberInfo, int> memberInfoSize = [];
            nint[] runtimeGenericParameters;

            nint[] CreateTypes(IDSharpType[] types)
            {
                var result = new nint[types.Length];

                for (int i = 0; i < types.Length; i++)
                {
                    result[i] = (nint)GetRuntimeInfo(types[i]);
                }

                return result;
            }

            if (genericParameters.Length > 0)
            {
                runtimeGenericParameters = CreateTypes(genericParameters);
            }
            else if (genericTypes.Length > 0)
            {
                runtimeGenericParameters = CreateTypes(genericTypes);
            }
            else
            {
                runtimeGenericParameters = [];
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

            int infoSize = sizeof(DSharpRuntimeTypeInfo) +
                           runtimeGenericParameters.Length * sizeof(nint) +
                           memberInfoSize.Values.Sum() +
                           type.Name.Length * sizeof(char) +
                           type.Namespace?.Length ?? 0;

            var info = (DSharpRuntimeTypeInfo*)Marshal.AllocHGlobal(infoSize);
            MemoryBuilder builder = new((nint)info, infoSize);
            builder.Allocate(sizeof(DSharpRuntimeTypeInfo));

            info->MetadataToken = type.MetadataToken;
            info->ObjectType = type.ObjectType;
            info->Size = type.Size;
            info->IsGeneric = type.IsGeneric;
            info->Name = builder.AllocateString(type.Name);
            info->Namespace = builder.AllocateString(type.Namespace);
            info->GenericParameters = builder.AllocateArray(runtimeGenericParameters);
            info->Constructors = builder.AllocateArray<DSharpRuntimeMethodInfo>(constructors.Length);
            info->Methods = builder.AllocateArray<DSharpRuntimeMethodInfo>(methods.Count);
            info->Properties = builder.AllocateArray<DSharpRuntimePropertyInfo>(properties.Count);
            info->Fields = builder.AllocateArray<DSharpRuntimeFieldInfo>(fields.Count);

            if (DSharpBuildInTypes.TryGetValueTypeIndex(type, out var valueTypeIndex))
            {
                info->BuildInValueTypeIndex = valueTypeIndex;
            }
            else
            {
                info->BuildInValueTypeIndex = -1;
            }

            for (int i = 0; i < constructors.Length; i++)
            {
                CreateMethodInfo(info, constructors[i], info->Constructors.GetItemReference(i), builder);
            }
            for (int i = 0; i < methods.Count; i++)
            {
                CreateMethodInfo(info, methods[i], info->Methods.GetItemReference(i), builder);
            }
            for (int i = 0; i < properties.Count; i++)
            {
                CreatePropertyInfo(info, properties[i], info->Properties.GetItemReference(i), builder);
            }

            int fieldOffset = 0;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = info->Fields.GetItemReference(i);
                CreateFieldInfo(info, fields[i], field, builder);

                int fieldSize = field->FieldType->ItemSize;
                field->Offset = fieldOffset;
                fieldOffset += fieldSize;
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

        private void CreateMethodInfo(DSharpRuntimeTypeInfo* type, IDSharpMethodInfo method, DSharpRuntimeMethodInfo* info, MemoryBuilder builder)
        {
            info->MetadataToken = method.MetadataToken;
            info->MethodType = method.MethodType;
            info->ReturnType = method.ReturnType == null ? null : GetRuntimeInfo(method.ReturnType);

            var parameters = method.GetParameters();
            var genericParameters = method.GetGenericParameters();

            info->ParametersType = builder.AllocateArray(parameters, p => (nint)GetRuntimeInfo(p.Type));
            info->GenericTypes = builder.AllocateArray(genericParameters, t => (nint)GetRuntimeInfo(t));

            var bytecode = method.Bytecode;

            if (bytecode == null)
            {
                info->Bytecode = new(null, 0);
                return;
            }

            info->Bytecode = builder.AllocateArray<byte>(bytecode.Size);
            bytecode.CopyTo(info->Bytecode);
        }
        private void CreatePropertyInfo(DSharpRuntimeTypeInfo* type, IDSharpPropertyInfo property, DSharpRuntimePropertyInfo* info, MemoryBuilder builder)
        {
            info->MetadataToken = property.MetadataToken;
            info->PropertyType = GetRuntimeInfo(property.PropertyType);
            
            if (property.Getter != null && type->TryGetMethod(property.Getter.MetadataToken, out var getter))
            {
                info->Getter = getter;
            }
            if (property.Setter != null && type->TryGetMethod(property.Setter.MetadataToken, out var setter))
            {
                info->Setter = setter;
            }
        }
        private void CreateFieldInfo(DSharpRuntimeTypeInfo* type, IDSharpFieldInfo field, DSharpRuntimeFieldInfo* info, MemoryBuilder builder)
        {
            info->MetadataToken = field.MetadataToken;
            info->FieldType = GetRuntimeInfo(field.FieldType);
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var type in _types.Values)
            {
                Marshal.FreeHGlobal(type);
            }

            _types.Clear();
            _methods.Clear();
            _properties.Clear();
            _fields.Clear();
        }

        #endregion
    }
}
