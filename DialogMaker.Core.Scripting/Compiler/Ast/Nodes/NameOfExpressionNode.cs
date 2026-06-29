using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Expression that represents nameof() expression
    /// </summary>
    /// <param name="token">Token that represents nameof keyword</param>
    public class NameOfExpressionNode(DSharpToken token) : CompileTimeExpressionNode<ExpressionNode>(token)
    {
        #region Статика

        /// <summary>
        /// Parse nameof() expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed nameof() expression</returns>
        public static NameOfExpressionNode Parse(AstParserStream stream)
        {
            return Parse<NameOfExpressionNode>(stream, DSharpTokenType.NameOf, t => new(t), () => ParseIdentifier(stream));
        }

        #endregion
    }
}
