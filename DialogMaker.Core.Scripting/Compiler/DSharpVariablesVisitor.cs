using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Visitor that searches information about captured variables
    /// </summary>
    public class DSharpVariablesVisitor : DSharpScopeVisitor
    {
        /// <summary>
        /// Root scopes and it's captured variables
        /// </summary>
        public Dictionary<Scope, Dictionary<string, Capture>> RootCaptures { get; } = [];

        #region Controls

        public override void StartVisiting(AstNode node)
        {
            base.StartVisiting(node);

            if (NotEmpty)
            {
                return;
            }

            if (node.Parent is InvokableNode invokable)
            {
                foreach (var parameter in invokable.Parameters)
                {
                    CurrentScope?.Variables.Add(parameter.Name, parameter);
                }
            }
            else if (node.Parent is DelegateExpressionNode delegateNode)
            {
                foreach (var parameter in delegateNode.Parameters)
                {
                    CurrentScope?.Variables.Add(parameter.Name, parameter);
                }
            }
        }

        public override void VisitVariableNode(VariableNode node)
        {
            base.VisitVariableNode(node);

            if (!NotEmpty)
            {
                AddVariable(node.Name, node);
            }
        }
        public override void VisitOutExpressionNode(OutExpressionNode node)
        {
            base.VisitOutExpressionNode(node);

            if (node.Identifier != null)
            {
                if (!NotEmpty)
                {
                    if (node.Type != null)
                    {
                        AddVariable(node.Identifier.Name, node.Identifier);
                    }
                }
                else
                {
                    AddIdentifier(node.Identifier.Name, node.Identifier);
                }
            }
        }
        public override void VisitIsTypeExpressionNode(IsTypeExpressionNode node)
        {
            base.VisitIsTypeExpressionNode(node);

            if (!NotEmpty && node.DestinationIdentifier != null)
            {
                AddVariable(node.DestinationIdentifier.Name, node.DestinationIdentifier);
            }
        }
        public override void VisitIdentifierExpressionNode(IdentifierExpressionNode node)
        {
            base.VisitIdentifierExpressionNode(node);

            if (NotEmpty && IsAvailable(node))
            {
                AddIdentifier(node.Name, node);
            }
        }
        public override void VisitThisExpressionNode(ThisExpressionNode node)
        {
            base.VisitThisExpressionNode(node);

            if (NotEmpty)
            {
                AddIdentifier(node.Name, node);
            }
        }

        private void AddIdentifier(string name, AstNode node)
        {
            if (!NotEmpty || CurrentScope == null ||
                !CurrentScope.TryGetVariableScope(name, out var scope) ||
                CurrentScope.ContainsInCurrentScope(name))
            {
                return;
            }

            var root = scope.FindRoot();
            Capture capture;

            if (!RootCaptures.TryGetValue(root, out var captures))
            {
                captures = [];
                RootCaptures.Add(root, captures);
            }
            if (!captures.TryGetValue(name, out capture!))
            {
                capture = new(name, scope.Variables[name]);
                captures.Add(name, capture);
            }
            if (!capture.Scopes.TryGetValue(scope, out var scopeIdentifiers))
            {
                scopeIdentifiers = [];
                capture.Scopes.Add(scope, scopeIdentifiers);
            }

            scopeIdentifiers.Add(node);
        }
        private static bool IsAvailable(AstNode node)
        {
            var parent = node;

            while (parent != null)
            {
                if (parent.Parent is MemberAccessExpressionNode memberAccess &&
                    memberAccess.Member == parent)
                {
                    return false;
                }

                parent = parent.Parent;
            }

            return true;
        }

        #endregion

        #region Classes

        /// <summary>
        /// Variable capturing information
        /// </summary>
        /// <param name="variableName">Name of captured variable</param>
        /// <param name="declaration">Captured variable declaration</param>
        public class Capture(string variableName, AstNode declaration)
        {
            /// <summary>
            /// Name of captured variable
            /// </summary>
            public string VariableName { get; } = variableName;
            /// <summary>
            /// Captured variable declaration
            /// </summary>
            public AstNode Declaration { get; } = declaration;
            /// <summary>
            /// Scopes and it's references to current variable
            /// </summary>
            public Dictionary<Scope, List<AstNode>> Scopes { get; } = [];
        }

        #endregion
    }
}
