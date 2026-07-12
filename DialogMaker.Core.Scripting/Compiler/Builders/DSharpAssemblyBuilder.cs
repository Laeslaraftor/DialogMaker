using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public class DSharpAssemblyBuilder(string name, IList<DSharpAssembly> references) : IDSharpAssembly
    {
        public string Name { get; } = name;
        public ReadOnlyCollection<DSharpAssembly> References { get; } = new(references);
        public ReferenceReadOnlyList<DSharpTypeBuilder> Types
        {
            get
            {
                field ??= new(_types);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpFieldBuilder> GlobalVariables
        {
            get
            {
                field ??= new(_globalVariables);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpMethodBuilder> GlobalFunctions
        {
            get
            {
                field ??= new(_globalFunctions);
                return field;
            }
        }
        IReadOnlyCollection<IDSharpType> IDSharpAssembly.Types => Types;

        #region Встроенные типы

        public DSharpTypeToken Int32Token
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Int);
                return field;
            }
        }
        public DSharpTypeToken UInt32Token
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.UnsignedInt);
                return field;
            }
        }
        public DSharpTypeToken Int64Token
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Long);
                return field;
            }
        }
        public DSharpTypeToken UInt64Token
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.UnsignedLong);
                return field;
            }
        }
        public DSharpTypeToken ShortToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Short);
                return field;
            }
        }
        public DSharpTypeToken UShortToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.UnsignedShort);
                return field;
            }
        }
        public DSharpTypeToken ByteToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Byte);
                return field;
            }
        }
        public DSharpTypeToken SByteToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.SignedByte);
                return field;
            }
        }
        public DSharpTypeToken NIntToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.NativeInt);
                return field;
            }
        }
        public DSharpTypeToken NUIntToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.NativeUnsignedInt);
                return field;
            }
        }
        public DSharpTypeToken DecimalToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Decimal);
                return field;
            }
        }
        public DSharpTypeToken DoubleToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Double);
                return field;
            }
        }
        public DSharpTypeToken SingleToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Single);
                return field;
            }
        }
        public DSharpTypeToken StringToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.String);
                return field;
            }
        }
        public DSharpTypeToken CharToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Char);
                return field;
            }
        }
        public DSharpTypeToken BoolToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Boolean);
                return field;
            }
        }
        public DSharpTypeToken ObjectToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Object);
                return field;
            }
        }
        public DSharpTypeToken EnumToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Extra.Enum);
                return field;
            }
        }
        public DSharpTypeToken ExceptionToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Extra.Exception);
                return field;
            }
        }
        public DSharpTypeToken TypeToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Extra.Type);
                return field;
            }
        }
        public DSharpTypeToken ArrayBaseToken
        {
            get
            {
                field ??= GetTypeToken(ArrayBaseType.Type);
                return field;
            }
        }
        public DSharpTypeToken IEnumeratorToken
        {
            get
            {
                field ??= GetTypeToken(IEnumeratorType.Type);
                return field;
            }
        }
        public DSharpTypeToken NullToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Null);
                return field;
            }
        }
        public DSharpTypeToken NullableToken
        {
            get
            {
                field ??= GetTypeToken(DSharpBuildInTypes.Nullable);
                return field;
            }
        }
        public DSharpTypeToken RuntimeHelperToken
        {
            get
            {
                field ??= GetTypeToken(RuntimeHelperType.Type);
                return field;
            }
        }
        public IDSharpType StringType
        {
            get
            {
                field ??= (IDSharpType)GetType(StringToken);
                return field;
            }
        }
        public IDSharpType Int32Type
        {
            get
            {
                field ??= (IDSharpType)GetType(Int32Token);
                return field;
            }
        }
        public IDSharpType UInt32Type
        {
            get
            {
                field ??= (IDSharpType)GetType(UInt32Token);
                return field;
            }
        }
        public IDSharpType Int64Type
        {
            get
            {
                field ??= (IDSharpType)GetType(Int64Token);
                return field;
            }
        }
        public IDSharpType UInt64Type
        {
            get
            {
                field ??= (IDSharpType)GetType(UInt64Token);
                return field;
            }
        }
        public IDSharpType ShortType
        {
            get
            {
                field ??= (IDSharpType)GetType(ShortToken);
                return field;
            }
        }
        public IDSharpType UShortType
        {
            get
            {
                field ??= (IDSharpType)GetType(UShortToken);
                return field;
            }
        }
        public IDSharpType ByteType
        {
            get
            {
                field ??= (IDSharpType)GetType(ByteToken);
                return field;
            }
        }
        public IDSharpType SByteType
        {
            get
            {
                field ??= (IDSharpType)GetType(SByteToken);
                return field;
            }
        }
        public IDSharpType DecimalType
        {
            get
            {
                field ??= (IDSharpType)GetType(DecimalToken);
                return field;
            }
        }
        public IDSharpType DoubleType
        {
            get
            {
                field ??= (IDSharpType)GetType(DoubleToken);
                return field;
            }
        }
        public IDSharpType SingleType
        {
            get
            {
                field ??= (IDSharpType)GetType(SingleToken);
                return field;
            }
        }
        public IDSharpType NIntType
        {
            get
            {
                field ??= (IDSharpType)GetType(NIntToken);
                return field;
            }
        }
        public IDSharpType NUIntType
        {
            get
            {
                field ??= (IDSharpType)GetType(NUIntToken);
                return field;
            }
        }
        public IDSharpType CharType
        {
            get
            {
                field ??= (IDSharpType)GetType(CharToken);
                return field;
            }
        }
        public IDSharpType BoolType
        {
            get
            {
                field ??= (IDSharpType)GetType(BoolToken);
                return field;
            }
        }
        public IDSharpType ObjectType
        {
            get
            {
                field ??= (IDSharpType)GetType(ObjectToken);
                return field;
            }
        }
        public IDSharpType EnumType
        {
            get
            {
                field ??= (IDSharpType)GetType(EnumToken);
                return field;
            }
        }
        public IDSharpType ExceptionType
        {
            get
            {
                field ??= (IDSharpType)GetType(ExceptionToken);
                return field;
            }
        }
        public IDSharpType TypeType
        {
            get
            {
                field ??= (IDSharpType)GetType(TypeToken);
                return field;
            }
        }
        public IDSharpType NullType
        {
            get
            {
                field ??= (IDSharpType)GetType(NullToken);
                return field;
            }
        }
        public IDSharpType NullableType
        {
            get
            {
                field ??= (IDSharpType)GetType(NullableToken);
                return field;
            }
        }
        public DSharpArrayType ArrayBaseType
        {
            get
            {
                field ??= DSharpArrayType.Create(this);
                return field;
            }
        }
        public DSharpIEnumeratorType IEnumeratorType
        {
            get
            {
                field ??= DSharpIEnumeratorType.Create(this);
                return field;
            }
        }
        public DSharpRuntimeHelperType RuntimeHelperType
        {
            get
            {
                field ??= DSharpRuntimeHelperType.Create(this);
                return field;
            }
        }

        #endregion

        private readonly ObservableCollection<DSharpTypeBuilder> _types = [];
        private readonly ObservableCollection<DSharpFieldBuilder> _globalVariables = [];
        private readonly ObservableCollection<DSharpMethodBuilder> _globalFunctions = [];
        private readonly List<DSharpTypeToken> _typeDefinitions = [];
        private readonly List<DSharpTypeToken> _methodsDefinitions = [];
        private readonly List<DSharpTypeToken> _fieldsDefinitions = [];
        private readonly List<DSharpTypeToken> _propertiesDefinitions = [];
        private readonly List<DSharpTypeToken> _operatorsDefinitions = [];

        #region Управление

        internal DSharpTypeToken AllocateMetadataToken(DSharpMetadataTokenType type)
        {
            List<DSharpTypeToken> tokensBuffer;

            if (type == DSharpMetadataTokenType.TypeDefinition)
            {
                tokensBuffer = _typeDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Property)
            {
                tokensBuffer = _propertiesDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Field)
            {
                tokensBuffer = _fieldsDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Method)
            {
                tokensBuffer = _methodsDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Operator)
            {
                tokensBuffer = _operatorsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Invalid token type: {type}");
            }

            DSharpTypeToken token = new(type, tokensBuffer.Count, 0);
            tokensBuffer.Add(token);

            return token;
        }
        internal bool RemoveMember(DSharpMemberInfoBuilder member)
        {
            List<DSharpTypeToken> tokens;

            if (member.MetadataToken.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                tokens = _typeDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Method)
            {
                tokens = _methodsDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Property)
            {
                tokens = _propertiesDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Field)
            {
                tokens = _fieldsDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Operator)
            {
                tokens = _operatorsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Member with invalid metadata token type: {member.MetadataToken}");
            }

            bool result = RemoveMember(tokens, member.MetadataToken);

            if (result && member is DSharpTypeBuilder typeBuilder)
            {
                _types.Remove(typeBuilder);

                foreach (var type in _types)
                {
                    type.RemoveBaseType(typeBuilder);
                }
            }

            return result;
        }
        internal DSharpTypeBuilder CreateType(string name, bool isGeneric, DSharpTypeBuilder? parent)
        {
            if (isGeneric && parent == null)
            {
                throw new ArgumentException("Parent type should be specified for generic type");
            }

            return CreateMember(DSharpMetadataTokenType.TypeDefinition, _types, t => new(this, isGeneric, parent, name, t));
        }

        public bool RemoveGlobalVariable(DSharpFieldBuilder variable)
        {
            if (_globalVariables.Remove(variable))
            {
                return RemoveMember(_fieldsDefinitions, variable.MetadataToken);
            }

            return false;
        }
        public bool RemoveGlobalFunction(DSharpMethodBuilder function)
        {
            if (_globalFunctions.Remove(function))
            {
                return RemoveMember(_methodsDefinitions, function.MetadataToken);
            }

            return false;
        }
        public bool RemoveType(DSharpTypeBuilder type) => RemoveMember(type);
        public DSharpTypeBuilder CreateType(string name, DSharpTypeBuilder? parent = null)
        {
            if (parent != null)
            {
                return parent.CreateChildType(name);
            }

            return CreateType(name, false, parent);
        }
        public DSharpFieldBuilder CreateGlobalVariable(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Field, _globalVariables, t => new(this, null, name, t));
        }
        public DSharpMethodBuilder CreateGlobalFunction(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Method, _globalFunctions, t => new(this, null, name, t));
        }
        public IDSharpType FillGeneric(IDSharpType genericType, params IEnumerable<IDSharpType> genericParameters)
        {
            return FillGeneric(genericType, null, genericParameters);
        }
        public IDSharpType FillGeneric(IDSharpType genericType, DSharpTypeBuilder? parent, params IEnumerable<IDSharpType> genericParameters)
        {
            if (genericType.GenericTemplate != null)
            {
                genericType = genericType.GenericTemplate;
            }

            if (this.TryFindGenericImplementationType(genericType, out var result, genericParameters))
            {
                return result;
            }

            var genericsCount = genericParameters.Count();
            var genericTypes = genericType.GetGenericTypes();

            if (genericTypes.Length != genericsCount)
            {
                throw new ArgumentException($"Generic parameters count must match to amount of generic types in filling type. Required types {genericTypes.Length} for \"{genericType}\", got {genericsCount}.", nameof(genericParameters));
            }

            Dictionary<IDSharpType, IDSharpType> replacedTypes = [];
            int i = 0;

            foreach (var parameter in genericParameters)
            {
                var genericTypeParameter = genericTypes[i];

                if (!genericTypeParameter.CanReplaceGenericType(parameter))
                {
                    throw new ArgumentException($"Generic type {genericTypeParameter} can not be replaced by {parameter}");
                }

                replacedTypes.Add(genericTypeParameter, parameter);
                i++;
            }

            return ReplaceTypes(genericType, parent, replacedTypes);
        }
        public IDSharpType ReplaceTypes(IDSharpType genericType, DSharpTypeBuilder? parent, IDictionary<IDSharpType, IDSharpType> replacedTypes)
        {
            Dictionary<IDSharpMemberInfo, IDSharpMemberInfo> replacedMembers = [];
            DSharpAssemblyBuilder assemblyBuilder = this;

            if (genericType.Assembly is DSharpAssemblyBuilder genericTypeAssemblyBuilder)
            {
                assemblyBuilder = genericTypeAssemblyBuilder;
            }
            if (genericType is DSharpTypeBuilder genericTypeBuilder &&
                genericTypeBuilder.SetupHandler != null)
            {
                genericTypeBuilder.SetupHandler();
                genericTypeBuilder.SetupHandler = null;
            }

            var newType = assemblyBuilder.CreateType(genericType.Name, parent);
            newType.Access = genericType.Access;
            newType.IsStatic = genericType.IsStatic;
            newType.IsAbstract = genericType.IsAbstract;
            newType.ObjectType = genericType.ObjectType;
            newType.IsSealed = genericType.IsSealed;
            newType.Namespace = genericType.Namespace;
            newType.GenericTemplate = genericType;

            int i = 0;
            var genericTypes = genericType.GetGenericTypes();

            foreach (var parameter in genericTypes)
            {
                if (!replacedTypes.TryGetValue(parameter, out var replacedGenericType))
                {
                    throw new InvalidOperationException($"Type for replacing generic type \"{parameter}\" in \"{genericType}\" not provided");
                }
                if (!parameter.CanReplaceGenericType(replacedGenericType))
                {
                    throw new ArgumentException($"Generic type {parameter} can not be replaced by {replacedGenericType}");
                }

                newType.GenericParameters.Add(GetTypeToken(replacedGenericType));
                replacedMembers.Add(parameter, replacedGenericType);
                i++;
            }
            foreach (var genericTypeBaseType in genericType.GetBaseTypes())
            {
                var currentBaseType = ReplaceGenericParameters(genericTypeBaseType, replacedTypes);
                newType.AddBaseType(GetTypeToken(currentBaseType));
            }
            foreach (var field in genericType.GetFields())
            {
                var newField = newType.CreateField(field.Name);
                newField.IsReadOnly = field.IsReadOnly;
                newField.IsStatic = field.IsStatic;
                newField.Access = field.Access;
                newField.RawValue = field.RawValue;
                newField.OriginalField = field;

                replacedMembers.Add(field, newField);
            }
            foreach (var property in genericType.GetProperties())
            {
                var newProperty = newType.CreateProperty(property.Name);
                SetupProperty(newProperty, property);

                replacedMembers.Add(property, newProperty);
            }
            foreach (var indexer in genericType.GetIndexers())
            {
                var newIndexer = newType.CreateIndexer();
                SetupProperty(newIndexer, indexer);
                SetupParameters(newIndexer.Parameters, indexer.GetParameters());

                replacedMembers.Add(newIndexer, indexer);
            }
            foreach (var @operator in genericType.GetCastOperators())
            {
                DSharpOperatorBuilder newOperator;

                if (@operator.Type == DSharpOperatorType.Implicit)
                {
                    newOperator = newType.CreateImplicitOperator();
                }
                else
                {
                    newOperator = newType.CreateExplicitOperator();
                }

                SetupOperator(newOperator, @operator);

                replacedMembers.Add(newOperator, newOperator);
            }
            foreach (var @operator in genericType.GetOperators())
            {
                DSharpOperatorBuilder newOperator;

                if (@operator.BinaryOperator != null)
                {
                    newOperator = newType.CreateOperator(@operator.BinaryOperator.Value);
                }
                else
                {
                    newOperator = newType.CreateOperator(@operator.UnaryOperator.GetValueOrDefault());
                }

                SetupOperator(newOperator, @operator);

                replacedMembers.Add(newOperator, newOperator);
            }
            foreach (var method in genericType.GetMethods())
            {
                if (method.MethodType != DSharpMethodType.Default)
                {
                    continue;
                }

                var newMethod = newType.CreateMethod(method.Name);
                ProcessMethod(newMethod, method);
            }
            foreach (var constructor in genericType.GetConstructors())
            {
                if (replacedMembers.ContainsKey(constructor))
                {
                    continue;
                }

                var newConstructor = newType.CreateConstructor();
                ProcessMethod(newConstructor, constructor);
            }
            foreach (var childType in genericType.GetChildrenTypes())
            {
                ReplaceTypes(childType, newType, replacedTypes);
            }

            if (genericType.Finalizer != null)
            {
                var newFinalizer = newType.CreateFinalizer();
                ProcessMethod(newFinalizer, genericType.Finalizer);
            }

            void SetupOperator(DSharpOperatorBuilder newOperator, IDSharpOperatorInfo @operator)
            {
                newOperator.Access = @operator.Access;
                newOperator.Type = @operator.Type;
                newOperator.BinaryOperator = @operator.BinaryOperator;
                newOperator.UnaryOperator = @operator.UnaryOperator;
                newOperator.OriginalOperator = @operator;

                ProcessMethod(newOperator.Method, @operator.Method);
            }
            void SetupProperty(DSharpPropertyBuilder newProperty, IDSharpPropertyInfo property)
            {
                newProperty.Access = property.Access;
                newProperty.IsStatic = property.IsStatic;
                newProperty.IsSealed = property.IsSealed;
                newProperty.IsAbstract = property.IsAbstract;
                newProperty.IsVirtual = property.IsVirtual;
                newProperty.OverrideProperty = property.OverrideProperty;
                newProperty.OriginalProperty = property;
                newProperty.CanRead = property.CanRead;
                newProperty.CanWrite = property.CanWrite;
                newProperty.GetterAccess = property.Getter?.Access ?? DSharpAccessModifier.Public;
                newProperty.SetterAccess = property.Setter?.Access ?? DSharpAccessModifier.Public;

                if (property.Getter != null)
                {
                    var getter = newProperty.CreateGetter();
                    ProcessMethod(getter, property.Getter);
                }
                if (property.Setter != null)
                {
                    var setter = newProperty.CreateSetter();
                    ProcessMethod(setter, property.Setter);
                }
            }
            void SetupParameters(IList<DSharpMethodBuilderParameter> newParameters, IDSharpParameterInfo[] parameters)
            {
                foreach (var parameter in parameters)
                {
                    newParameters.Add(new(this)
                    {
                        Name = parameter.Name,
                        Type = GetTypeToken(ReplaceGenericParameters(parameter.Type, replacedTypes))
                    });
                }
            }
            void ProcessMethod(DSharpMethodBuilder newMethod, IDSharpMethodInfo method)
            {
                newMethod.Access = method.Access;
                newMethod.IsStatic = method.IsStatic;
                newMethod.IsAbstract = method.IsAbstract;
                newMethod.IsVirtual = method.IsVirtual;
                newMethod.IsExtern = method.IsExtern;
                newMethod.OriginalMethod = method;

                if (method.ReturnType != null)
                {
                    var newReturnType = ReplaceGenericParameters(method.ReturnType, replacedTypes);
                    newMethod.ReturnType = GetTypeToken(newReturnType);
                }

                newMethod.OverrideMethod = method.OverrideMethod;

                if (newMethod.MethodType != DSharpMethodType.Getter &&
                    newMethod.MethodType != DSharpMethodType.Setter)
                {
                    var genericParameters = method.GetGenericParameters();

                    foreach (var genericParameter in genericParameters)
                    {
                        var newGenericParameter = newMethod.CreateGenericParameter(genericParameter.Name);
                        newGenericParameter.GenericAttributes = genericParameter.GenericAttributes;

                        replacedMembers.Add(genericParameter, newGenericParameter);
                        replacedTypes.Add(genericParameter, newGenericParameter);
                    }
                    foreach (var genericParameter in genericParameters)
                    {
                        if (!replacedTypes.TryGetValue(genericParameter, out var newGenericParameter) ||
                            newGenericParameter is not DSharpTypeBuilder parameterBuilder)
                        {
                            continue;
                        }
                        foreach (var baseType in genericParameter.GetBaseTypes())
                        {
                            var replacedType = ReplaceGenericParameters(baseType, replacedTypes);
                            parameterBuilder.AddBaseType(replacedType);
                        }
                    }


                    SetupParameters(newMethod.Parameters, method.GetParameters());
                }

                replacedMembers.Add(method, newMethod);
            }

            newType._templatedMembers = new ReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo>(replacedMembers);
            newType._replacedTypes = new(replacedTypes);

            return newType;
        }
        public IDSharpType ReplaceGenericParameters(IDSharpType genericType, IDictionary<IDSharpType, IDSharpType> replacedTypes)
        {
            if (replacedTypes.Count == 0)
            {
                return genericType;
            }
            if (replacedTypes.TryGetValue(genericType, out var directReplacedType))
            {
                return directReplacedType;
            }

            var baseTypeGenericParameters = genericType.GetGenericParameters();
            var baseTypeGenericTypes = genericType.GetGenericTypes();

            bool ReplaceTypes(IDSharpType[] types)
            {
                bool replaced = false;

                for (int i = 0; i < types.Length; i++)
                {
                    var parameter = types[i];

                    if (replacedTypes.TryGetValue(parameter, out var replacedType))
                    {
                        types[i] = replacedType;
                        replaced = true;
                    }
                }

                return replaced;
            }

            IDSharpType[] newTypes;

            if (baseTypeGenericParameters.Length > 0)
            {
                newTypes = baseTypeGenericParameters;

                if (!ReplaceTypes(baseTypeGenericParameters))
                {
                    return genericType;
                }
            }
            else if (baseTypeGenericTypes.Length > 0)
            {
                newTypes = baseTypeGenericTypes;

                if (!ReplaceTypes(baseTypeGenericTypes))
                {
                    return genericType;
                }
            }
            else
            {
                return genericType;
            }

            return FillGeneric(genericType, newTypes);
        }
        public IDSharpType CreateArray(IDSharpType elementType)
        {
            return FillGeneric(ArrayBaseType.Type, elementType);
        }
        public IDSharpType CreateNullable(IDSharpType type)
        {
            if (type.IsValueType())
            {
                return FillGeneric(NullableType, type);
            }

            return type;
        }

        private T CreateMember<T>(DSharpMetadataTokenType tokenType, IList<T> members, Func<DSharpTypeToken, T> fabric)
        {
            var token = AllocateMetadataToken(tokenType);
            var type = fabric(token);
            members.Add(type);

            return type;
        }
        private bool RemoveMember(List<DSharpTypeToken> membersToken, DSharpTypeToken token)
        {
            var index = membersToken.IndexOf(token);

            if (index == -1)
            {
                return false;
            }

            membersToken.RemoveAt(index);
            token.Index = -1;

            for (int i = index; i < membersToken.Count; i++)
            {
                membersToken[i].Index--;
            }

            return true;
        }

        #endregion

        #region Поиск

        public DSharpTypeToken GetTypeToken(string fullName)
        {
            if (TryGetTypeToken(fullName, out var token))
            {
                return token;
            }

            throw new ArgumentException($"Unknown type: {fullName}", nameof(fullName));
        }
        public DSharpTypeToken GetTypeToken(IDSharpMemberInfo type)
        {
            if (type is DSharpMemberInfoBuilder builder)
            {
                return builder.MetadataToken;
            }

            int assemblyIndex = References.IndexOf(type.Assembly);

            if (assemblyIndex == -1)
            {
                throw new ArgumentException($"Unable to get type token because specified type contains in assembly that not referenced by builder: {type}", nameof(type));
            }

            return new(type.MetadataToken, assemblyIndex);
        }
        public bool TryGetStandardType(string name, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            if (DSharpBuildInTypes.TryGetTypeInfo(name, out var info) &&
                TryGetTypeToken(info.FullName, out result))
            {
                return true;
            }

            result = null;

            return false;
        }
        public bool TryGetTypeToken(string fullName, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;
            var type = _types.FirstOrDefault(t => t.FullName == fullName);

            if (type != null)
            {
                result = type;
                return true;
            }

            int referenceIndex = 0;

            foreach (var reference in References)
            {
                if (reference.TryGetType(fullName, out var referencedType))
                {
                    result = new(referencedType.MetadataToken, referenceIndex);
                    return true;
                }

                referenceIndex++;
            }

            return false;
        }
        public bool TryGetTypeToken(string? @namespace, string name, List<DSharpTypeToken>? genericTypeTokens, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;
            List<IDSharpType>? genericTypes = null;
            int genericTypesCount = 0;
            var currentFullName = name;
            IDSharpType? foundAssignableTemplate = null;

            if (@namespace != null)
            {
                currentFullName = $"{@namespace}.{name}";
            }

            if (genericTypeTokens != null && genericTypeTokens.Count > 0)
            {
                genericTypes = [];

                foreach (var token in genericTypeTokens)
                {
                    genericTypes.Add((IDSharpType)GetType(token));
                }

                genericTypesCount = genericTypes.Count;
            }

            bool? IsValid(IDSharpType type)
            {
                var fullName = GetFullName(type);

                if (fullName != currentFullName)
                {
                    return false;
                }

                if (type.GenericTemplate != null)
                {
                    var genericParameters = type.GetGenericParameters();

                    if (genericParameters.Length == 0 && genericTypesCount == 0)
                    {
                        return true;
                    }
                    if (genericParameters.Length != genericTypesCount)
                    {
                        return false;
                    }

                    return genericParameters.SequenceEqual(genericTypes);
                }

                var typeGenerics = type.GetGenericTypes();

                if (typeGenerics.Length == 0 && genericTypesCount == 0)
                {
                    return true;
                }
                if (typeGenerics.Length != genericTypesCount)
                {
                    return false;
                }
                if (typeGenerics.SequenceEqual(genericTypes))
                {
                    return true;
                }

                for (int i = 0; i < typeGenerics.Length; i++)
                {
                    if (!typeGenerics[i].CanReplaceGenericType(genericTypes![i]))
                    {
                        throw new ArgumentException($"\"{genericTypes[i]}\" can not replace generic type in \"{type}\" at {i} index");
                    }
                }

                return null;
            }
            string GetFullName(IDSharpType type)
            {
                string name = string.Empty;
                var declaringType = type;

                while (declaringType != null)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        name = declaringType.Name;
                    }
                    else
                    {
                        name = $"{declaringType.Name}.{name}";
                    }

                    if (declaringType.DeclaringType == null)
                    {
                        break;
                    }

                    declaringType = declaringType.DeclaringType;
                }

                if (declaringType?.Namespace != null)
                {
                    name = $"{declaringType.Namespace}.{name}";
                }

                return name;
            }
            DSharpTypeToken? Check(IEnumerable<IDSharpType> types)
            {
                foreach (var type in types)
                {
                    var validateResult = IsValid(type);

                    if (validateResult == false)
                    {
                        continue;
                    }
                    if (validateResult == null)
                    {
                        foundAssignableTemplate = type;
                        continue;
                    }

                    return GetTypeToken(type);
                }

                return null;
            }

            result = Check(_types);

            if (result == null)
            {
                foreach (var reference in References)
                {
                    result = Check(reference.Types);

                    if (result != null)
                    {
                        break;
                    }
                }
            }

            if (result != null)
            {
                return true;
            }
            else if (foundAssignableTemplate != null)
            {
                var newType = FillGeneric(foundAssignableTemplate, genericTypes!);
                result = GetTypeToken(newType);
                return true;
            }

            return false;
        }
        public IDSharpType GetType(string fullName)
        {
            IDSharpType type = _types.FirstOrDefault(t => t.FullName == fullName);

            if (type != null)
            {
                return type;
            }

            foreach (var assembly in References)
            {
                type = assembly.Types.FirstOrDefault(t => t.FullName == fullName);

                if (type != null)
                {
                    return type;
                }
            }

            return type ?? throw new ArgumentException($"Unknown type: {fullName}", nameof(fullName));
        }
        public IDSharpType GetType(DSharpLiteralType literalType)
        {
            return literalType switch
            {
                DSharpLiteralType.Null => NullType,
                DSharpLiteralType.String => StringType,
                DSharpLiteralType.Char => CharType,
                DSharpLiteralType.Bool => BoolType,
                DSharpLiteralType.Int => Int32Type,
                DSharpLiteralType.UInt => UInt32Type,
                DSharpLiteralType.Long => Int64Type,
                DSharpLiteralType.ULong => UInt64Type,
                DSharpLiteralType.Short => ShortType,
                DSharpLiteralType.UShort => UShortType,
                DSharpLiteralType.Byte => ByteType,
                DSharpLiteralType.SByte => SByteType,
                DSharpLiteralType.NInt => NIntType,
                DSharpLiteralType.NUInt => NUIntType,
                DSharpLiteralType.Decimal => DecimalType,
                DSharpLiteralType.Double => DoubleType,
                DSharpLiteralType.Float => SingleType,
                _ => throw new ArgumentException($"Invalid literal type: {literalType}", nameof(literalType))
            };
        }
        public IDSharpMemberInfo? GetTypeOrDefault(DSharpTypeToken? token)
        {
            if (token == null)
            {
                return null;
            }

            return GetType(token);
        }
        public IDSharpMemberInfo GetType(DSharpTypeToken token)
        {
            return GetType((DSharpMetadataToken)token);
        }
        public IDSharpMemberInfo GetType(DSharpMetadataToken metadata)
        {
            if (metadata.AssemblyIndex > 0)
            {
                DSharpMetadataToken originalToken = metadata;
                originalToken = new(metadata, 0);
                return References[metadata.AssemblyIndex - 1].GetType(originalToken);
            }

            if (metadata.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                return _types.First(t => t.MetadataToken == metadata);
            }
            else if (metadata.Type == DSharpMetadataTokenType.Field)
            {
                var globalVariable = _globalVariables.FirstOrDefault(v => v.MetadataToken == metadata);

                if (globalVariable != null)
                {
                    return globalVariable;
                }

                foreach (var type in _types)
                {
                    var field = type.Fields.FirstOrDefault(f => f.MetadataToken == metadata);

                    if (field != null)
                    {
                        return field;
                    }
                }

                throw new ArgumentException($"Unable to find field for token: {metadata}", nameof(metadata));
            }
            else if (metadata.Type == DSharpMetadataTokenType.Method)
            {
                var globalFunction = _globalFunctions.FirstOrDefault(f => f.MetadataToken == metadata);

                if (globalFunction != null)
                {
                    return globalFunction;
                }

                foreach (var type in _types)
                {
                    var method = type.Methods.FirstOrDefault(m => m.MetadataToken == metadata);

                    if (method != null)
                    {
                        return method;
                    }
                }

                throw new ArgumentException($"Unable to find method for token: {metadata}", nameof(metadata));
            }
            else if (metadata.Type == DSharpMetadataTokenType.Property)
            {
                foreach (var type in _types)
                {
                    var property = type.Properties.FirstOrDefault(p => p.MetadataToken == metadata);

                    if (property != null)
                    {
                        return property;
                    }
                }

                throw new ArgumentException($"Unable to find method for token: {metadata}", nameof(metadata));
            }
            else if (metadata.Type == DSharpMetadataTokenType.Operator)
            {
                foreach (var type in _types)
                {
                    var @operator = type.Operators.FirstOrDefault(p => p.MetadataToken == metadata);

                    if (@operator != null)
                    {
                        return @operator;
                    }
                }

                throw new ArgumentException($"Unable to find method for token: {metadata}", nameof(metadata));
            }

            throw new ArgumentException($"Invalid token type: {metadata}", nameof(metadata));
        }
        public IDSharpType GetType(TypeInfoNode typeInfo)
        {
            return GetType(typeInfo.GetFullName(true, false));
        }
        public IDSharpFieldInfo[] GetGlobalVariables() => [.. _globalVariables];
        public IDSharpMethodInfo[] GetGlobalFunctions() => [.. GlobalFunctions];
        public List<IDSharpType> GetTypes(string fullName)
        {
            List<IDSharpType> result = [.. Types.Where(t => t.FullName == fullName)];

            foreach (var reference in References)
            {
                var types = reference.Types.Where(t => t.FullName == fullName);
                result.AddRange(types);
            }

            return result;
        }
        public List<IDSharpType> GetTypes(string? @namespace, string name)
        {
            List<IDSharpType> result = [.. Types.Where(t => t.Namespace == @namespace &&
                                                            t.Name == name)];

            foreach (var reference in References)
            {
                var types = reference.Types.Where(t => t.Namespace == @namespace &&
                                                       t.Name == name);
                result.AddRange(types);
            }

            return result;
        }

        #endregion

        #region Константы

        public const string VarName = "var";

        #endregion
    }
}
