using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public partial class DSharpCompiler
    {
        public DSharpCompiler(DSharpAssemblyBuilder assemblyBuilder)
        {
            _assemblyBuilder = assemblyBuilder;
            _context = new()
            {
                Assembly = _assemblyBuilder,
                Usings = _usings,
                ResolvedTypes = _resolvedTypes
            };
        }

        private readonly DSharpAssemblyBuilder _assemblyBuilder;
        private readonly List<string> _usings = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _createdTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, FieldNode> _createdFields = [];
        private readonly Dictionary<DSharpFieldBuilder, VariableNode> _createdGlobalVariables = [];
        private readonly Dictionary<DSharpPropertyBuilder, FieldNode> _createdProperties = [];
        private readonly Dictionary<DSharpMethodBuilder, MethodNode> _createdMethods = [];
        private readonly Dictionary<DSharpMethodBuilder, FinalizerNode> _createdFinalizers = [];
        private readonly Dictionary<DSharpIndexerBuilder, IndexerNode> _createdIndexers = [];
        private readonly Dictionary<DSharpMethodBuilder, ConstructorNode> _createdConstructors = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _enumTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, LiteralExpressionNode> _enumValues = [];
        private readonly Dictionary<string, DSharpTypeToken> _resolvedTypes = [];

        private string? _currentNamespace;
        private DSharpCompilerContext _context;

        #region Управление

        public void CompileTrees(params IEnumerable<DSharpTreeRoot> treeRoots)
        {
            _resolvedTypes.Clear();
            _createdTypes.Clear();
            _createdFields.Clear();
            _createdProperties.Clear();
            _createdMethods.Clear();
            _enumTypes.Clear();
            _enumValues.Clear();
            _usings.Clear();
            _propertiesWithCustomAccessors.Clear();
            _propertyFields.Clear();
            _currentNamespace = null;

            foreach (var treeRoot in treeRoots)
            {
                ParseStatements(_assemblyBuilder, treeRoot.Statements);
            }

            SetupTypes(_assemblyBuilder);
        }

        #endregion

        #region Парсинг

        private void ParseStatements(DSharpAssemblyBuilder assemblyBuilder, List<StatementNode> statements)
        {
            foreach (var statement in statements)
            {
                ParseStatement(assemblyBuilder, statement);
            }
        }
        private void ParseStatement(DSharpAssemblyBuilder assemblyBuilder, StatementNode statement)
        {
            if (statement is ObjectDeclarationStatementNode declarationNode)
            {
                if (declarationNode.ObjectDeclaration == null)
                {
                    throw new ArgumentException($"Object declaration is null at: {declarationNode}", nameof(statement));
                }

                CreateType(assemblyBuilder, null, declarationNode.ObjectDeclaration);
            }
            else if (statement is EnumStatementNode enumStatement)
            {
                if (enumStatement.Enum == null)
                {
                    throw new ArgumentException($"Enum declaration is null at: {enumStatement}", nameof(statement));
                }
            }
            else if (statement is UsingStatementNode usingStatement)
            {
                if (usingStatement.Identifier == null)
                {
                    throw new ArgumentException($"Using statement must contains namespace identifier: {usingStatement}", nameof(statement));
                }

                _usings.Add(usingStatement.GetNamespace());
            }
            else if (statement is BlockStatementNode block)
            {
                ParseStatements(assemblyBuilder, block.Statements);
            }
            else if (statement is NamespaceBlockStatementBlock namespaceBlockStatement)
            {
                var previousNamespace = _currentNamespace;
                var currentNamespace = namespaceBlockStatement.GetName();

                if (previousNamespace != null)
                {
                    _currentNamespace += "." + currentNamespace;
                }
                else
                {
                    _currentNamespace = currentNamespace;
                }

                if (namespaceBlockStatement.Block?.Statements != null)
                {
                    ParseStatements(assemblyBuilder, namespaceBlockStatement.Block.Statements);
                }

                _currentNamespace = previousNamespace;
            }
            else if (statement is NamespaceStatementNode namespaceStatement)
            {
                _currentNamespace = namespaceStatement.GetName();
            }
            else if (statement is InvokableStatementNode invokableStatement)
            {
                if (invokableStatement.Invokable == null)
                {
                    throw new ArgumentException($"Invokable statement must provide invokable node: {invokableStatement}", nameof(statement));
                }
                if (invokableStatement.Invokable is not MethodNode methodNode)
                {
                    throw new ArgumentException($"Only global functions can contains in root of script");
                }

                CreateMethod(assemblyBuilder, null, methodNode);
            }
            else if (statement is VariableStatementNode variableStatement)
            {
                if (variableStatement.Variable == null)
                {
                    throw new ArgumentException($"Variable statement must provide variable node: {variableStatement}", nameof(statement));
                }
                if (variableStatement.Variable is FieldNode field)
                {
                    CreateField(assemblyBuilder, null, field);
                    return;
                }

                CreateGlobalVariable(assemblyBuilder, variableStatement.Variable);
            }
        }

        #endregion

        #region Создание типов

        private void CreateType(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? parent, ObjectDeclarationNode declaration)
        {
            if (declaration.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of object: {declaration}", nameof(declaration));
            }
            if (declaration.IsAbstract && declaration.IsStatic)
            {
                throw new ArgumentException($"Object can not be abstract and static: {declaration}", nameof(declaration));
            }
            if (parent == null && declaration.Access != DSharpAccessModifier.Public)
            {
                throw new ArgumentException($"Objects in namespace only can be public: {declaration}", nameof(declaration));
            }

            var type = assemblyBuilder.CreateType(declaration.Identifier.Name, parent);
            type.IsStatic = declaration.IsStatic;
            type.IsAbstract = declaration.IsAbstract;
            type.ObjectType = declaration.Type;
            type.Access = declaration.Access;
            type.Namespace = _currentNamespace;

            void CreateGenerics(DSharpTypeBuilder builder, IEnumerable<TypeInfoNode> types)
            {
                foreach (var type in types)
                {
                    var genericType = builder.CreateGenericType(type.Name);
                    CreateGenerics(genericType, type.GenericParameters);
                }
            }

            CreateGenerics(type, declaration.Identifier.GenericParameters);

            foreach (var childType in declaration.Children)
            {
                CreateType(assemblyBuilder, type, childType);
            }
            foreach (var childEnum in declaration.ChildrenEnums)
            {
                CreateEnum(assemblyBuilder, type, childEnum);
            }
            foreach (var field in declaration.Fields)
            {
                CreateFieldOrProperty(assemblyBuilder, type, field);
            }
            foreach (var indexer in declaration.Indexers)
            {
                CreateIndexer(assemblyBuilder, type, indexer);
            }
            foreach (var method in declaration.Methods)
            {
                CreateMethod(assemblyBuilder, type, method);
            }
            foreach (var finalizer in declaration.Finalizers)
            {
                CreateFinalizer(assemblyBuilder, type, finalizer);
            }
            foreach (var constructor in declaration.Constructors)
            {
                CreateConstructor(assemblyBuilder, type, constructor);
            }

            _createdTypes.Add(type, declaration);
        }
        private void CreateFieldOrProperty(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            if (fieldNode.CustomGetterSetter)
            {
                CreateProperty(assemblyBuilder, declareType, fieldNode);
                return;
            }

            CreateField(assemblyBuilder, declareType, fieldNode);
        }
        private void CreateGlobalVariable(DSharpAssemblyBuilder assemblyBuilder, VariableNode variableNode)
        {
            var variable = assemblyBuilder.CreateGlobalVariable(variableNode.Name);
            variable.Namespace = _currentNamespace;
            variable.Access = DSharpAccessModifier.Public;
            variable.IsStatic = true;

            if (variableNode.Initializer?.TrySimplifyToLiteral(out var rawValue) == true)
            {
                variable.RawValue = rawValue;
            }

            _createdGlobalVariables.Add(variable, variableNode);
        }
        private void CreateField(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            if (fieldNode.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of field: {fieldNode}", nameof(fieldNode));
            }
            if (declareType == null)
            {
                throw new ArgumentException($"Field must have a type that declares it", nameof(declareType));
            }

            DSharpFieldBuilder field = declareType.CreateField(fieldNode.Identifier.GetName(false));
            field.Access = fieldNode.Access;
            field.IsStatic = fieldNode.IsStatic;
            field.IsReadOnly = fieldNode.IsReadOnly;

            if (fieldNode.Initializer?.TrySimplifyToLiteral(out var rawValue) == true)
            {
                field.RawValue = rawValue;
            }

            _createdFields.Add(field, fieldNode);
        }
        private void CreateProperty(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            var property = CreateProperty(assemblyBuilder, declareType, fieldNode, (t, n) => t.CreateProperty(n));
            _createdProperties.Add(property, fieldNode);
        }
        private void CreateMethod(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, MethodNode methodNode)
        {
            if (methodNode.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of method: {methodNode}", nameof(methodNode));
            }
            if ((methodNode.IsExtern || methodNode.IsStatic) &&
                ((methodNode.Body != null && methodNode.IsExtern) ||
                methodNode.Mode != DSharpObjectMemberMode.Default ||
                methodNode.IsSealed ||
                methodNode.IsOverride))
            {
                if (methodNode.IsExtern)
                {
                    throw new ArgumentException($"Extern method can be only static or instance: {methodNode}", nameof(methodNode));
                }
                else
                {
                    throw new ArgumentException($"Static method can not be virtual, abstract, sealed and overriden: {methodNode}", nameof(methodNode));
                }
            }
            if ((!methodNode.IsExtern && declareType?.ObjectType != DSharpObjectType.Interface) && methodNode.Body == null)
            {
                throw new ArgumentException($"Method must have a body: {methodNode}", nameof(methodNode));
            }
            if (methodNode.Identifier.Name == DSharpTypeBuilder.FinalizerName)
            {
                throw new ArgumentException($"Invalid identifier of method, name \"{methodNode.Identifier.Name}\" was reserved for finalizer/destructor: {methodNode}", nameof(methodNode));
            }

            DSharpMethodBuilder method;

            if (declareType == null)
            {
                if (methodNode.Identifier.GenericParameters.Count != 0)
                {
                    throw new ArgumentException("Global function can not contains generic parameters");
                }

                method = assemblyBuilder.CreateGlobalFunction(methodNode.Identifier.Name);
                method.Namespace = _currentNamespace;
            }
            else
            {
                method = declareType.CreateMethod(methodNode.Identifier.Name);
            }

            method.IsExtern = methodNode.IsExtern;
            method.IsStatic = methodNode.IsStatic || declareType == null;
            method.IsSealed = methodNode.IsSealed;
            method.IsAbstract = methodNode.Mode == DSharpObjectMemberMode.Abstract;
            method.IsVirtual = methodNode.Mode == DSharpObjectMemberMode.Virtual;

            if (declareType == null)
            {
                method.Access = DSharpAccessModifier.Public;
            }
            else
            {
                method.Access = methodNode.Access;
            }

            _createdMethods.Add(method, methodNode);
        }
        private void CreateFinalizer(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder declareType, FinalizerNode finalizerNode)
        {
            if (finalizerNode.Parameters.Count > 0)
            {
                throw new ArgumentException($"Finalizers not supports parameters: {finalizerNode}", nameof(finalizerNode));
            }
            if (finalizerNode.Identifier == null)
            {
                throw new ArgumentException($"Finalizer should have identifier: {finalizerNode}", nameof(finalizerNode));
            }
            if (finalizerNode.Identifier.Name != declareType.Name)
            {
                throw new ArgumentException($"Finalizer identifier should be same to type that it contains ({declareType}): {finalizerNode}", nameof(finalizerNode));
            }

            DSharpMethodBuilder finalizer;

            try
            {
                finalizer = declareType.CreateFinalizer();
            }
            catch (Exception error)
            {
                throw new InvalidOperationException($"Unable to create finalizer: {finalizerNode}", error);
            }

            _createdFinalizers.Add(finalizer, finalizerNode);
        }
        private void CreateIndexer(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, IndexerNode indexerNode)
        {
            if (indexerNode.Parameters.Count == 0)
            {
                throw new ArgumentException($"Indexers must contains at least one parameters: {indexerNode}", nameof(indexerNode));
            }
            if (indexerNode.IsStatic)
            {
                throw new ArgumentException($"Indexers can not be static: {indexerNode}", nameof(indexerNode));
            }

            var indexer = CreateProperty(assemblyBuilder, declareType, indexerNode, (t, n) => t.CreateIndexer());
            _createdIndexers.Add(indexer, indexerNode);
        }
        private void CreateConstructor(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, ConstructorNode constructorNode)
        {
            if (declareType == null)
            {
                throw new ArgumentException($"Unable to create constructor when type that should be constructed not provided", nameof(declareType));
            }

            var constructor = declareType.CreateConstructor();
            constructor.Access = constructorNode.Access;
            _createdConstructors.Add(constructor, constructorNode);
        }
        private void CreateEnum(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? parent, EnumNode enumNode)
        {
            var type = assemblyBuilder.CreateType(enumNode.Name, parent);
            type.ObjectType = DSharpObjectType.Struct;
            type.Name = enumNode.Name;
            type.Namespace = _currentNamespace;

            foreach (var enumValue in enumNode.Members)
            {
                var field = type.CreateField(enumValue.Name);

                if (enumValue.Type != DSharpLiteralType.Number)
                {
                    throw new InvalidDataException($"Enum value must be a number, got: {enumValue.Value}");
                }

                field.IsReadOnly = true;
                field.RawValue = enumValue.Value;
            }
        }

        private T CreateProperty<T>(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, FieldNode fieldNode, Func<DSharpTypeBuilder, string, T> fabric)
            where T : DSharpPropertyBuilder
        {
            if (fieldNode.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of property: {fieldNode}", nameof(fieldNode));
            }
            if (declareType == null)
            {
                throw new ArgumentException($"Property must have a type that declares it", nameof(declareType));
            }

            var property = fabric(declareType, fieldNode.Identifier.GetName(false));
            property.Access = fieldNode.Access;
            property.IsStatic = fieldNode.IsStatic;
            property.IsSealed = fieldNode.IsSealed;
            property.IsAbstract = fieldNode.Mode == DSharpObjectMemberMode.Abstract;
            property.IsVirtual = fieldNode.Mode == DSharpObjectMemberMode.Virtual;

            if (fieldNode.CanRead &&
                ((declareType?.ObjectType == DSharpObjectType.Interface && fieldNode.Getter != null) ||
                 declareType?.ObjectType != DSharpObjectType.Interface))
            {
                var getter = property.CreateGetter();
                getter.Access = fieldNode.GetterAccess;
            }
            if (fieldNode.CanWrite &&
                ((declareType?.ObjectType == DSharpObjectType.Interface && fieldNode.Setter != null) ||
                 declareType?.ObjectType != DSharpObjectType.Interface))
            {
                var setter = property.CreateSetter();
                setter.Access = fieldNode.SetterAccess;
            }

            return property;
        }

        #endregion

        #region Разрешение типов

        private void SetupTypes(DSharpAssemblyBuilder assemblyBuilder)
        {
            T? FindBaseMember<T>(Func<IDSharpType, T> selector, IDSharpType type, Predicate<T>? extraPredicate = null)
                where T : IDSharpMemberInfo
            {
                var member = selector(type);

                if (member != null && extraPredicate?.Invoke(member) == true)
                {
                    member = default;
                }
                if (member == null && type != _assemblyBuilder.ObjectType)
                {
                    var baseTypes = type.GetBaseTypes();

                    if (baseTypes.Length != 0)
                    {
                        foreach (var baseType in type.GetBaseTypes())
                        {
                            if (baseType.ObjectType == DSharpObjectType.Interface)
                            {
                                continue;
                            }

                            member = FindBaseMember(selector, baseType, extraPredicate);

                            if (member != null)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        member = FindBaseMember(selector, _assemblyBuilder.ObjectType, extraPredicate);
                    }
                }

                return member;
            }
            void ResolveParameters(List<VariableNode> variables, IList<DSharpMethodBuilderParameter> parameters, DSharpCompilerContext context)
            {
                foreach (var parameter in variables)
                {
                    if (parameter.Type == null)
                    {
                        throw new InvalidOperationException($"Parameter must have a type: {parameter}");
                    }

                    parameters.Add(new(assemblyBuilder)
                    {
                        Name = parameter.Name,
                        TypeGetter = () => context.ResolveType(parameter.Type)
                    });
                }
            }
            void OverrideProperty(DSharpPropertyBuilder property, FieldNode node, string memberName, Func<IDSharpType, IDSharpPropertyInfo> selector)
            {
                if (!node.IsOverride)
                {
                    return;
                }
                
                var overrideProperty = FindBaseMember(selector, property.DeclaringType, m => m == property);

                if (overrideProperty == null)
                {
                    throw new InvalidOperationException($"Unable to override {memberName} \"{property}\" because property with same signature not found in parents: {node}");
                }
                if (overrideProperty.IsSealed)
                {
                    throw new InvalidOperationException($"Unable to override sealed {memberName} \"{overrideProperty}\" by \"{property}\"");
                }

                property.OverrideProperty = overrideProperty;
            }

            foreach (var enumValue in _enumValues.Keys)
            {
                enumValue.FieldType = _assemblyBuilder.NumberToken;
            }
            foreach (var info in _createdGlobalVariables)
            {
                if (info.Value.Type != null)
                {
                    info.Key.FieldTypeResolver = () => ResolveType(info.Key, info.Value.Type);
                }
            }
            foreach (var info in _createdFields)
            {
                if (info.Value.Type != null)
                {
                    info.Key.FieldTypeResolver = () => ResolveType(info.Key, info.Value.Type);
                }
            }
            foreach (var info in _createdProperties)
            {
                if (info.Value.Type != null)
                {
                    info.Key.PropertyTypeResolver = () => ResolveType(info.Key, info.Value.Type);
                }
            }
            foreach (var info in _createdIndexers)
            {
                var context = _context;
                context.CurrentMember = info.Key;

                if (info.Value.Type != null)
                {
                    info.Key.PropertyTypeResolver = () => ResolveType(info.Key, info.Value.Type);
                }

                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);
            }
            foreach (var info in _createdMethods)
            {
                var context = _context;
                context.CurrentMember = info.Key;

                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);

                if (info.Value.ReturnType != null && info.Value.ReturnType.Name != "func")
                {
                    info.Key.ReturnTypeResolver = () => context.ResolveType(info.Value.ReturnType);
                }
            }
            foreach (var info in _createdConstructors)
            {
                var context = _context;
                context.CurrentMember = info.Key;

                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);
            }
            foreach (var enumType in _enumTypes.Keys)
            {
                enumType.BaseTypes.Add(assemblyBuilder.EnumToken);
            }
            foreach (var info in _createdTypes)
            {
                foreach (var baseType in info.Value.BaseTypes)
                {
                    var typeToken = ResolveType(info.Key, baseType);

                    if (typeToken == info.Key.MetadataToken)
                    {
                        throw new InvalidOperationException($"Type can not inherit itself \"{info.Key}\": {baseType}");
                    }

                    info.Key.BaseTypes.Add(typeToken);
                }
            }

            foreach (var info in _createdProperties)
            {
                OverrideProperty(info.Key, info.Value, "property", t => t.GetProperty(info.Key.Name));
            }
            foreach (var info in _createdIndexers)
            {
                OverrideProperty(info.Key, info.Value, "indexer", t => t.GetIndexer(info.Key.Parameters));
            }
            foreach (var info in _createdMethods)
            {
                if (!info.Value.IsOverride)
                {
                    continue;
                }

                if (info.Key.DeclaringType == null)
                {
                    throw new InvalidOperationException($"Unable to override global function \"{info.Key}\": {info.Value}");
                }

                var parameters = info.Key.GetParameters();
                var genericParameters = info.Key.GetGenericParameters();

                var overrideMethod = FindBaseMember(t => t.GetMethod(info.Key.Name), info.Key.DeclaringType, m =>
                {
                    return m.DeclaringType == info.Key.DeclaringType ||
                           !m.GetParameters().SequenceEqual(parameters) ||
                           !m.GetGenericParameters().SequenceEqual(genericParameters);
                });

                if (overrideMethod == null)
                {
                    throw new InvalidOperationException($"Unable to override method \"{info.Key}\" because method with same signature not found in parents: {info.Value}");
                }
                if (overrideMethod.IsSealed)
                {
                    throw new InvalidOperationException($"Unable to override sealed method \"{overrideMethod}\" by \"{info.Key}\"");
                }

                info.Key.OverrideMethod = overrideMethod;
            }

            foreach (var info in _createdProperties)
            {
                CompileProperty(info.Key, info.Value);
            }
            foreach (var info in _createdIndexers)
            {
                CompileProperty(info.Key, info.Value);
            }
            foreach (var info in _createdMethods)
            {
                CompileMethod(info.Key, info.Value);
            }
            foreach (var info in _createdConstructors)
            {
                CompileMethod(info.Key, info.Value);
            }
            foreach (var info in _createdFinalizers)
            {
                CompileMethod(info.Key, info.Value);
            }

            foreach (var info in _createdTypes)
            {
                if (info.Key.ObjectType == DSharpObjectType.Interface)
                {
                    ValidateInterface(info.Key);
                }
                else if (info.Key.BaseTypes.Count > 0)
                {
                    foreach (var baseTypeToken in info.Key.BaseTypes)
                    {
                        var baseType = (IDSharpType)_assemblyBuilder.GetType(baseTypeToken);

                        if (baseType.ObjectType == DSharpObjectType.Interface)
                        {
                            CheckInterfaceImplementation(info.Key, baseType);
                        }
                    }
                }
            }

            int i = 0;

            while (i < _assemblyBuilder.Types.Count)
            {
                var type = _assemblyBuilder.Types[i];
                type.Update();
                i++;
            }
        }

        private DSharpTypeToken ResolveType(DSharpMemberInfoBuilder member, TypeInfoNode typeInfo)
        {
            var context = _context;
            context.CurrentMember = member;
            var typeToken = context.ResolveType(typeInfo);
            var type = _assemblyBuilder.GetType(typeToken);

            if (!context.CanAccessTo(type))
            {
                context.ThrowCanNotAccessException(type);
            }

            return typeToken;
        }

        #endregion
    }
}
