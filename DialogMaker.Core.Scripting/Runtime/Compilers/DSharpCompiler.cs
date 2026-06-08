using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public partial class DSharpCompiler(DSharpAssemblyBuilder assemblyBuilder)
    {
        private readonly DSharpAssemblyBuilder _assemblyBuilder = assemblyBuilder;
        private readonly List<string> _usings = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _createdTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, FieldNode> _createdFields = [];
        private readonly Dictionary<DSharpFieldBuilder, VariableNode> _createdGlobalVariables = [];
        private readonly Dictionary<DSharpPropertyBuilder, FieldNode> _createdProperties = [];
        private readonly Dictionary<DSharpMethodBuilder, MethodNode> _createdMethods = [];
        private readonly Dictionary<DSharpMethodBuilder, ConstructorNode> _createdConstructors = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _enumTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, LiteralExpressionNode> _enumValues = [];
        private readonly Dictionary<string, DSharpTypeToken> _resolvedTypes = [];

        private NamespaceStatementNode? CurrentNamespace
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    _currentNamespaceIdentifier = value?.Identifier?.GetName(false);
                }
            }
        }

        private string? _currentNamespaceIdentifier;

        #region Управление

        public void CompileTypes(params IEnumerable<DSharpTreeRoot> treeRoots)
        {
            _resolvedTypes.Clear();
            _createdTypes.Clear();
            _createdFields.Clear();
            _createdProperties.Clear();
            _createdMethods.Clear();
            _enumTypes.Clear();
            _enumValues.Clear();
            _usings.Clear();
            CurrentNamespace = null;

            foreach (var treeRoot in treeRoots)
            {
                ParseStatements(_assemblyBuilder, treeRoot.Statements);
            }

            SetupBaseTypes(_assemblyBuilder);

            foreach (var enumValue in _enumValues.Keys)
            {
                enumValue.FieldType = _assemblyBuilder.NumberType;
            }
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
                var previousNamespace = CurrentNamespace;
                CurrentNamespace = namespaceBlockStatement;

                if (namespaceBlockStatement.Block?.Statements != null)
                {
                    ParseStatements(assemblyBuilder, namespaceBlockStatement.Block.Statements);
                }

                CurrentNamespace = previousNamespace;
            }
            else if (statement is NamespaceStatementNode namespaceStatement)
            {
                CurrentNamespace = namespaceStatement;
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

            var type = assemblyBuilder.CreateType(declaration.Identifier.Name, parent);
            type.IsStatic = declaration.IsStatic;
            type.IsAbstract = declaration.IsAbstract;
            type.Type = (DSharpObjectType)declaration.Type;
            type.Access = declaration.Access;
            type.Namespace = CurrentNamespace?.Identifier?.Name;

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
            foreach (var method in declaration.Methods)
            {
                CreateMethod(assemblyBuilder, type, method);
            }
            foreach (var constructor in declaration.Constructors)
            {
                CreateConstructor(assemblyBuilder, type, constructor);
            }
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
            variable.Namespace = _currentNamespaceIdentifier;

            if (TryGetLiteralValue(variableNode.Initializer, out var rawValue))
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
            field.IsReadOnly = field.IsReadOnly;
            
            if (TryGetLiteralValue(fieldNode.Initializer, out var rawValue))
            {
                field.RawValue = rawValue;
            }

            _createdFields.Add(field, fieldNode);
        }
        private void CreateProperty(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? declareType, FieldNode fieldNode)
        {
            if (fieldNode.Identifier == null)
            {
                throw new ArgumentException($"Empty identifier of property: {fieldNode}", nameof(fieldNode));
            }
            if (declareType == null)
            {
                throw new ArgumentException($"Property must have a type that declares it", nameof(declareType));
            }

            var property = declareType.CreateProperty(fieldNode.Identifier.GetName(false));
            property.Access = fieldNode.Access;
            property.IsStatic = fieldNode.IsStatic;
            property.IsSealed = fieldNode.IsSealed;
            property.IsOverride = fieldNode.IsOverride;
            property.IsAbstract = fieldNode.Mode == DSharpObjectMemberMode.Abstract;
            property.IsVirtual = fieldNode.Mode == DSharpObjectMemberMode.Virtual;

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
            if (!methodNode.IsExtern && methodNode.Body == null)
            {
                throw new ArgumentException($"Method must have a body: {methodNode}", nameof(methodNode));
            }

            DSharpMethodBuilder method;

            if (declareType == null)
            {
                if (methodNode.Identifier.GenericParameters.Count != 0)
                {
                    throw new ArgumentException("Global function can not contains generic parameters");
                }

                method = assemblyBuilder.CreateGlobalFunction(methodNode.Identifier.Name);
                method.Namespace = _currentNamespaceIdentifier;
            }
            else
            {
                method = declareType.CreateMethod(methodNode.Identifier.Name);
            }

            method.IsExtern = methodNode.IsExtern;
            method.IsStatic = methodNode.IsStatic;
            method.IsSealed = methodNode.IsSealed;
            method.IsAbstract = methodNode.Mode == DSharpObjectMemberMode.Abstract;
            method.IsVirtual = methodNode.Mode == DSharpObjectMemberMode.Virtual;
            method.Access = methodNode.Access;

            _createdMethods.Add(method, methodNode);
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
            type.Name = enumNode.Name;
            type.Namespace = CurrentNamespace?.Name;

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

        #endregion

        #region Разрешение типов

        private bool TryGetLiteralValue(ExpressionNode? expression, [NotNullWhen(true)] out DSharpLiteralValue result)
        {
            result = DSharpLiteralValue.Null;

            if (expression is LiteralExpressionNode literalExpression)
            {
                result = literalExpression.Value;
                return true;
            }

            return false;
        }

        private void SetupBaseTypes(DSharpAssemblyBuilder assemblyBuilder)
        {
            foreach (var enumType in _enumTypes.Keys)
            {
                enumType.BaseTypes.Add(assemblyBuilder.EnumType);
            }
            foreach (var info in _createdTypes)
            {
                foreach (var baseType in info.Value.BaseTypes)
                {
                    var typeToken = ResolveType(assemblyBuilder, info.Key.Namespace, baseType);
                    info.Key.BaseTypes.Add(typeToken);
                }
            }
            foreach (var info in _createdFields)
            {
                if (info.Value.Type != null)
                {
                    info.Key.FieldType = ResolveType(assemblyBuilder, info.Key.Namespace ?? info.Key.DeclaringType?.Namespace, info.Value.Type);
                }
            }
            foreach (var info in _createdGlobalVariables)
            {
                if (info.Value.Type != null)
                {
                    info.Key.FieldType = ResolveType(assemblyBuilder, info.Key.Namespace, info.Value.Type);
                }
            }
            foreach (var info in _createdProperties)
            {
                if (info.Value.Type != null)
                {
                    info.Key.PropertyType = ResolveType(assemblyBuilder, info.Key.DeclaringType.Namespace, info.Value.Type);
                }

                CompileProperty(info.Key, info.Value);
            }
            foreach (var info in _createdMethods)
            {
                foreach (var parameter in info.Value.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        throw new InvalidOperationException($"Parameter must have a type: {parameter}");
                    }

                    var type = ResolveType(assemblyBuilder, info.Key.DeclaringType?.Namespace, parameter.Type);
                    info.Key.Parameters.Add(new()
                    {
                        Name = parameter.Name,
                        Type = type
                    });
                }

                if (info.Value.ReturnType != null && info.Value.ReturnType.Name != "func")
                {
                    info.Key.ReturnType = ResolveType(assemblyBuilder, info.Key.DeclaringType?.Namespace, info.Value.ReturnType);
                }

                CompileMethod(info.Key, info.Value);
            }
            foreach (var info in _createdConstructors)
            {
                foreach (var parameter in info.Value.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        throw new InvalidOperationException($"Parameter must have a type: {parameter}");
                    }

                    var type = ResolveType(assemblyBuilder, info.Key.DeclaringType?.Namespace, parameter.Type);
                    info.Key.Parameters.Add(new()
                    {
                        Name = parameter.Name,
                        Type = type
                    });
                }
            }
        }

        private DSharpTypeToken ResolveType(DSharpMemberInfoBuilder member, TypeInfoNode typeInfo)
        {
            return ResolveType(member.Assembly, member.DeclaringType?.Namespace, typeInfo);
        }
        private DSharpTypeToken ResolveType(DSharpAssemblyBuilder assemblyBuilder, string? @namespace, TypeInfoNode typeInfo)
        {
            string typeName = typeInfo.GetFullName(true, false);
            var resolvedType = TryResolveType(assemblyBuilder, @namespace, typeName);

            if (resolvedType != null)
            {
                return resolvedType;
            }

            foreach (var @using in _usings)
            {
                resolvedType = TryResolveType(assemblyBuilder, @using, typeName);

                if (resolvedType != null)
                {
                    return resolvedType;
                }
            }

            throw new ArgumentException($"Unable to resolve type: {typeName}", nameof(typeInfo));
        }
        private DSharpTypeToken? TryResolveType(DSharpAssemblyBuilder assemblyBuilder, string? @namespace, string typeName)
        {
            var fullName = $"{@namespace}.{typeName}";

            if (_resolvedTypes.TryGetValue(typeName, out var token) ||
                _resolvedTypes.TryGetValue(fullName, out token))
            {
                return token;
            }
            if (assemblyBuilder.TryGetStandardType(typeName, out token) ||
                assemblyBuilder.TryGetTypeToken(typeName, out token))
            {
                _resolvedTypes.Add(typeName, token);
                return token;
            }
            if (@namespace == null)
            {
                return null;
            }
            if (assemblyBuilder.TryGetTypeToken(fullName, out token))
            {
                _resolvedTypes.Add(fullName, token);
                return token;
            }

            return null;
        }

        #endregion
    }
}
