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
        public ExpressionNode? Identifier { get; set; }

        #region Controls

        /// <summary>
        /// Get full name of current namespace
        /// </summary>
        /// <returns>Full name of current namespace</returns>
        /// <exception cref="InvalidOperationException">Namespace identifier not specified</exception>
        /// <exception cref="InvalidOperationException">Invalid namespace identifier</exception>
        public string GetName()
        {
            if (Identifier == null)
            {
                throw new InvalidOperationException($"Namespace identifier not specified: {this}");
            }
            if (Identifier is IdentifierExpressionNode identifierExpression)
            {
                return identifierExpression.GetName(false);
            }
            else if (Identifier is MemberAccessExpressionNode memberAccess)
            {
                return memberAccess.GetName(false);
            }

            throw new InvalidOperationException($"Invalid namespace identifier: {Identifier}");
        }

        #endregion

        #region Static

        /// <summary>
        /// Parse namespace statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed namespace statement</returns>
        public static NamespaceStatementNode Parse(AstParserStream stream)
        {
            var namespaceKeyword = stream.Eat(DSharpTokenType.Namespace);
            var identifier = ExpressionNode.ParseIdentifier(stream, false);

            if (stream.Check(DSharpTokenType.Semicolon))
            {
                stream.Eat(DSharpTokenType.Semicolon);
                NamespaceStatementNode result = new(namespaceKeyword)
                {
                    Identifier = identifier
                };
                identifier.Parent = result;

                return result;
            }

            NamespaceBlockStatementBlock namespaceBlock = new(namespaceKeyword)
            {
                Identifier = identifier,
                Block = BlockStatementNode.Parse(stream, DSharpStatementType.Declaration)
            };

            identifier.Parent = namespaceBlock;
            namespaceBlock.Block.Parent = namespaceBlock;

            return namespaceBlock;
        }

        #endregion
    }
}
