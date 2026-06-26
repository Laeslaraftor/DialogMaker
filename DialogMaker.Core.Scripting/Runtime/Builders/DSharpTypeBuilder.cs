using DialogMaker.Core.Scripting.Compiler.Ast;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

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
        public override DSharpAccessModifier Access { get; set; } = DSharpAccessModifier.Public;
        public DSharpObjectType ObjectType { get; set; } = DSharpObjectType.Class;
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
                else if (GenericParameters.Count > 0)
                {
                    result += "`" + GenericParameters.Count;
                }
                if (DeclaringType != null)
                {
                    result = $"{DeclaringType.FullName}.{result}";
                }
                else if (Namespace != null)
                {
                    result = $"{Namespace}.{result}";
                }

                return result;
            }
        }
        public List<DSharpTypeToken> BaseTypes { get; } = [];
        /// <summary>
        /// <inheritdoc cref="IDSharpType.GetGenericParameters"/>
        /// </summary>
        public List<DSharpTypeToken> GenericParameters { get; } = [];
        /// <summary>
        /// <inheritdoc cref="IDSharpType.GetGenericTypes"/>
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
        public ReferenceReadOnlyList<DSharpIndexerBuilder> Indexers
        {
            get
            {
                field ??= new(_indexers);
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
        public ReferenceReadOnlyList<DSharpTypeBuilder> ChildrenTypes
        {
            get
            {
                field ??= new(_childrenTypes);
                return field;
            }
        }
        /// <summary>
        /// <inheritdoc cref="IDSharpType.Finalizer"/>
        /// </summary>
        public DSharpMethodBuilder? Finalizer { get; private set; }
        public IDSharpType? GenericTemplate
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    _templatedMembers = null;
                    _replacedTypes = null;
                }
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool IsDeclaration => false;

        IDSharpMethodInfo? IDSharpType.Finalizer => Finalizer;

        private readonly List<DSharpMethodBuilder> _constructors = [];
        private readonly List<DSharpMethodBuilder> _methods = [];
        private readonly List<DSharpPropertyBuilder> _properties = [];
        private readonly List<DSharpIndexerBuilder> _indexers = [];
        private readonly List<DSharpFieldBuilder> _fields = [];
        private readonly List<DSharpTypeBuilder> _genericTypes = [];
        private readonly List<DSharpTypeBuilder> _childrenTypes = [];
        internal IReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo>? _templatedMembers;
        internal IReadOnlyDictionary<IDSharpType, IDSharpType>? _replacedTypes;

        #region Управление

        internal DSharpMethodBuilder CreateMethod(Func<DSharpTypeToken, DSharpMethodBuilder> fabric)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains methods");
            }

            return CreateMember(DSharpMetadataTokenType.Method, _methods, fabric);
        }
        internal override void Update()
        {
            base.Update();

            static void UpdateAll(IEnumerable<DSharpMemberInfoBuilder> builders)
            {
                foreach (var builder in builders)
                {
                    builder.Update();
                }
            }

            UpdateAll(Constructors);
            UpdateAll(Methods);
            UpdateAll(Fields);
            UpdateAll(Properties);
            UpdateAll(Indexers);
        }

        public DSharpTypeBuilder CreateGenericType(string name)
        {
            var type = Assembly.CreateType(name, true, this);
            _genericTypes.Add(type);

            return type;
        }
        public DSharpMethodBuilder CreateConstructor()
        {
            if (ObjectType == DSharpObjectType.Interface)
            {
                throw new InvalidOperationException($"Can not create constructor for interface \"{this}\"");
            }
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains constructors");
            }

            return CreateMember(DSharpMetadataTokenType.Method, _constructors, t => DSharpMethodBuilder.CreateConstructor(this, t));
        }
        public DSharpMethodBuilder CreateFinalizer()
        {
            if (ObjectType == DSharpObjectType.Interface)
            {
                throw new InvalidOperationException($"Can not create finalizer for interface \"{this}\"");
            }
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains finalizers");
            }
            if (Finalizer != null)
            {
                throw new InvalidOperationException($"Can not create multiple finalizers for \"{this}\"");
            }

            var finalizer = CreateMember(DSharpMetadataTokenType.Method, _methods, t => DSharpMethodBuilder.CreateFinalizer(this, t));
            Finalizer = finalizer;

            return finalizer;
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
        public DSharpIndexerBuilder CreateIndexer()
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains indexers");
            }

            var indexer = CreateMember(DSharpMetadataTokenType.Property, _indexers, t => new(Assembly, this, IndexerName, t));

            return indexer;
        }
        public DSharpTypeBuilder CreateChildType(string name)
        {
            var type = Assembly.CreateType(name, false, this);
            _childrenTypes.Add(type);

            return type;
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
        public bool RemoveConstructor(DSharpMethodBuilder constructor) => RemoveMember(_constructors, constructor);
        public bool RemoveMethod(DSharpMethodBuilder method) => RemoveMember(_methods, method);
        public bool RemoveFinalizer()
        {
            if (Finalizer == null)
            {
                return false;
            }

            var finalizer = Finalizer;
            Finalizer = null;

            return _methods.Remove(finalizer);
        }
        public bool RemoveProperty(DSharpPropertyBuilder property) => RemoveMember(_properties, property);
        public bool RemoveIndexer(DSharpIndexerBuilder indexer) => RemoveMember(_indexers, indexer);
        public bool RemoveField(DSharpFieldBuilder field) => RemoveMember(_fields, field);
        public bool RemoveChildType(DSharpTypeBuilder type)
        {
            Assembly.RemoveType(type);
            return _childrenTypes.Remove(type);
        }

        /// <summary>
        /// Get dictionary of members that created based on <see cref="GenericTemplate"/>
        /// </summary>
        /// <returns>Dictionary of members that created based template</returns>
        public IReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo> GetTemplatedMembers()
        {
            if (_templatedMembers != null)
            {
                return _templatedMembers;
            }
            if (GenericTemplate == null)
            {
                throw new InvalidOperationException($"Unable to get dictionary of members that created based on template because current type does not have template \"{this}\"");
            }

            Dictionary<IDSharpMemberInfo, IDSharpMemberInfo> members = [];

            var properties = GenericTemplate.GetProperties();
            var fields = GenericTemplate.GetFields();
            var methods = GenericTemplate.GetMethods();
            var constructors = GenericTemplate.GetConstructors();
            var genericTypes = GenericTemplate.GetGenericTypes();

            if (genericTypes.Length != GenericParameters.Count)
            {
                throw new InvalidOperationException($"Type must contains same amount of generic parameters that it's template");
            }
            if (properties.Length != Properties.Count)
            {
                throw new InvalidOperationException($"Type must contains same properties that it's template");
            }
            if (fields.Length != Fields.Count)
            {
                throw new InvalidOperationException($"Type must contains same fields that it's template");
            }
            if (methods.Length != Methods.Count)
            {
                throw new InvalidOperationException($"Type must contains same methods that it's template");
            }
            if (constructors.Length != Constructors.Count)
            {
                throw new InvalidOperationException($"Type must contains same constructors that it's template");
            }

            void Copy<T>(T[] templateMembers, IReadOnlyList<T> newMembers)
                where T : IDSharpMemberInfo
            {
                for (int i = 0; i < templateMembers.Length; i++)
                {
                    members.Add(templateMembers[i], newMembers[i]);
                }
            }

            for (int i = 0; i < genericTypes.Length; i++)
            {
                var newType = (IDSharpType)Assembly.GetType(GenericParameters[i]);
                members.Add(genericTypes[i], newType);
            }

            Copy(properties, Properties);
            Copy(fields, Fields);
            Copy(methods, Methods);
            Copy(constructors, Constructors);

            _templatedMembers = new ReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo>(members);

            return _templatedMembers;
        }
        public IReadOnlyDictionary<IDSharpType, IDSharpType> GetReplacedTypes()
        {
            if (_replacedTypes != null)
            {
                return _replacedTypes;
            }
            if (GenericTemplate == null)
            {
                throw new InvalidOperationException($"Unable to get dictionary of types that replaced by type parameters because current type does not have template \"{this}\"");
            }

            Dictionary<IDSharpType, IDSharpType> replacedTypes = [];
            var genericTypes = GenericTemplate.GetGenericTypes();

            for (int i = 0; i < GenericParameters.Count; i++)
            {
                replacedTypes.Add(genericTypes[i], (IDSharpType)Assembly.GetType(GenericParameters[i]));
            }

            _replacedTypes = new ReadOnlyDictionary<IDSharpType, IDSharpType>(replacedTypes);

            return _replacedTypes;
        }

        public bool TryGetInheritedFinalizer([NotNullWhen(true)] out IDSharpMethodInfo? result)
        {
            static IDSharpMethodInfo? FindInBaseType(IDSharpType type, bool skipFirstCheck)
            {
                if (!skipFirstCheck)
                {
                    if (type is DSharpTypeBuilder builder)
                    {
                        if (builder.Finalizer != null)
                        {
                            return builder.Finalizer;
                        }
                    }
                    else
                    {
                        var finalizer = type.GetMethodOrDefault(FinalizerName);

                        if (finalizer != null)
                        {
                            return finalizer;
                        }
                    }
                }

                foreach (var baseType in type.GetBaseTypes())
                {
                    if (baseType.ObjectType == DSharpObjectType.Interface)
                    {
                        continue;
                    }

                    var finalizer = FindInBaseType(baseType, false);
                    return finalizer;
                }

                return null;
            }

            result = FindInBaseType(this, true);
            return result != null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return FullName;
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
        public IDSharpIndexerInfo[] GetIndexers() => [.. _indexers];
        public IDSharpIndexerInfo[] GetIndexers(Predicate<IDSharpIndexerInfo> predicate) => [.. _indexers.Where(i => predicate(i))];
        public IDSharpFieldInfo[] GetFields() => [.._fields];
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate) => [.. _fields.Where(f => predicate(f))];
        public IDSharpType[] GetBaseTypes() => [.. BaseTypes.Select(t => (IDSharpType)Assembly.GetType(t))];
        public IDSharpMethodInfo[] GetConstructors() => [.. _constructors];
        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate) => [.. _constructors.Where(c => predicate(c))];
        public IDSharpType[] GetGenericParameters() => [.. GenericParameters.Select(t => (IDSharpType)Assembly.GetType(t))];
        public IDSharpType[] GetGenericTypes() => [.. GenericTypes];
        public IDSharpType[] GetChildrenTypes() => [.. ChildrenTypes];

        #endregion

        #region Константы

        public const string ConstructorName = "ctor";
        public const string FinalizerName = "Finalize";
        public const string IndexerName = "Item";

        #endregion
    }
}
