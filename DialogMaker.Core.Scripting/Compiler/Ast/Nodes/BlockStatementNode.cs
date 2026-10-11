using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Block of statements
    /// </summary>
    /// <param name="token">Token that represents start of block</param>
    public partial class BlockStatementNode(DSharpToken token) : StatementNode(token)
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
            return Parse(stream, type, endWith != DSharpTokenType.Semicolon, startWith, endTokens: endWith);
        }
        public static BlockStatementNode Parse(AstParserStream stream, DSharpStatementType type, bool eatEnding, DSharpTokenType startWith, params DSharpTokenType[] endTokens)
        {
            var blockStartToken = stream.Eat(startWith);
            BlockStatementNode block = new(blockStartToken);

            ParseBody(stream, type, block.Statements, endTokens);
            block.Statements.SetParent(block);

            if (eatEnding)
            {
                bool tokenEaten = false;

                foreach (var token in endTokens)
                {
                    if (stream.Check(token))
                    {
                        stream.Eat(token);
                        tokenEaten = true;
                        break;
                    }
                }

                if (!tokenEaten)
                {
                    stream.ThrowUnexpectedTokenException();
                }
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
            ParseBody(stream, type, buffer, endTokens: endWith);
        }
        public static void ParseBody(AstParserStream stream, DSharpStatementType type, List<StatementNode> buffer, params DSharpTokenType[] endTokens)
        {
            if (endTokens.Length == 0)
            {
                return;
            }

            while (!stream.Check(endTokens) && !stream.IsEndOfFile())
            {
                if (stream.Check(DSharpTokenType.MultilineComment) ||
                    stream.Check(DSharpTokenType.Comment))
                {
                    stream.Eat(stream.Current!.Type);
                    continue;
                }

                buffer.Add(ParseStatement(stream, type));

                if (endTokens.Length == 1 && endTokens[0] == DSharpTokenType.Semicolon)
                {
                    break;
                }
            }
        }

        #endregion
    }
}
