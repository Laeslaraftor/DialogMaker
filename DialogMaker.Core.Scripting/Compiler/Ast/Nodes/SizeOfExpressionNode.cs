using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents sizeof() expression
    /// </summary>
    /// <param name="token">Token that represents sizeof keyword</param>
    public class SizeOfExpressionNode(DSharpToken token) : CompileTimeExpressionNode<TypeInfoNode>(token)
    {
        #region Статика

        /// <summary>
        /// Parse sizeof() expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed sizeof() expression</returns>
        public static SizeOfExpressionNode Parse(AstParserStream stream)
        {
            return Parse<SizeOfExpressionNode>(stream, DSharpTokenType.SizeOf, t => new(t), () => TypeInfoNode.Parse(stream, true, true));
        }

        #endregion
    }
}
