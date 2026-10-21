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

        /// <summary>
        /// Count captures for same variable
        /// </summary>
        /// <param name="variableCapture">Variable capture for counting other capturing</param>
        /// <returns>Amount of capturing specified variable</returns>
        public int CountVariablesCapture(Capture variableCapture)
        {
            int counter = 0;
            var declaration = variableCapture.Declaration;

            foreach (var capture in RootCaptures.Values.SelectMany(i => i.Values))
            {
                if (declaration.Node == capture.Declaration.Node)
                {
                    counter++;
                }
            }

            return counter;
        }

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
                    CurrentScope?.Variables.TryAdd(parameter.Name, new(parameter, parameter.Type, null));
                }
            }
            else if (node.Parent is DelegateExpressionNode delegateNode)
            {
                foreach (var parameter in delegateNode.Parameters)
                {
                    CurrentScope?.Variables.TryAdd(parameter.Name, new(parameter, parameter.Type, null));
                }
            }
        }

        //public override void VisitCatchBlock(TryStatementNode.CatchBlock node)
        //{
        //    base.VisitCatchBlock(node);

        //    var identifier = node.ExceptionVariableIdentifier;

        //    if (!NotEmpty && node.ExceptionType != null &&
        //        identifier != null)
        //    {
        //        AddVariable(identifier.Name, new(identifier, node.ExceptionType, null));
        //    }
        //}
        public override void VisitVariableNode(VariableNode node)
        {
            base.VisitVariableNode(node);

            if (!NotEmpty && node.Type != null)
            {
                AddVariable(node.Name, new(node, node.Type, node.Initializer));
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
                        AddVariable(node.Identifier.Name, new(node.Identifier, node.Type, null));
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

            if (!NotEmpty && node.DestinationIdentifier != null && node.DestinationType != null)
            {
                AddVariable(node.DestinationIdentifier.Name, new(node.DestinationIdentifier, node.DestinationType, null));
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
                !CurrentScope.TryGetVariableScope(name, out var scope))
            {
                return;
            }

            var root = CurrentScope.FindRoot();
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

            scopeIdentifiers.Add(new(CurrentScope, node, name));
        }
        private static bool IsAvailable(AstNode node)
        {
            var parent = node;

            while (parent != null)
            {
                if (parent is CallExpressionNode)
                {
                    return true;
                }
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
        public class Capture(string variableName, CaptureDeclaration declaration)
        {
            /// <summary>
            /// Name of captured variable
            /// </summary>
            public string VariableName { get; } = variableName;
            /// <summary>
            /// Captured variable declaration
            /// </summary>
            public CaptureDeclaration Declaration { get; } = declaration;
            /// <summary>
            /// Scopes and it's references to current variable
            /// </summary>
            public Dictionary<Scope, List<VariableReference>> Scopes { get; } = [];
        }
        /// <summary>
        /// Captured variable declaration
        /// </summary>
        /// <param name="Node">Node that declared variable</param>
        /// <param name="Type">Type of variable</param>
        /// <param name="Initializer">Variable initializer</param>
        public record struct CaptureDeclaration(AstNode Node, TypeInfoNode? Type, ExpressionNode? Initializer);
        /// <summary>
        /// Information about reference to variable
        /// </summary>
        /// <param name="Scope">Scope that contains this reference</param>
        /// <param name="Node">Node that represent referencing to variable</param>
        /// <param name="Name">Name of referencing variable</param>
        public record struct VariableReference(Scope Scope, AstNode Node, string Name);

        #endregion
    }
}
