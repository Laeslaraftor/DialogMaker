using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Using statement
    /// </summary>
    /// <param name="token">Token that represents using keyword</param>
    public class UsingStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Identifier of namespace
        /// </summary>
        public ExpressionNode? Identifier { get; set; }

        #region Управление

        /// <summary>
        /// Get full namespace value
        /// </summary>
        /// <returns>Namespace value</returns>
        /// <exception cref="InvalidOperationException">Identifier must be not null and valid expression</exception>
        public string GetNamespace()
        {
            if (Identifier is IdentifierExpressionNode identifier)
            {
                return identifier.GetName(false);
            }
            else if (Identifier is MemberAccessExpressionNode memberAccess)
            {
                return memberAccess.GetName(false);
            }

            throw new InvalidOperationException($"Identifier must be not null and valid expression");
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse using statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed using statement</returns>
        public static UsingStatementNode Parse(AstParserStream stream)
        {
            var usingToken = stream.Eat(DSharpTokenType.Using);
            var identifier = ExpressionNode.ParseIdentifier(stream, false);
            stream.Eat(DSharpTokenType.Semicolon);

            return new(usingToken)
            {
                Identifier = identifier
            };
        }

        #endregion
    }
}
