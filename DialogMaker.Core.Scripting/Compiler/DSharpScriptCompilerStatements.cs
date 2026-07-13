using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        internal event EventHandler<TypeSetupRequestEventArgs>? TypeToSetupRequested;

        public ReferenceReadOnlyList<DSharpTypeBuilder> Types
        {
            get
            {
                field ??= new(_types);
                return field;
            }
        }

        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _createdTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, FieldNode> _createdFields = [];
        private readonly Dictionary<DSharpFieldBuilder, ExpressionNode> _createdGlobalVariablesRawValueExpressions = [];
        private readonly Dictionary<DSharpFieldBuilder, VariableNode> _createdGlobalVariables = [];
        private readonly Dictionary<DSharpPropertyBuilder, FieldNode> _createdProperties = [];
        private readonly Dictionary<DSharpMethodBuilder, MethodNode> _createdMethods = [];
        private readonly Dictionary<DSharpMethodBuilder, FinalizerNode> _createdFinalizers = [];
        private readonly Dictionary<DSharpIndexerBuilder, IndexerNode> _createdIndexers = [];
        private readonly Dictionary<DSharpOperatorBuilder, OperatorNode> _createdOperators = [];
        private readonly Dictionary<DSharpMethodBuilder, ConstructorNode> _createdConstructors = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _enumTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, LiteralExpressionNode> _enumValues = [];
        private readonly List<DSharpTypeBuilder> _types = [];
        private Dictionary<IDSharpType, ObjectDeclarationNode>? _typesToSetupBases;
        private string? _currentNamespace = null;

        #region Statements

        private void ParseStatements(IEnumerable<StatementNode> statements)
        {
            foreach (var statement in statements)
            {
                ParseStatement(statement);
            }
        }
        private void ParseStatement(StatementNode statement)
        {
            void AddNamespace(string @namespace)
            {
                var parts = @namespace.Split('.');
                string currentNamespace = string.Empty;
                var namespaces = Scope.Namespaces;

                foreach (var part in parts)
                {
                    if (!string.IsNullOrEmpty(currentNamespace))
                    {
                        currentNamespace += ".";
                    }

                    currentNamespace += part;

                    if (!namespaces.Contains(currentNamespace))
                    {
                        namespaces.Add(currentNamespace);
                    }
                }
            }

            if (statement is ObjectDeclarationStatementNode declarationNode)
            {
                if (declarationNode.ObjectDeclaration == null)
                {
                    throw new ArgumentException($"Object declaration is null at: {declarationNode}", nameof(statement));
                }

                CreateType(null, declarationNode.ObjectDeclaration);
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

                AddNamespace(usingStatement.GetNamespace());
            }
            else if (statement is BlockStatementNode block)
            {
                ParseStatements(block.Statements);
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

                AddNamespace(_currentNamespace);

                if (namespaceBlockStatement.Block?.Statements != null)
                {
                    ParseStatements(namespaceBlockStatement.Block.Statements);
                }

                _currentNamespace = previousNamespace;
            }
            else if (statement is NamespaceStatementNode namespaceStatement)
            {
                _currentNamespace = namespaceStatement.GetName();
                AddNamespace(_currentNamespace);
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

                CreateMethod(null, methodNode);
            }
            else if (statement is VariableStatementNode variableStatement)
            {
                if (variableStatement.Variable == null)
                {
                    throw new ArgumentException($"Variable statement must provide variable node: {variableStatement}", nameof(statement));
                }

                CreateGlobalVariable(variableStatement.Variable);
            }
        }

        #endregion

        #region Creating type and members

        private void CreateType(DSharpTypeBuilder? parent, ObjectDeclarationNode declaration)
        {
            if (declaration.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of object: {declaration}", nameof(declaration));
            }
            if (declaration.IsAbstract && declaration.IsStatic)
            {
                throw new ArgumentException($"Object can not be abstract and static: {declaration}", nameof(declaration));
            }
            if (parent == null && declaration.Access != DSharpAccessModifier.Public &&
                                  declaration.Access != DSharpAccessModifier.Internal)
            {
                throw new ArgumentException($"Objects in namespace only can be public or internal: {declaration}", nameof(declaration));
            }

            var type = Assembly.CreateType(declaration.Identifier.Name, parent);
            type.IsStatic = declaration.IsStatic;
            type.IsAbstract = declaration.IsAbstract;
            type.ObjectType = declaration.Type;
            type.Access = declaration.Access;
            type.IsSealed = declaration.IsSealed;
            type.Namespace = _currentNamespace;

            CreateGenerics(type, declaration.Identifier.GenericParameters);

            foreach (var childType in declaration.Children)
            {
                CreateType(type, childType);
            }
            foreach (var childEnum in declaration.ChildrenEnums)
            {
                CreateEnum(type, childEnum);
            }
            foreach (var field in declaration.Fields)
            {
                CreateFieldOrProperty(type, field);
            }
            foreach (var indexer in declaration.Indexers)
            {
                CreateIndexer(type, indexer);
            }
            foreach (var method in declaration.Methods)
            {
                CreateMethod(type, method);
            }
            foreach (var finalizer in declaration.Finalizers)
            {
                CreateFinalizer(type, finalizer);
            }
            foreach (var @operator in declaration.Operators)
            {
                CreateOperator(type, @operator);
            }
            foreach (var constructor in declaration.Constructors)
            {
                CreateConstructor(type, constructor);
            }

            type.SetupHandler = () =>
            {
                TypeToSetupRequested?.Invoke(type, new(type));
            };

            _createdTypes.Add(type, declaration);
            _types.Add(type);
        }
        private void CreateFieldOrProperty(DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            if (fieldNode.CustomGetterSetter)
            {
                CreateProperty(declareType, fieldNode);
                return;
            }

            CreateField(declareType, fieldNode);
        }
        private void CreateGlobalVariable(VariableNode variableNode)
        {
            var variable = Assembly.CreateGlobalVariable(variableNode.Name);
            variable.Namespace = _currentNamespace;
            variable.Access = DSharpAccessModifier.Public;
            variable.IsStatic = true;

            if (variableNode.Initializer?.TrySimplifyToLiteral(out var rawValue) == true)
            {
                variable.RawValue = rawValue;
                _createdGlobalVariablesRawValueExpressions.Add(variable, variableNode.Initializer);
            }

            _createdGlobalVariables.Add(variable, variableNode);
        }
        private void CreateField(DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            if (fieldNode.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of field: {fieldNode}", nameof(fieldNode));
            }
            if (declareType == null)
            {
                throw new ArgumentException($"Field must have a type that declares it: {fieldNode}", nameof(declareType));
            }

            DSharpFieldBuilder field = declareType.CreateField(fieldNode.Identifier.GetName(false));
            field.Access = fieldNode.Access;
            field.IsStatic = fieldNode.IsStatic;
            field.IsReadOnly = fieldNode.IsReadOnly;

            if (fieldNode.Initializer?.TrySimplifyToLiteral(out var rawValue) == true)
            {
                field.RawValue = rawValue;
            }
            if (fieldNode.Initializer != null)
            {
                CreateInitializerIfNotExists(declareType, field.IsStatic);
            }

            _createdFields.Add(field, fieldNode);
        }
        private void CreateProperty(DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            var property = CreateProperty(declareType, fieldNode, (t, n) => t.CreateProperty(n));

            if (fieldNode.Initializer != null)
            {
                CreateInitializerIfNotExists(property.DeclaringType, property.IsStatic);
            }

            _createdProperties.Add(property, fieldNode);
        }
        private void CreateMethod(DSharpTypeBuilder? declareType, MethodNode methodNode)
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
            if ((!methodNode.IsExtern && methodNode.Mode != DSharpObjectMemberMode.Abstract && declareType?.ObjectType != DSharpObjectType.Interface) && methodNode.Body == null)
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

                method = Assembly.CreateGlobalFunction(methodNode.Identifier.Name);
                method.Namespace = _currentNamespace;
            }
            else
            {
                method = declareType.CreateMethod(methodNode.Identifier.Name);

                foreach (var genericType in methodNode.Identifier.GenericParameters)
                {
                    if (declareType.IsNameExists(genericType.Name))
                    {
                        throw new InvalidOperationException($"Name \"{genericType.Name}\" already exists in current context: {genericType}");
                    }

                    var genericParameter = method.CreateGenericParameter(genericType.Name);
                    CreateGenerics(genericParameter, genericType.GenericParameters);
                }
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
        private void CreateFinalizer(DSharpTypeBuilder declareType, FinalizerNode finalizerNode)
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
        private void CreateIndexer(DSharpTypeBuilder? declareType, IndexerNode indexerNode)
        {
            if (indexerNode.Parameters.Count == 0)
            {
                throw new ArgumentException($"Indexers must contains at least one parameters: {indexerNode}", nameof(indexerNode));
            }
            if (indexerNode.IsStatic)
            {
                throw new ArgumentException($"Indexers can not be static: {indexerNode}", nameof(indexerNode));
            }

            var indexer = CreateProperty(declareType, indexerNode, (t, n) => t.CreateIndexer());
            _createdIndexers.Add(indexer, indexerNode);
        }
        private void CreateOperator(DSharpTypeBuilder declareType, OperatorNode operatorNode)
        {
            if (operatorNode.OperatorType != DSharpOperatorType.Binary &&
                operatorNode.Parameters.Count != 1)
            {
                throw new ArgumentException($"Explicit, implicit and unary operators must contains 1 parameters: {operatorNode}", nameof(operatorNode));
            }
            if (operatorNode.OperatorType == DSharpOperatorType.Binary &&
                operatorNode.Parameters.Count != 2)
            {
                throw new ArgumentException($"Binary operators must contains 2 parameters: {operatorNode}", nameof(operatorNode));
            }
            if (operatorNode.ReturnType == null)
            {
                throw new ArgumentException($"Operators must contains return type: {operatorNode}", nameof(operatorNode));
            }
            if (operatorNode.Body == null)
            {
                throw new ArgumentException($"Operators must contains body: {operatorNode}", nameof(operatorNode));
            }

            DSharpOperatorBuilder @operator;

            if (operatorNode.OperatorType == DSharpOperatorType.Implicit)
            {
                @operator = declareType.CreateImplicitOperator();
            }
            else if (operatorNode.OperatorType == DSharpOperatorType.Explicit)
            {
                @operator = declareType.CreateExplicitOperator();
            }
            else if (operatorNode.OperatorType == DSharpOperatorType.Unary)
            {
                @operator = declareType.CreateOperator(operatorNode.UnaryOperator.GetValueOrDefault());
            }
            else if (operatorNode.OperatorType == DSharpOperatorType.Binary)
            {
                var binaryOperator = operatorNode.BinaryOperator.GetValueOrDefault();

                try
                {
                    if (binaryOperator == DSharpBinaryOperator.LogicalAnd)
                    {
                        throw new InvalidOperationException($"Logical AND operator (&&) is unavailable for overloading: {operatorNode}");
                    }
                    else if (binaryOperator == DSharpBinaryOperator.LogicalOr)
                    {
                        throw new InvalidOperationException($"Logical OR operator (||) is unavailable for overloading: {operatorNode}");
                    }
                }
                catch (Exception error)
                {
                    throw new InvalidOperationException($"Invalid logical operator. For logical operator like && and || use & and |: {operatorNode}", error);
                }

                @operator = declareType.CreateOperator(binaryOperator);
            }
            else
            {
                throw new ArgumentException($"Invalid operator type: {operatorNode}", nameof(operatorNode));
            }

            @operator.Access = operatorNode.Access;
            @operator.Type = operatorNode.OperatorType;
            @operator.BinaryOperator = operatorNode.BinaryOperator;
            @operator.UnaryOperator = operatorNode.UnaryOperator;

            _createdOperators.Add(@operator, operatorNode);
        }
        private void CreateConstructor(DSharpTypeBuilder? declareType, ConstructorNode constructorNode)
        {
            if (declareType == null)
            {
                throw new ArgumentException($"Unable to create constructor when type that should be constructed not provided: {constructorNode}", nameof(declareType));
            }
            if (constructorNode.Name != declareType.Name)
            {
                throw new ArgumentException($"Constructor should have same name to constructing type: {constructorNode}", nameof(constructorNode));
            }

            var constructor = declareType.CreateConstructor(constructorNode.IsStatic);
            constructor.Access = constructorNode.Access;
            _createdConstructors.Add(constructor, constructorNode);
        }
        private void CreateEnum(DSharpTypeBuilder? parent, EnumNode enumNode)
        {
            var type = Assembly.CreateType(enumNode.Name, parent);
            type.ObjectType = DSharpObjectType.Struct;
            type.Name = enumNode.Name;
            type.Namespace = _currentNamespace;

            foreach (var enumValue in enumNode.Members)
            {
                var field = type.CreateField(enumValue.Name);

                if (!enumValue.Value.IsNumber)
                {
                    throw new InvalidDataException($"Enum value must be a number, got: {enumValue.Value}");
                }

                field.IsReadOnly = true;
                field.RawValue = enumValue.Value;
            }
        }
        private void CreateGenerics(DSharpTypeBuilder builder, IEnumerable<TypeInfoNode> types)
        {
            foreach (var type in types)
            {
                var genericType = builder.CreateGenericType(type.Name);
                CreateGenerics(genericType, type.GenericParameters);
            }
        }

        private T CreateProperty<T>(DSharpTypeBuilder? declareType, FieldNode fieldNode, Func<DSharpTypeBuilder, string, T> fabric)
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
            property.CanRead = fieldNode.CanRead;
            property.CanWrite = fieldNode.CanWrite;
            property.GetterAccess = fieldNode.GetterAccess;
            property.SetterAccess = fieldNode.SetterAccess;

            if (!property.IsAbstract)
            {
                if (fieldNode.CanRead && (fieldNode.Getter != null || declareType.ObjectType != DSharpObjectType.Interface))
                {
                    var getter = property.CreateGetter();
                    getter.GetBytecodeBuilder();
                }
                if (fieldNode.CanWrite && (fieldNode.Setter != null || declareType.ObjectType != DSharpObjectType.Interface))
                {
                    var setter = property.CreateSetter();
                    setter.GetBytecodeBuilder();
                }
            }

            return property;
        }
        private void CreateInitializerIfNotExists(DSharpTypeBuilder type, bool isStatic)
        {
            if (isStatic && type.StaticInitializer != null ||
                !isStatic && type.Initializer != null)
            {
                return;
            }

            type.CreateInitializer(isStatic);
        }

        #endregion

        #region Resolving types

        private void ResolveCreatedTypes()
        {
            T? FindBaseMember<T>(Func<IDSharpType, T> selector, IDSharpType type, Predicate<T>? extraPredicate = null)
                where T : IDSharpMemberInfo
            {
                var member = selector(type);

                if (member != null && extraPredicate?.Invoke(member) == true)
                {
                    member = default;
                }
                if (member == null && type != Assembly.ObjectType)
                {
                    var baseTypes = type.GetBaseTypes();

                    if (baseTypes.Length != 0)
                    {
                        foreach (var baseType in baseTypes)
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
                }

                if (member == null && type != Assembly.ObjectType)
                {
                    member = FindBaseMember(selector, Assembly.ObjectType, extraPredicate);
                }

                return member;
            }
            void ResolveParameters(List<VariableNode> variables, IList<DSharpMethodBuilderParameter> parameters, DSharpCompilerContext context)
            {
                parameters.Clear();

                foreach (var parameter in variables)
                {
                    if (parameter.Type == null)
                    {
                        throw new InvalidOperationException($"Parameter must have a type: {parameter}");
                    }

                    parameters.Add(new(Assembly)
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
            void ValidateInvokableNodeParameters(InvokableNode invokable)
            {
                try
                {
                    ValidateParameters(invokable.Parameters);
                }
                catch (Exception error)
                {
                    throw new ArgumentException($"Invalid parameters at: {invokable}", error);
                }
            }
            void ValidateParameters(List<VariableNode> variables)
            {
                int CountNames(string name)
                {
                    int count = 0;

                    foreach (var parameter in variables)
                    {
                        if (parameter.Name == name)
                        {
                            count++;
                        }
                    }

                    return count;
                }

                foreach (var parameter in variables)
                {
                    int namesCount = CountNames(parameter.Name);

                    if (namesCount > 1)
                    {
                        throw new ArgumentException($"Parameter \"{parameter.Name}\" repeat {namesCount} times. Parameter names should be unique");
                    }
                }
            }
            void SetupGenericDescription(DSharpMemberInfoBuilder member, Func<string, DSharpTypeBuilder?> typeSelector, WhereNode genericDescription)
            {
                if (genericDescription.Type == null)
                {
                    throw new InvalidOperationException($"Invalid generic description: {genericDescription}");
                }

                var typeName = genericDescription.Type.Name;
                var genericType = typeSelector(typeName)
                    ?? throw new InvalidOperationException($"Unable to find generic type with name \"{typeName}\": {genericDescription}");

                genericType.GenericAttributes = genericDescription.Attributes;

                foreach (var baseType in genericDescription.BaseTypes)
                {
                    var typeToken = ResolveType(member, baseType);
                    genericType.AddBaseType(typeToken);
                }
            }

            _typesToSetupBases ??= new(_createdTypes.Select(p => new KeyValuePair<IDSharpType, ObjectDeclarationNode>(p.Key, p.Value)));
            var typesToSetupBases = _typesToSetupBases;

            foreach (var enumValue in _enumValues.Keys)
            {
                enumValue.FieldType = Assembly.Int32Token;
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
                DSharpCompilerContext context = new(Context, info.Key);

                if (info.Value.Type != null)
                {
                    info.Key.FieldTypeResolver = () => context.ResolveType(info.Value.Type);
                }
            }
            foreach (var info in _createdProperties)
            {
                DSharpCompilerContext context = new(Context, info.Key);

                if (info.Value.Type != null)
                {
                    info.Key.PropertyTypeResolver = () => context.ResolveType(info.Value.Type);
                }
            }
            foreach (var info in _createdIndexers)
            {
                var context = Context;
                context.CurrentMember = info.Key;
                context.Scope = GetScope(info.Key.DeclaringType);

                if (info.Value.Type != null)
                {
                    info.Key.PropertyTypeResolver = () => context.ResolveType(info.Value.Type);
                }

                try
                {
                    ValidateParameters(info.Value.Parameters);
                }
                catch (Exception error)
                {
                    throw new ArgumentException($"Invalid parameters at: {info.Value}", error);
                }

                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);
            }
            foreach (var info in _createdOperators)
            {
                var context = Context;
                context.CurrentMember = info.Key;
                context.Scope = GetScope(info.Key.DeclaringType);

                if (info.Value.ReturnType != null)
                {
                    info.Key.ReturnTypeResolver = () => ResolveType(info.Key, info.Value.ReturnType);
                }

                ValidateInvokableNodeParameters(info.Value);
                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);
            }
            foreach (var info in _createdMethods)
            {
                var context = Context;
                context.CurrentMember = info.Key;
                context.Scope = GetScope(info.Key);

                ValidateInvokableNodeParameters(info.Value);
                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);

                if (info.Value.ReturnType != null && info.Value.ReturnType.Token.Type != DSharpTokenType.Void)
                {
                    info.Key.ReturnTypeResolver = () => context.ResolveType(info.Value.ReturnType);
                }
            }
            foreach (var info in _createdConstructors)
            {
                var context = Context;
                context.CurrentMember = info.Key;
                context.Scope = GetScope(info.Key.DeclaringType!);

                ValidateInvokableNodeParameters(info.Value);
                ResolveParameters(info.Value.Parameters, info.Key.Parameters, context);
            }
            foreach (var enumType in _enumTypes.Keys)
            {
                enumType.AddBaseType(Assembly.EnumType);
            }

            bool TryGetBaseTypeToSetup(TypeInfoNode typeInfo, out KeyValuePair<IDSharpType, ObjectDeclarationNode> result)
            {
                result = default;

                foreach (var pair in typesToSetupBases)
                {
                    if (pair.Key.Name == typeInfo.Name)
                    {
                        result = pair;
                        return true;
                    }
                }

                return false;
            }
            void SetupBaseTypes(IDSharpType type, ObjectDeclarationNode declaration)
            {
                if (type is not DSharpTypeBuilder typeBuilder)
                {
                    return;
                }

                typesToSetupBases.Remove(type);
                typeBuilder.SetupHandler = null;

                foreach (var baseTypeInfo in declaration.BaseTypes)
                {
                    bool setupCompleted = false;

                    if (TryGetBaseTypeToSetup(baseTypeInfo, out var baseTypePair))
                    {
                        SetupBaseTypes(baseTypePair.Key, baseTypePair.Value);
                        setupCompleted = true;
                    }

                    if (!setupCompleted)
                    {
                        var genericParameters = baseTypeInfo.GenericParameters;

                        if (genericParameters.Count > 0)
                        {
                            baseTypeInfo.GenericParameters = [];
                        }

                        var setupTypeToken = ResolveType(typeBuilder, baseTypeInfo);
                        var baseType = Assembly.GetType(setupTypeToken);

                        if (baseType is DSharpTypeBuilder baseTypeBuilder &&
                            !_createdTypes.ContainsKey(baseTypeBuilder))
                        {
                            TypeToSetupRequested?.Invoke(this, new(baseTypeBuilder));
                        }

                        baseTypeInfo.GenericParameters = genericParameters;
                    }

                    var typeToken = ResolveType(typeBuilder, baseTypeInfo);

                    try
                    {
                        typeBuilder.AddBaseType(typeToken);
                    }
                    catch
                    {
                        typesToSetupBases.Add(type, declaration);
                        throw;
                    }
                }
                foreach (var genericDescription in declaration.GenericDescriptions)
                {
                    SetupGenericDescription(typeBuilder, n => typeBuilder.GenericTypes.FirstOrDefault(t => t.Name == n), genericDescription);
                }
            }

            foreach (var info in _createdMethods)
            {
                if (info.Key.GenericParameters.Count == 0 ||
                    info.Value.GenericDescriptions.Count == 0)
                {
                    continue;
                }

                foreach (var genericParameter in info.Key.GenericParameters)
                {
                    genericParameter.ClearBaseTypes();
                }
                foreach (var genericDescription in info.Value.GenericDescriptions)
                {
                    SetupGenericDescription(info.Key, n => info.Key.GenericParameters.FirstOrDefault(t => t.Name == n), genericDescription);
                }
            }

            while (typesToSetupBases.Count > 0)
            {
                var firstPair = typesToSetupBases.First();
                SetupBaseTypes(firstPair.Key, firstPair.Value);
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
        }
        public partial void ValidateTypes()
        {
            foreach (var info in _createdTypes)
            {
                if (info.Key.ObjectType == DSharpObjectType.Interface)
                {
                    ValidateInterface(info.Key);
                    continue;
                }

                ValidateVirtualization(info.Key);

                if (info.Key.BaseTypes.Count > 0)
                {
                    foreach (var baseType in info.Key.BaseTypes)
                    {
                        if (baseType.ObjectType == DSharpObjectType.Interface)
                        {
                            CheckInterfaceImplementation(info.Key, baseType);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
