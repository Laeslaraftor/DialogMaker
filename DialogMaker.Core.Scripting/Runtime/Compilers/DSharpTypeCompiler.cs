using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public class DSharpTypeCompiler
    {
        private NamespaceStatementNode? _currentNamespace;
        private readonly Dictionary<DSharpTypeBuilder, ObjectDeclarationNode> _createdTypes = [];

        public void CompileTypes(DSharpAssemblyBuilder assemblyBuilder, DSharpTreeRoot treeRoot)
        {
            _createdTypes.Clear();
            _currentNamespace = null;

            ParseStatements(assemblyBuilder, treeRoot.Statements);
            SetupBaseTypes(assemblyBuilder);
        }

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
            type.Namespace = _currentNamespace?.Identifier?.Name;

            foreach (var childType in declaration.Children)
            {
                CreateType(assemblyBuilder, type, childType);
            }
            foreach (var childEnum in declaration.ChildrenEnums)
            {
                CreateEnum(assemblyBuilder, type, childEnum);
            }
        }
        private void CreateEnum(DSharpAssemblyBuilder assemblyBuilder, DSharpTypeBuilder? parent, EnumNode enumNode)
        {

        }
        private void SetupBaseTypes(DSharpAssemblyBuilder assemblyBuilder)
        {

        }
    }
}
