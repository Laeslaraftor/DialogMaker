using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement of foreach loop
    /// </summary>
    /// <param name="token">Token that represents foreach keyword</param>
    public class ForeachStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Variable that contains value of iteration
        /// </summary>
        public VariableNode? Variable { get; set; }
        /// <summary>
        /// Expression that must returns object with <c>public IEnumerator GetEnumerator()</c>
        /// </summary>
        public ExpressionNode? EnumeratorExpression { get; set; }
        /// <summary>
        /// Body of foreach loop
        /// </summary>
        public BlockStatementNode? Body { get; set; }

        #region Статика

        /// <summary>
        /// Parse foreach loop starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed foreach loop</returns>
        public static ForeachStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Foreach);
            ForeachStatementNode result = new(token);

            stream.Eat(DSharpTokenType.LeftParen);

            result.Variable = VariableNode.ParseVariable(stream, null, false, false);

            stream.Eat(DSharpTokenType.In);

            result.EnumeratorExpression = ExpressionNode.ParseExpression(stream);

            stream.Eat(DSharpTokenType.RightParen);

            result.Body = BlockStatementNode.Parse(stream);

            return result;
        }

        #endregion
    }
}
