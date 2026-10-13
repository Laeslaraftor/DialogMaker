using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents yield statement
    /// </summary>
    /// <param name="token">Token that represents yield keyword</param>
    public partial class YieldStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Yielding statement
        /// </summary>
        public StatementNode? Statement { get; set; }

        #region Static

        /// <summary>
        /// Parse yield statement starts with current node
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed yield statement</returns>
        public static YieldStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Yield);
            YieldStatementNode yieldStatement = new(token);

            if (stream.Check(DSharpTokenType.Return))
            {
                yieldStatement.Statement = ReturnStatementNode.Parse(stream);
            }
            else if (stream.Check(DSharpTokenType.Continue))
            {
                yieldStatement.Statement = ContinueStatementNode.Parse(stream);
            }
            else if (stream.Check(DSharpTokenType.Break))
            {
                yieldStatement.Statement = BreakStatementNode.Parse(stream);
            }
            else
            {
                stream.ThrowPositionException("Yield statement should returning, continuing and breaking");
            }

            yieldStatement.Statement?.Parent = yieldStatement;

            return yieldStatement;
        }

        #endregion
    }
}
