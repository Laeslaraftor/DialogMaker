using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Builders
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

                if (GenericParameters.Count > 0)
                {
                    result += '<';
                    bool isFirst = true;

                    foreach (var typeToken in GenericParameters)
                    {
                        var type = Assembly.GetType(typeToken);

                        if (!isFirst)
                        {
                            result += ", ";
                        }

                        result += type;
                        isFirst = false;
                    }

                    result += '>';
                }
                else if (GenericTypes.Count > 0)
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

                return result;
            }
        }
        public ReferenceReadOnlyList<IDSharpType> BaseTypes
        {
            get
            {
                field ??= new(_baseTypes);
                return field;
            }
        }
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
        public ReferenceReadOnlyList<DSharpOperatorBuilder> CastOperators
        {
            get
            {
                field ??= new(_castOperators);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpOperatorBuilder> Operators
        {
            get
            {
                field ??= new(_operators);
                return field;
            }
        }
        /// <summary>
        /// <inheritdoc cref="IDSharpType.Finalizer"/>
        /// </summary>
        public DSharpMethodBuilder? Finalizer { get; private set; }
        /// <summary>
        /// <inheritdoc cref="IDSharpType.Initializer"/>
        /// </summary>
        public DSharpMethodBuilder? Initializer { get; private set; }
        /// <summary>
        /// <inheritdoc cref="IDSharpType.StaticInitializer"/>
        /// </summary>
        public DSharpMethodBuilder? StaticInitializer { get; private set; }
        /// <summary>
        /// <inheritdoc cref="IDSharpType.StaticConstructor"/>
        /// </summary>
        public DSharpMethodBuilder? StaticConstructor { get; private set; }
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
        public DSharpGenericTypeAttributes GenericAttributes { get; set; }
        internal Action? SetupHandler { get; set; }

        IDSharpMethodInfo? IDSharpType.Finalizer => Finalizer;
        IDSharpMethodInfo? IDSharpType.Initializer => Initializer;
        IDSharpMethodInfo? IDSharpType.StaticInitializer => StaticInitializer;
        IDSharpMethodInfo? IDSharpType.StaticConstructor => StaticConstructor;

        private readonly List<IDSharpType> _baseTypes = [];
        private readonly List<DSharpMethodBuilder> _constructors = [];
        private readonly List<DSharpMethodBuilder> _methods = [];
        private readonly List<DSharpPropertyBuilder> _properties = [];
        private readonly List<DSharpIndexerBuilder> _indexers = [];
        private readonly List<DSharpFieldBuilder> _fields = [];
        private readonly List<DSharpOperatorBuilder> _castOperators = [];
        private readonly List<DSharpOperatorBuilder> _operators = [];
        private readonly List<DSharpTypeBuilder> _genericTypes = [];
        private readonly List<DSharpTypeBuilder> _childrenTypes = [];
        internal IReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo>? _templatedMembers;
        internal ReadOnlyDictionary<IDSharpType, IDSharpType>? _replacedTypes;

        #region Управление

        internal DSharpMethodBuilder CreateMethod(Func<DSharpTypeToken, DSharpMethodBuilder> fabric)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains methods");
            }

            return CreateMember(DSharpMetadataTokenType.Method, _methods, fabric);
        }
        internal DSharpTypeBuilder CreateType(string name, bool isGeneric, bool addToChilds = false)
        {
            var type = Assembly.CreateType(name, isGeneric, this);

            if (addToChilds)
            {
                _childrenTypes.Add(type);
            }

            return type;
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

        public void AddBaseType(DSharpTypeToken typeToken)
        {
            if (Assembly.GetType(typeToken) is not IDSharpType type)
            {
                throw new ArgumentException($"Provided token is not referencing to type: {typeToken}", nameof(type));
            }

            AddBaseType(type);
        }
        public void AddBaseType(IDSharpType type)
        {
            if (type == this)
            {
                throw new ArgumentException($"Type \"{this}\" can not inherit itself", nameof(type));
            }
            if (type.IsSealed)
            {
                throw new ArgumentException($"Unable to inherit \"{this}\" by sealed type \"{type}\"", nameof(type));
            }
            if (ObjectType == DSharpObjectType.Enum &&
                type != Assembly.EnumType)
            {
                throw new ArgumentException($"Enum \"{this}\" can not inherit \"{type}\" because enums can not inherit anything", nameof(type));
            }
            if (ObjectType == DSharpObjectType.Interface &&
                type.ObjectType != DSharpObjectType.Interface)
            {
                throw new ArgumentException($"Interface \"{this}\" can not inherit \"{type}\" because interfaces can only inherit interfaces", nameof(type));
            }
            if (ObjectType == DSharpObjectType.Struct &&
                type.ObjectType != DSharpObjectType.Interface)
            {
                throw new ArgumentException($"Structure \"{this}\" can not inherit \"{type}\" because structures can only inherit interfaces", nameof(type));
            }
            if (ObjectType == DSharpObjectType.Class &&
                type.ObjectType != DSharpObjectType.Interface &&
                type.ObjectType != DSharpObjectType.Class)
            {
                throw new ArgumentException($"Class \"{this}\" can not inherit \"{type}\" because classes can only inherit other classes or interfaces", nameof(type));
            }
            if (_baseTypes.Contains(type))
            {
                throw new ArgumentException($"Type \"{this}\" already inherits \"{type}\"", nameof(type));
            }
            if (IsGeneric &&
                GenericAttributes.HasFlag(DSharpGenericTypeAttributes.Struct) &&
                type.ObjectType == DSharpObjectType.Class)
            {
                throw new ArgumentException($"Generic type \"{this}\" marked as struct and can not implement \"{type}\"", nameof(type));
            }
            if (ContainsInBaseTypes(this, type) ||
                ContainsInBaseTypes(type, this))
            {
                throw new ArgumentException($"Inheriting loop detected between \"{this}\" and \"{type}\"", nameof(type));
            }

            _baseTypes.Add(type);
        }
        public bool RemoveBaseType(IDSharpType type) => _baseTypes.Remove(type);
        public void ClearBaseTypes() => _baseTypes.Clear();

        public DSharpTypeBuilder CreateGenericType(string name)
        {
            var type = CreateType(name, true);
            _genericTypes.Add(type);

            return type;
        }
        public DSharpMethodBuilder CreateConstructor(bool isStatic = false)
        {
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains constructors");
            }
            if (isStatic && StaticConstructor != null)
            {
                throw new InvalidOperationException($"Type \"{this}\" already contains static constructor");
            }
            if (ObjectType == DSharpObjectType.Interface)
            {
                throw new InvalidOperationException($"Can not create constructor for interface \"{this}\"");
            }

            var constructor = CreateMember(DSharpMetadataTokenType.Method, _constructors, t => DSharpMethodBuilder.CreateConstructor(this, t));
            constructor.IsStatic = isStatic;

            return constructor;
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
        public DSharpMethodBuilder CreateInitializer(bool isStatic)
        {
            if (!isStatic && ObjectType == DSharpObjectType.Interface)
            {
                throw new InvalidOperationException($"Can not create initializer for interface \"{this}\"");
            }
            if (IsGeneric)
            {
                throw new InvalidOperationException("Generic types can not contains initializers");
            }

            var initializer = isStatic ? StaticInitializer : Initializer;

            if (initializer != null)
            {
                throw new InvalidOperationException($"Can not create multiple initializers for \"{this}\"");
            }

            initializer = CreateMember(DSharpMetadataTokenType.Method, _methods, t => DSharpMethodBuilder.CreateInitializer(this, t));
            initializer.IsStatic = isStatic;

            if (isStatic)
            {
                StaticInitializer = initializer;
            }
            else
            {
                Initializer = initializer;
            }

            return initializer;
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
            var type = CreateType(name, false);
            _childrenTypes.Add(type);

            return type;
        }

        public DSharpOperatorBuilder CreateImplicitOperator() => CreateOperator<DSharpBinaryOperator>(DSharpOperatorType.Implicit);
        public DSharpOperatorBuilder CreateExplicitOperator() => CreateOperator<DSharpBinaryOperator>(DSharpOperatorType.Explicit);
        public DSharpOperatorBuilder CreateOperator(DSharpBinaryOperator @operator)
        {
            return CreateOperator<DSharpBinaryOperator>(DSharpOperatorType.Binary, @operator);
        }
        public DSharpOperatorBuilder CreateOperator(DSharpUnaryOperator @operator)
        {
            return CreateOperator<DSharpUnaryOperator>(DSharpOperatorType.Binary, @operator);
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
            if (constructor == StaticConstructor)
            {
                StaticConstructor = null;
            }

            return RemoveMember(_constructors, constructor);
        }
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
        public bool RemoveInitializer(bool isStatic)
        {
            if (!isStatic && Initializer == null ||
                isStatic && StaticInitializer == null)
            {
                return false;
            }

            DSharpMethodBuilder initializer;

            if (isStatic)
            {
                initializer = StaticInitializer!;
                StaticInitializer = null;
            }
            else
            {
                initializer = Initializer!;
                Initializer = null;
            }

            return _methods.Remove(initializer);
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
        public ReadOnlyDictionary<IDSharpType, IDSharpType> GetReplacedTypes()
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

            _replacedTypes = new(replacedTypes);

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
        private DSharpOperatorBuilder CreateOperator<T>(DSharpOperatorType type, T? @operator = null)
            where T : struct, Enum
        {
            var operatorsList = _castOperators;

            if (type != DSharpOperatorType.Implicit &&
                type != DSharpOperatorType.Explicit)
            {
                operatorsList = _operators;
            }

            string name = $"{OperatorPrefixName}{type}";

            if (@operator != null)
            {
                name += $"_{(DSharpTokenType)(object)@operator.Value}";
            }

            var result = CreateMember(DSharpMetadataTokenType.Operator, operatorsList, t => new(this, name, t));
            result.Type = type;

            if (type == DSharpOperatorType.Binary)
            {
                result.BinaryOperator = (DSharpBinaryOperator)(object)@operator.GetValueOrDefault();
            }
            else if (type == DSharpOperatorType.Unary)
            {
                result.UnaryOperator = (DSharpUnaryOperator)(object)@operator.GetValueOrDefault();
            }

            return result;
        }

        #endregion

        #region Получение членов

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpType[] GetBaseTypes() => [.. _baseTypes];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpType[] GetChildrenTypes() => [.. ChildrenTypes];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpType[] GetGenericParameters() => [.. GenericParameters.Select(t => (IDSharpType)Assembly.GetType(t))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpType[] GetGenericTypes() => [.. GenericTypes];

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="predicate"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public IDSharpMethodInfo[] GetMethods(Predicate<IDSharpMethodInfo> predicate) => [.. _methods.Where(m => predicate(m))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="predicate"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public IDSharpPropertyInfo[] GetProperties(Predicate<IDSharpPropertyInfo> predicate) => [.. _properties.Where(p => predicate(p))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="predicate"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public IDSharpIndexerInfo[] GetIndexers(Predicate<IDSharpIndexerInfo> predicate) => [.. _indexers.Where(i => predicate(i))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="predicate"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate) => [.. _fields.Where(f => predicate(f))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="predicate"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate) => [.. _constructors.Where(c => predicate(c))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpOperatorInfo[] GetCastOperators() => [.. _castOperators];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpOperatorInfo[] GetOperators() => [.. _operators];

        #endregion

        #region Константы

        /// <summary>
        /// Name for all constructors
        /// </summary>
        public const string ConstructorName = "ctor";
        /// <summary>
        /// Name for all finalizer methods
        /// </summary>
        public const string FinalizerName = "Finalize";
        /// <summary>
        /// Name for initializer methods
        /// </summary>
        public const string InitializerName = "init";
        /// <summary>
        /// Name for static initializer methods
        /// </summary>
        public const string StaticInitializerName = "init_static";
        /// <summary>
        /// Name for all indexers
        /// </summary>
        public const string IndexerName = "Item";
        /// <summary>
        /// Name prefix for all operators
        /// </summary>
        public const string OperatorPrefixName = "__Operator_";

        #endregion

        #region Статика

        /// <summary>
        /// Check containing base type by specified type
        /// </summary>
        /// <param name="type">Type to checking for containing provided base type</param>
        /// <param name="baseType">Base type to check for containing</param>
        /// <returns>Is base type contains in specified type</returns>
        public static bool ContainsInBaseTypes(IDSharpType type, IDSharpType baseType)
        {
            bool Contains(IDSharpType type)
            {
                foreach (var bType in type.GetBaseTypes())
                {
                    if (bType == baseType || Contains(bType))
                    {
                        return true;
                    }
                }

                return false;
            }

            return Contains(type);
        }

        #endregion
    }
}
