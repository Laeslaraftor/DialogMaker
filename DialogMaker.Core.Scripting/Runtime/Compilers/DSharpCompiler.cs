using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

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
        private DSharpCompilerContext _context;

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

            SetupTypes(_assemblyBuilder);

            foreach (var enumValue in _enumValues.Keys)
            {
                enumValue.FieldType = _assemblyBuilder.NumberToken;
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
            type.ObjectType = (DSharpObjectType)declaration.Type;
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
            variable.Namespace = _currentNamespaceIdentifier;

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
            field.IsReadOnly = field.IsReadOnly;

            if (fieldNode.Initializer?.TrySimplifyToLiteral(out var rawValue) == true)
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
            property.IsAbstract = fieldNode.Mode == DSharpObjectMemberMode.Abstract;
            property.IsVirtual = fieldNode.Mode == DSharpObjectMemberMode.Virtual;

            if (fieldNode.CanRead)
            {
                property.CreateGetter();
            }
            if (fieldNode.CanWrite)
            {
                property.CreateSetter();
            }

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

        private void SetupTypes(DSharpAssemblyBuilder assemblyBuilder)
        {
            T? FindBaseMember<T>(Func<IDSharpType, T> selector, IDSharpType type, Predicate<T>? extraPredicate = null)
                where T : IDSharpMemberInfo
            {
                var member = selector(type);

                if (member == null || extraPredicate?.Invoke(member) == true)
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
                            return member;
                        }
                    }
                }

                return member;
            }

            foreach (var info in _createdGlobalVariables)
            {
                if (info.Value.Type != null)
                {
                    info.Key.FieldType = ResolveType(info.Key, info.Value.Type);
                }
            }
            foreach (var info in _createdFields)
            {
                if (info.Value.Type != null)
                {
                    info.Key.FieldType = ResolveType(info.Key, info.Value.Type);
                }
            }
            foreach (var info in _createdProperties)
            {
                if (info.Value.Type != null)
                {
                    info.Key.PropertyType = ResolveType(info.Key, info.Value.Type);
                }
            }
            foreach (var info in _createdMethods)
            {
                var context = _context;
                context.CurrentMember = info.Key;

                foreach (var parameter in info.Value.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        throw new InvalidOperationException($"Parameter must have a type: {parameter}");
                    }

                    var type = context.ResolveType(parameter.Type);
                    info.Key.Parameters.Add(new(assemblyBuilder)
                    {
                        Name = parameter.Name,
                        Type = type
                    });
                }

                if (info.Value.ReturnType != null && info.Value.ReturnType.Name != "func")
                {
                    info.Key.ReturnType = context.ResolveType(info.Value.ReturnType);
                }
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
                    info.Key.BaseTypes.Add(typeToken);
                }
            }
            
            foreach (var info in _createdProperties)
            {
                if (!info.Value.IsOverride)
                {
                    continue;
                }

                var overrideProperty = FindBaseMember(t => t.GetProperty(info.Key.Name), info.Key.DeclaringType);

                if (overrideProperty == null)
                {
                    throw new InvalidOperationException($"Unable to override property \"{info.Key}\" because property with same signature not found in parents: {info.Value}");
                }

                info.Key.OverrideProperty = overrideProperty;
            }
            foreach (var info in _createdConstructors)
            {
                var context = _context;
                context.CurrentMember = info.Key;

                foreach (var parameter in info.Value.Parameters)
                {
                    if (parameter.Type == null)
                    {
                        throw new InvalidOperationException($"Parameter must have a type: {parameter}");
                    }

                    var type = context.ResolveType(parameter.Type);
                    info.Key.Parameters.Add(new(assemblyBuilder)
                    {
                        Name = parameter.Name,
                        Type = type
                    });
                }
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

                var overrideMethod = FindBaseMember(t => t.GetMethod(info.Key.Name), info.Key.DeclaringType, m =>
                {
                    return m.GetParameters().Length != info.Key.Parameters.Count ||
                           m.GetGenericParameters().Length != info.Key.GenericParameters.Count;
                });

                if (overrideMethod == null)
                {
                    throw new InvalidOperationException($"Unable to override property \"{info.Key}\" because property with same signature not found in parents: {info.Value}");
                }

                info.Key.OverrideMethod = overrideMethod;
            }

            foreach (var info in _createdProperties)
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
        }

        private DSharpTypeToken ResolveType(DSharpMemberInfoBuilder member, TypeInfoNode typeInfo)
        {
            var context = _context;
            context.CurrentMember = member;
            return context.ResolveType(typeInfo);
        }

        #endregion
    }
}
