using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents case statement
    /// </summary>
    /// <param name="token">Token that represents case keyword</param>
    public partial class CaseStatementNode(DSharpToken token) : StatementNode(token), ICaseNode
    {
        /// <summary>
        /// Expression of comparing value
        /// </summary>
        public ExpressionNode? KeyExpression { get; set; }
        /// <summary>
        /// Case body
        /// </summary>
        public BlockStatementNode? Body { get; set; }
        /// <summary>
        /// Is default case
        /// </summary>
        public bool IsDefault => Token.Type == DSharpTokenType.Default;
        AstNode? ICaseNode.ValueNode => Body;

        #region Controls

        AstNode ICaseNode.AsNode() => this;

        #endregion

        #region Static

        /// <summary>
        /// Parse case statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed case statement</returns>
        public static CaseStatementNode Parse(AstParserStream stream)
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
                token = stream.Eat(DSharpTokenType.Case);
                valueExpression = ExpressionNode.ParseExpression(stream);
            }

            var body = BlockStatementNode.Parse(stream, DSharpStatementType.Code, false, DSharpTokenType.Colon, DSharpTokenType.RightBrace,
                                                                                                                DSharpTokenType.Case,
                                                                                                                DSharpTokenType.Default);
            CaseStatementNode caseStatement = new(token)
            {
                KeyExpression = valueExpression,
                Body = body
            };

            valueExpression?.Parent = caseStatement;
            body.Parent = caseStatement; 

            return caseStatement;
        }

        #endregion
    }
}
