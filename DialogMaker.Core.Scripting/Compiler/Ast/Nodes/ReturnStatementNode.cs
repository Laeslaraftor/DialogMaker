using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Return statement for returning value from function/method or just stopping it execution
    /// </summary>
    /// <param name="token">Token that represents return statement</param>
    public class ReturnStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Returning expression
        /// </summary>
        public ExpressionNode? Value { get; set; }

        #region Статика

        /// <summary>
        /// Parse return statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed return statement</returns>
        public static ReturnStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Return);
            ReturnStatementNode returnStatement = new(token);

            if (!stream.Check(DSharpTokenType.Semicolon))
            {
                returnStatement.Value = ExpressionNode.ParseExpression(stream);
            }

            stream.Eat(DSharpTokenType.Semicolon);

            return returnStatement;
        }

        #endregion
    }
}
