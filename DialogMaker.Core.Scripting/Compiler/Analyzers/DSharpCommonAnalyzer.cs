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
            /// <param name="node">Node to check</param>
            /// <returns>Is node with specified type contains in provided node</returns>
            public bool Contains<T>()
                where T : AstNode
            {
                bool contains = false;

                node.Accept(n => contains |= n is T);

                return contains;
            }
            /// <summary>
            /// Accept delegate visitor
            /// </summary>
            /// <param name="visitor">Nodes handler</param>
            /// <param name="visitMode">Visiting mode</param>
            public void Accept(Action<AstNode> visitor, DSharpAstVisitMode visitMode = DSharpAstVisitMode.Simple)
            {
                DSharpAstNodeDelegateVisitor delegateVisitor;

                lock (_delegateVisitorsPool)
                {
                    delegateVisitor = _delegateVisitorsPool.GetElement();
                }

                delegateVisitor.Handler = visitor;

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
