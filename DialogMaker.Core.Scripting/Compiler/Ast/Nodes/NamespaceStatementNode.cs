using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Namespace statement
    /// </summary>
    /// <param name="token">Token that represents namespace keyword</param>
    public class NamespaceStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Identifier of this namespace
        /// </summary>
        public IdentifierExpressionNode? Identifier { get; set; }

        #region Статика

        /// <summary>
        /// Parse namespace statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed namespace statement</returns>
        public static NamespaceStatementNode Parse(AstParserStream stream)
        {
            var namespaceKeyword = stream.Eat(DSharpTokenType.Namespace);
            var identifier = IdentifierExpressionNode.Parse(stream, false);

            if (stream.Check(DSharpTokenType.Semicolon))
            {
                stream.Eat(DSharpTokenType.Semicolon);
                return new(namespaceKeyword)
                {
                    Identifier = identifier
                };
            }

            return new NamespaceBlockStatementBlock(namespaceKeyword)
            {
                Identifier = identifier,
                Block = BlockStatementNode.Parse(stream)
            };
        }

        #endregion
    }
}
