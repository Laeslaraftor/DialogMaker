using Acly.Data;
using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler.Analyzers
{
    public static class DSharpCommonAnalyzer
    {
        private static readonly ElementsPool<DSharpAstNodeDelegateVisitor> _delegateVisitorsPool = new();

        extension(AstNode node)
        {
            /// <summary>
            /// Check provided node on containing node with specified type
            /// </summary>
            /// <typeparam name="T">Checking node type</typeparam>
            /// <param name="visitMode">Visiting mode</param>
            /// <param name="skipMethodsAndDelegates">
            /// Skip contained method and delegates.
            /// So, if it's sets as <c>true</c> this check only current method scope
            /// </param>
            /// <param name="startMethodDepth">
            /// Inner method depth. It used as start value for other method recursive counter.
            /// Set <c>-1</c> if current node is method node or delegate expression, otherwise set 0
            /// </param>
            /// <returns>Is node with specified type contains in provided node</returns>
            public bool Contains<T>(DSharpAstVisitMode visitMode = DSharpAstVisitMode.Recursive, bool skipMethodsAndDelegates = false, int startMethodDepth = 0)
                where T : AstNode
            {
                bool contains = false;

                if (skipMethodsAndDelegates)
                {
                    int otherMethodAndDelegateDepth = startMethodDepth;

                    node.Accept(n =>
                    {
                        if (n is MethodNode || n is DelegateExpressionNode)
                        {
                            otherMethodAndDelegateDepth++;
                            return;
                        }
                        if (otherMethodAndDelegateDepth == 0)
                        {
                            contains |= n is T;
                        }
                    }, n =>
                    {
                        if (n is MethodNode || n is DelegateExpressionNode)
                        {
                            otherMethodAndDelegateDepth--;
                        }
                    }, visitMode);
                }
                else
                {
                    node.Accept(n => contains |= n is T, null, visitMode);
                }

                return contains;
            }
            /// <summary>
            /// Accept delegate visitor
            /// </summary>
            /// <param name="startVisitor">Nodes start visiting handler</param>
            /// <param name="endVisitor">Nodes end visiting handler</param>
            /// <param name="visitMode">Visiting mode</param>
            public void Accept(Action<AstNode>? startVisitor, Action<AstNode>? endVisitor, DSharpAstVisitMode visitMode = DSharpAstVisitMode.Simple)
            {
                if (startVisitor == null && endVisitor == null)
                {
                    return;
                }

                DSharpAstNodeDelegateVisitor delegateVisitor;

                lock (_delegateVisitorsPool)
                {
                    delegateVisitor = _delegateVisitorsPool.GetElement();
                }

                delegateVisitor.StartHandler = startVisitor;
                delegateVisitor.EndHandler = endVisitor;

                try
                {
                    node.Accept(delegateVisitor, visitMode);
                }
                finally
                {
                    lock (_delegateVisitorsPool)
                    {
                        _delegateVisitorsPool.Free(delegateVisitor);
                    }
                }
            }
        }
    }
}
