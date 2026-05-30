using DialogMaker.Core.Scripting.Compiler.Lexer;
using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents something that can be invoked or called
    /// </summary>
    /// <param name="token">Token that represents name of this node</param>
    public abstract class InvokableNode(DialogScriptToken token) : NamedNode(token)
    {
        /// <summary>
        /// Attributes list of this invokable
        /// </summary>
        public List<AttributeNode>? Attributes { get; set; }
        /// <summary>
        /// Parameter to invoke or call this node
        /// </summary>
        public List<VariableNode> Parameters { get; set; } = [];
        /// <summary>
        /// Body of this node
        /// </summary>
        public BlockStatementNode? Body { get; set; }

        #region Статика

        /// <summary>
        /// Parse parameters starts with current token and write them into buffer
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="buffer">Buffer for parsed parameters</param>
        public static void ParseParameters(AstParserStream stream, List<VariableNode> buffer)
        {
            stream.Eat(DialogScriptTokenType.LeftParen);

            while (!stream.Check(DialogScriptTokenType.RightParen))
            {
                AttributeNode.TryParse(stream, out var attributes);
                var variable = VariableNode.ParseVariable(stream, attributes, false);
                buffer.Add(variable);

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DialogScriptTokenType.RightParen))
                {
                    stream.ThrowPositionException("Required parameter");
                }
            }

            stream.Eat(DialogScriptTokenType.RightParen);
        }
        /// <summary>
        /// Parse parameters starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>List of parsed parameters</returns>
        public static List<VariableNode> ParseParameters(AstParserStream stream)
        {
            List<VariableNode> buffer = [];
            ParseParameters(stream, buffer);

            return buffer;
        }

        #endregion
    }
}
