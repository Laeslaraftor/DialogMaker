using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents typeof() expression
    /// </summary>
    /// <param name="token">Token that represents typeof keyword</param>
    public class TypeOfExpressionNode(DSharpToken token) : CompileTimeExpressionNode<TypeInfoNode>(token)
    {
        #region Статика

        /// <summary>
        /// Parse typeof() expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed typeof() expression</returns>
        public static TypeOfExpressionNode Parse(AstParserStream stream)
        {
            return Parse<TypeOfExpressionNode>(stream, DSharpTokenType.TypeOf, t => new(t), () => TypeInfoNode.Parse(stream, true, true));
        }

        #endregion
    }
}
