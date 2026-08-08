using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

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

        #region Controls

        public override string ToString()
        {
            if (Statements.Count == 0)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());

            foreach (var statement in Statements)
            {
                builder.AppendLine(statement.ToString().Trim());
            }

            return builder.ToString();
        }

        #endregion

        #region Static

        /// <summary>
        /// Parse block start with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="type">Statements type</param>
        /// <param name="startWith">Token that indicate start of statements block</param>
        /// <param name="endWith">Token that indicate end of statements block</param>
        /// <returns>Parsed block of statements</returns>
        public static BlockStatementNode Parse(AstParserStream stream, DSharpStatementType type, DSharpTokenType endWith = DSharpTokenType.RightBrace, DSharpTokenType startWith = DSharpTokenType.LeftBrace)
        {
            var blockStartToken = stream.Eat(startWith);
            BlockStatementNode block = new(blockStartToken);

            ParseBody(stream, type, block.Statements, endWith);

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
        /// <param name="type">Statements type</param>
        /// <param name="buffer">Buffer of statements</param>
        /// <param name="endWith">Token that indicate end of statements block</param>
        public static void ParseBody(AstParserStream stream, DSharpStatementType type, List<StatementNode> buffer, DSharpTokenType endWith = DSharpTokenType.RightBrace)
        {
            while (!stream.Check(endWith) && !stream.IsEndOfFile())
            {
                if (stream.Check(DSharpTokenType.MultilineComment) ||
                    stream.Check(DSharpTokenType.Comment))
                {
                    stream.Eat(stream.Current!.Type);
                    continue;
                }

                buffer.Add(ParseStatement(stream, type));

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
        /// <param name="type">Statements type</param>
        /// <param name="endWith">Token that indicate end of statements block</param>
        /// <returns>List of parsed statements</returns>
        public static List<StatementNode> ParseBody(AstParserStream stream, DSharpStatementType type, DSharpTokenType endWith = DSharpTokenType.RightBrace)
        {
            List<StatementNode> buffer = [];
            ParseBody(stream, type, buffer, endWith);

            return buffer;
        }

        #endregion
    }
}
