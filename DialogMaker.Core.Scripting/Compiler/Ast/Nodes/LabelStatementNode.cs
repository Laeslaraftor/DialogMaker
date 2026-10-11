using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents label
    /// </summary>
    /// <param name="token">Token that represents label identifier</param>
    public partial class LabelStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Check current tokens on label definition
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Is next token represents label definition</returns>
        public static bool IsLabel(AstParserStream stream)
        {
            return stream.Check(DSharpTokenType.Identifier) &&
                   stream.Check(DSharpTokenType.Colon, 1);
        }
        /// <summary>
        /// Parse label node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed label node</returns>
        public static LabelStatementNode Parse(AstParserStream stream)
        {
            var identifier = stream.Eat(DSharpTokenType.Identifier);
            stream.Eat(DSharpTokenType.Colon);

            if (stream.Check(DSharpTokenType.Semicolon))
            {
                stream.Eat(DSharpTokenType.Semicolon);
            }

            return new(identifier);
        }
    }
}
