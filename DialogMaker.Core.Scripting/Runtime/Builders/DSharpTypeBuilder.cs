namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpTypeBuilder(DSharpAssemblyBuilder assembly, bool isGeneric, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken) 
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken), IDSharpType
    {
        public DSharpTypeBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken)
            : this(assembly, false, declaringType, name, metadataToken)
        {
        }

        /// <summary>
        /// Flag that marks this type as generic.
        /// All generic types is auto generated from generic parameters
        /// </summary>
        public bool IsGeneric { get; } = isGeneric;
        /// <summary>
        /// Type that declared this field
        /// </summary>
        public override DSharpTypeBuilder? DeclaringType { get; } = declaringType;
        public DSharpObjectType Type { get; set; }
        public string? Namespace { get; set; }
        public string FullName
        {
            get
            {
                string result = Name;

                if (GenericTypes.Count > 0)
                {
                    result += "`" + GenericTypes.Count;
                }
                if (DeclaringType != null)
                {
                    result = $"{DeclaringType.FullName}.{result}";
                }
                else if (Namespace != null)
                {
                    result = $"{Namespace}.{result}";
                }

                for (int i = 0; i < ArrayDimensions; i++)
                {
                    result += "[]";
                }

                return result;
            }
        }
        public List<DSharpTypeToken> BaseTypes { get; } = [];
        /// <summary>
        /// List of types that must fill generic types. 
        /// Size of this list must be equals to generic types list or empty
        /// </summary>
        public List<DSharpTypeToken> GenericParameters { get; } = [];
        /// <summary>
        /// Generic types that created by this type
        /// </summary>
        public ReferenceReadOnlyList<DSharpTypeBuilder> GenericTypes
        {
            get
            {
                field ??= new(_genericTypes);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpMethodBuilder> Constructors
        {
            get
            {
                field ??= new(_constructors);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpMethodBuilder> Methods
        {
            get
            {
                field ??= new(_methods);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpPropertyBuilder> Properties
        {
            get
            {
                field ??= new(_properties);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpFieldBuilder> Fields
        {
            get
            {
                field ??= new(_fields);
                return field;
            }
        }
        public int ArrayDimensions { get; set; }

        private readonly List<DSharpMethodBuilder> _constructors = [];
        private readonly List<DSharpMethodBuilder> _methods = [];
        private readonly List<DSharpPropertyBuilder> _properties = [];
        private readonly List<DSharpFieldBuilder> _fields = [];
        private readonly List<DSharpTypeBuilder> _genericTypes = [];

        #region Управление

        internal DSharpMethodBuilder CreateMethod(Func<DSharpTypeToken, DSharpMethodBuilder> fabric)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains methods");
            }

            return CreateMember(DSharpMetadataTokenType.Method, _methods, fabric);
        }

        public DSharpTypeBuilder CreateGenericType(string name)
        {
            var type = Assembly.CreateType(name, true, this);
            _genericTypes.Add(type);

            return type;
        }
        public DSharpMethodBuilder CreateConstructor()
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains constructors");
            }

            return CreateMember(DSharpMetadataTokenType.Method, _constructors, t => new(Assembly, this, ConstructorName, t));
        }
        public DSharpMethodBuilder CreateMethod(string name)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains methods");
            }

            return CreateMethod(t => new(Assembly, this, name, t));
        }
        public DSharpPropertyBuilder CreateProperty(string name)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains properties");
            }

            return CreateMember(DSharpMetadataTokenType.Property, _properties, t => new(Assembly, this, name, t));
        }
        public DSharpFieldBuilder CreateField(string name)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains fields");
            }

            return CreateMember(DSharpMetadataTokenType.Field, _fields, t => new(Assembly, this, name, t));
        }

        public bool RemoveGenericType(DSharpTypeBuilder type)
        {
            if (RemoveMember(_genericTypes, type))
            {
                type.RemoveAllGenericTypes();
                return true;
            }

            return false;
        }
        public bool RemoveConstructor(DSharpMethodBuilder constructor)
        {
            return RemoveMember(_constructors, constructor);
        }
        public bool RemoveMethod(DSharpMethodBuilder method)
        {
            return RemoveMember(_methods, method);
        }
        public bool RemoveProperty(DSharpPropertyBuilder property)
        {
            return RemoveMember(_properties, property);
        }
        public bool RemoveField(DSharpFieldBuilder field)
        {
            return RemoveMember(_fields, field);
        }

        private void RemoveAllGenericTypes()
        {
            if (_genericTypes.Count == 0)
            {
                return;
            }

            foreach (var type in _genericTypes)
            {
                Assembly.RemoveType(type);
                type.RemoveAllGenericTypes();
            }

            _genericTypes.Clear();
        }

        #endregion

        #region Получение членов

        public IDSharpMethodInfo[] GetMethods() => [.. _methods];
        public IDSharpMethodInfo[] GetMethods(Predicate<IDSharpMethodInfo> predicate) => [.. _methods.Where(m => predicate(m))];
        public IDSharpPropertyInfo[] GetProperties() => [.. _properties];
        public IDSharpPropertyInfo[] GetProperties(Predicate<IDSharpPropertyInfo> predicate) => [.. _properties.Where(p => predicate(p))];
        public IDSharpFieldInfo[] GetFields() => [.._fields];
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate) => [.. _fields.Where(f => predicate(f))];
        public IDSharpType[] GetBaseTypes() => [.. BaseTypes.Select(t => (IDSharpType)Assembly.GetType(t))];
        public IDSharpMethodInfo[] GetConstructors() => [.. _constructors];
        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate) => [.. _constructors.Where(c => predicate(c))];

        #endregion

        #region Константы

        public const string ConstructorName = ".ctor";

        #endregion
    }
}
