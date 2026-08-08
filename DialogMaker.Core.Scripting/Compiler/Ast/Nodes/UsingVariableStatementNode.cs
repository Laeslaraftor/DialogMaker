using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents using for disposable
    /// </summary>
    /// <param name="token">Token that represents using keyword</param>
    public class UsingVariableStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Variable that contains disposable object
        /// </summary>
        public VariableNode? Variable { get; set; }
        /// <summary>
        /// Using body. This property can be null when simplified using was declared
        /// </summary>
        public BlockStatementNode? Body { get; set; }

        #region Static

        /// <summary>
        /// Parse using block statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed using block statement</returns>
        public static UsingVariableStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Using);
            UsingVariableStatementNode result = new(token);

            if (stream.Check(DSharpTokenType.LeftParen))
            {
                stream.Eat(DSharpTokenType.LeftParen);
                result.Variable = VariableNode.ParseVariable(stream, null, false);
                stream.Eat(DSharpTokenType.RightParen);
                result.Body = BlockStatementNode.Parse(stream, DSharpStatementType.Code);
            }
            else
            {
                result.Variable = VariableNode.ParseVariable(stream, null);
            }

            return result;
        }

        #endregion
    }
}
