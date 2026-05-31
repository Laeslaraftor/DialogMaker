using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Block of statements
    /// </summary>
    /// <param name="token">Token that represents start of block</param>
    public class BlockStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// List of statement
        /// </summary>
        public List<StatementNode> Statements { get; set; } = [];

        #region Статика

        /// <summary>
        /// Parse block start with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="endWith">Token that indicate end of statements block</param>
        /// <returns>Parsed block of statements</returns>
        public static BlockStatementNode Parse(AstParserStream stream, DSharpTokenType endWith = DSharpTokenType.RightBrace, DSharpTokenType startWith = DSharpTokenType.LeftBrace)
        {
            var blockStartToken = stream.Eat(startWith);
            BlockStatementNode block = new(blockStartToken);

            ParseBody(stream, block.Statements, endWith);
            
            if (endWith != DSharpTokenType.Semicolon)
            {
                stream.Eat(endWith);
            }

            return block;
        }
        /// <summary>
        /// Parse all statements and add them to buffer
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="buffer">Buffer of statements</param>
        /// <param name="endWith">Token that indicate end of statements block</param>
        public static void ParseBody(AstParserStream stream, List<StatementNode> buffer, DSharpTokenType endWith = DSharpTokenType.RightBrace)
        {
            while (!stream.Check(endWith) && !stream.IsEndOfFile())
            {
                buffer.Add(ParseStatement(stream));

                if (endWith == DSharpTokenType.Semicolon)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// Parse all statements
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="endWith">Token that indicate end of statements block</param>
        /// <returns>List of parsed statements</returns>
        public static List<StatementNode> ParseBody(AstParserStream stream, DSharpTokenType endWith = DSharpTokenType.RightBrace)
        {
            List<StatementNode> buffer = [];
            ParseBody(stream, buffer, endWith);

            return buffer;
        }

        #endregion
    }
}
