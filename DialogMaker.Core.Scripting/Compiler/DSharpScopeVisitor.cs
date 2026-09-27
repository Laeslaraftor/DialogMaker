using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Visitor that searching scoped variables
    /// </summary>
    public class DSharpScopeVisitor : DSharpAstVisitor
    {
        /// <summary>
        /// Scopes that was found
        /// </summary>
        public Dictionary<AstNode, Scope> Scopes { get; } = [];
        /// <summary>
        /// Root scope
        /// </summary>
        public Scope? RootScope { get; private set; }

        protected Scope? CurrentScope { get; private set; }
        protected bool NotEmpty { get; private set; }

        #region Controls

        /// <summary>
        /// Add variable to current scope
        /// </summary>
        /// <param name="name">Variable name</param>
        /// <param name="node">Node that represent variable or it name</param>
        /// <exception cref="DSharpCompilerException">Variable with same name already declared in current scope</exception>
        public void AddVariable(string name, AstNode node)
        {
            if (CurrentScope == null)
            {
                CurrentScope = new(node, true);
                RootScope = CurrentScope;
                Scopes.Add(node, CurrentScope);
            }
            else if (CurrentScope.ContainsInCurrentScope(name))
            {
                throw new DSharpCompilerException($"Variable \"{name}\" with same name already declared in current scope", node);
            }

            CurrentScope.Variables.Add(name, node);
        }
        /// <summary>
        /// Reset visitor
        /// </summary>
        /// <param name="full">Is full resetting. Full resetting clears all values, otherwise it just set root scope as current</param>
        public virtual void Reset(bool full = true)
        {
            if (full || Scopes.Count == 0)
            {
                NotEmpty = false;
                CurrentScope = null;
                Scopes.Clear();
            }
            else
            {
                NotEmpty = true;
                CurrentScope = Scopes.First().Value;
            }
        }

        public override void StartVisiting(AstNode node)
        {
            base.StartVisiting(node);

            if (node is not BlockStatementNode)
            {
                return;
            }

            Scope nextScope;
            bool addScope = false;

            if (Scopes.TryGetValue(node, out var scope) ||
                CurrentScope != null && CurrentScope.InnerScopes?.TryGetValue(node, out scope) == true)
            {
                nextScope = scope;
            }
            else
            {
                addScope = true;
                nextScope = new(node)
                {
                    PreviousScope = CurrentScope
                };
            }
            if (CurrentScope != null && !NotEmpty)
            {
                CurrentScope.InnerScopes ??= [];
                CurrentScope.InnerScopes.Add(node, nextScope);
            }

            if (addScope)
            {
                Scopes.Add(node, nextScope);
            }

            CurrentScope = nextScope;
            RootScope ??= nextScope;
        }
        public override void EndVisiting(AstNode node)
        {
            base.EndVisiting(node);

            if (node is BlockStatementNode)
            {
                CurrentScope = CurrentScope?.PreviousScope;
            }
        }

        #endregion

        #region Classes

        /// <summary>
        /// Variables scope
        /// </summary>
        /// <param name="node">Node that represents scope beginning</param>
        /// <param name="forceRoot">Is scope forces to be root</param>
        public class Scope(AstNode node, bool forceRoot = false)
        {
            /// <summary>
            /// Node that represents scope beginning
            /// </summary>
            public AstNode Node { get; } = node;
            /// <summary>
            /// Is root node. All invokable and delegate expression is root nodes
            /// </summary>
            public bool IsRoot { get; } = forceRoot ||
                                          node.Parent is InvokableNode ||
                                          node.Parent is DelegateExpressionNode;
            /// <summary>
            /// Previous scope
            /// </summary>
            public Scope? PreviousScope { get; set; }
            /// <summary>
            /// Scope that contained by current scope
            /// </summary>
            public Dictionary<AstNode, Scope>? InnerScopes { get; set; }
            /// <summary>
            /// Scope variables
            /// </summary>
            public Dictionary<string, AstNode> Variables { get; } = [];

            /// <summary>
            /// Check variable on containing in current scope.
            /// It recursively checks all previous scopes include themselves and stops when scopes ended or root scope was found.
            /// </summary>
            /// <param name="name">Name of variable to check</param>
            /// <returns>Is variable contains in current scope</returns>
            public bool ContainsInCurrentScope(string name)
            {
                var parent = this;

                while (parent != null)
                {
                    if (parent.Variables.ContainsKey(name))
                    {
                        return true;
                    }
                    else if (parent.IsRoot)
                    {
                        return false;
                    }

                    parent = parent.PreviousScope;
                }

                return false;
            }
            /// <summary>
            /// Try to get variable scope
            /// </summary>
            /// <param name="name">Name of variable for getting it scope</param>
            /// <param name="result">Scope that was found</param>
            /// <returns>Is scope found successfully</returns>
            public bool TryGetVariableScope(string name, [NotNullWhen(true)] out Scope? result)
            {
                var parent = this;

                while (parent != null)
                {
                    if (parent.Variables.ContainsKey(name))
                    {
                        result = parent;
                        return true;
                    }

                    parent = parent.PreviousScope;
                }

                result = null;
                return false;
            }
            /// <summary>
            /// Find root scope for current scope.
            /// </summary>
            /// <returns>Root scope</returns>
            /// <exception cref="InvalidOperationException">Unable to find root scope</exception>
            public Scope FindRoot()
            {
                var parent = this;

                while (parent != null)
                {
                    if (parent.IsRoot)
                    {
                        return parent;
                    }

                    parent = parent.PreviousScope;
                }

                throw new InvalidOperationException("Unable to find root scope");
            }
        }

        #endregion
    }
}
