using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Base class of expression that got it value at compile time
    /// </summary>
    /// <typeparam name="T">Type of expression</typeparam>
    /// <param name="token">Token that represents expression keyword</param>
    public abstract class CompileTimeExpressionNode<T>(DSharpToken token) : ExpressionNode(token)
        where T : AstNode
    {
        /// <summary>
        /// Value of expression
        /// </summary>
        public T? Value { get; set; }

        #region Статика

        /// <summary>
        /// Parse compile time expression starts with current token
        /// </summary>
        /// <typeparam name="TNode">Type of expression that will be parsed</typeparam>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="tokenType">Token of expression</param>
        /// <param name="fabric">Expression node fabric</param>
        /// <param name="valueParser">Parser for value that contains in expression</param>
        /// <returns>Parsed expression</returns>
        protected static TNode Parse<TNode>(AstParserStream stream, DSharpTokenType tokenType, Func<DSharpToken, TNode> fabric, Func<T> valueParser)
            where TNode : CompileTimeExpressionNode<T>
        {
            var token = stream.Eat(tokenType);
            stream.Eat(DSharpTokenType.LeftParen);
            var value = valueParser();
            stream.Eat(DSharpTokenType.RightParen);

            var result = fabric(token);
            result.Value = value;

            return result;
        }

        #endregion
    }
}
