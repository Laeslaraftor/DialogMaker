using DialogMaker.Core.Scripting.Runtime.Executor.Bytecode;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor.TypesInfo
{
    /// <summary>
    /// Runtime types provides. It builds and stores runtime information about D# types
    /// </summary>
    /// <param name="assembly">Assembly that contains providing types information</param>
    public unsafe class DSharpRuntimeInformationProvider(IDSharpAssembly assembly) : Disposable
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
                throw new ObjectDisposedException(nameof(DSharpRuntimeInformationProvider));
            }

            if (!_types.TryGetValue(type.MetadataToken, out var runtimeType))
            {
                runtimeType = (nint)CreateTypeInfo(type);
                _types.Add(type.MetadataToken, runtimeType);
            }

            return (DSharpRuntimeTypeInfo*)runtimeType;
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

            methodInfo->ParsedBytecode = DSharpRuntimeBytecode.Parse(this, methodInfo->Bytecode);

            return methodInfo->ParsedBytecode;
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

            int staticSize = type.GetSize(false, false);
            int infoSize = sizeof(DSharpRuntimeTypeInfo) +
                           staticSize +
                           runtimeGenericParameters.Length * sizeof(nint) +
                           memberInfoSize.Values.Sum() +
                           type.Name.Length * sizeof(char) +
                           type.Namespace?.Length ?? 0;

            var info = (DSharpRuntimeTypeInfo*)Marshal.AllocHGlobal(infoSize);
            MemoryBuilder builder = new((nint)info, infoSize);
            builder.Allocate(sizeof(DSharpRuntimeTypeInfo));

            info->MetadataToken = type.MetadataToken;
            info->ObjectType = type.ObjectType;
            info->Size = type.GetSize(true, false);
            info->IsGeneric = type.IsGeneric;
            info->Name = builder.AllocateString(type.Name);
            info->Namespace = builder.AllocateString(type.Namespace);
            info->GenericParameters = builder.AllocateArray(runtimeGenericParameters);
            info->Constructors = builder.AllocateArray<DSharpRuntimeMethodInfo>(constructors.Length);
            info->Methods = builder.AllocateArray<DSharpRuntimeMethodInfo>(methods.Count);
            info->Properties = builder.AllocateArray<DSharpRuntimePropertyInfo>(properties.Count);
            info->Fields = builder.AllocateArray<DSharpRuntimeFieldInfo>(fields.Count);
            info->StaticFieldsData = builder.AllocateArray<byte>(staticSize);

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
        }
        private void CreateFieldInfo(DSharpRuntimeTypeInfo* type, IDSharpFieldInfo field, DSharpRuntimeFieldInfo* info, ref MemoryBuilder builder)
        {
            info->MetadataToken = field.MetadataToken;
            info->IsStatic = field.IsStatic;
            info->DeclaringType = type;
            info->FieldType = GetRuntimeInfo(field.FieldType);
        }

        #endregion

        #region Disposing

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var type in _types.Values)
            {
                var typeInfo = (DSharpRuntimeTypeInfo*)type;

                for (int i = 0; i < typeInfo->Methods.Length; i++)
                {
                    var method = typeInfo->Methods[i];

                    if (method.ParsedBytecode != null)
                    {
                        Marshal.FreeHGlobal((nint)method.ParsedBytecode);
                    }
                }

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
