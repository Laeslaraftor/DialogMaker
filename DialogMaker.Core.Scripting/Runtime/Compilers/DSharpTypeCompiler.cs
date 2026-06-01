using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Xml.Linq;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public class DSharpTypeCompiler
    {
        private NamespaceStatementNode? _currentNamespace;
        private readonly List<string> _usings = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _createdTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, FieldNode> _createdFields = [];
        private readonly Dictionary<DSharpPropertyBuilder, FieldNode> _createdProperties = [];
        private readonly Dictionary<DSharpMethodBuilder, MethodNode> _createdMethods = [];
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _enumTypes = [];
        private readonly Dictionary<DSharpFieldBuilder, LiteralExpressionNode> _enumValues = [];

        #region Управление

        public void CompileTypes(DSharpAssemblyBuilder assemblyBuilder, DSharpTreeRoot treeRoot)
        {
            _createdTypes.Clear();
            _createdFields.Clear();
            _createdProperties.Clear();
            _createdMethods.Clear();
            _enumTypes.Clear();
            _enumValues.Clear();
            _usings.Clear();
            _currentNamespace = null;

            ParseStatements(assemblyBuilder, treeRoot.Statements);
            SetupBaseTypes(assemblyBuilder);

            foreach (var enumValue in _enumValues.Keys)
            {
                enumValue.FieldType = assemblyBuilder.NumberType;
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
                var previousNamespace = _currentNamespace;
                _currentNamespace = namespaceBlockStatement;

                if (namespaceBlockStatement.Block?.Statements != null)
                {
                    ParseStatements(assemblyBuilder, namespaceBlockStatement.Block.Statements);
                }

                _currentNamespace = previousNamespace;
            }
            else if (statement is NamespaceStatementNode namespaceStatement)
            {
                _currentNamespace = namespaceStatement;
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
            type.Namespace = _currentNamespace?.Identifier?.Name;

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
                method = assemblyBuilder.CreateGlobalFunction(methodNode.Identifier.Name);
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

        }
        private void CreateEnum(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? parent, EnumNode enumNode)
        {
            var type = assemblyBuilder.CreateType(enumNode.Name, parent);
            type.Name = enumNode.Name;
            type.Namespace = _currentNamespace?.Name;

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
        }

        private DSharpTypeToken ResolveType(DSharpAssemblyBuilder assemblyBuilder, string? @namespace, TypeInfoNode typeInfo)
        {
            string typeName = typeInfo.GetFullName(true, false);
            var resolve1 = TryResolveType(assemblyBuilder, @namespace, typeName);

            foreach (var @using in _usings)
            {
                var usingResolve = TryResolveType(assemblyBuilder, @using, typeName);
            }

            throw new ArgumentException($"Unable to resolve type: {typeName}", nameof(typeInfo));
        }
        private DSharpTypeToken? TryResolveType(DSharpAssemblyBuilder assemblyBuilder, string? @namespace, string typeName)
        {
            if (assemblyBuilder.TryGetTypeToken(typeName, out var token1))
            {
                return token1;
            }
            if (@namespace == null)
            {
                return null;
            }

            var fullName = $"{@namespace}.{typeName}";

            if (assemblyBuilder.TryGetTypeToken(fullName, out var token2))
            {
                return token2;
            }

            return null;
        }

        #endregion
    }
}
