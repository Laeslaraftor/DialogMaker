using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents case statement
    /// </summary>
    /// <param name="token">Token that represents case keyword</param>
    public partial class CaseStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Expression of comparing value
        /// </summary>
        public ExpressionNode? ValueExpression { get; set; }
        /// <summary>
        /// Case body
        /// </summary>
        public BlockStatementNode? Body { get; set; }
        /// <summary>
        /// Is default case
        /// </summary>
        public bool IsDefault => Token.Type == DSharpTokenType.Default;

        #region Static

        /// <summary>
        /// Parse case statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="isLambda">Is lambda case required to parse</param>
        /// <returns>Parsed case statement</returns>
        public static CaseStatementNode Parse(AstParserStream stream, bool isLambda)
        {
            DSharpToken token;
            ExpressionNode? valueExpression;

            if (stream.Check(DSharpTokenType.Default))
            {
                token = stream.Eat(DSharpTokenType.Default);
                valueExpression = null;
            }
            else
            {
                if (isLambda)
                {
                    valueExpression = ExpressionNode.ParseExpression(stream);
                    token = valueExpression.Token;
                }
                else
                {
                    token = stream.Eat(DSharpTokenType.Case);
                    valueExpression = ExpressionNode.ParseExpression(stream);
                }
            }

            var body = isLambda ? BlockStatementNode.Parse(stream, DSharpStatementType.Code, DSharpTokenType.Semicolon, DSharpTokenType.Lambda) :
                                  BlockStatementNode.Parse(stream, DSharpStatementType.Code, false, DSharpTokenType.Colon, DSharpTokenType.RightBrace,
                                                                                                                           DSharpTokenType.Case,
                                                                                                                           DSharpTokenType.Default);
            CaseStatementNode caseStatement = new(token)
            {
                ValueExpression = valueExpression,
                Body = body
            };

            valueExpression?.Parent = caseStatement;
            body.Parent = caseStatement; 

            return caseStatement;
        }

        #endregion
    }
}
