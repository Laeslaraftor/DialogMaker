using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Attribute node
    /// </summary>
    /// <param name="token">Token that represents name of attribute</param>
    public class AttributeNode(DialogScriptToken token) : NamedNode(token)
    {
        /// <summary>
        /// Type of this attribute
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Attribute arguments
        /// </summary>
        public List<ExpressionNode> Arguments { get; set; } = [];

        #region Статика

        /// <summary>
        /// Try parse attributes
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="result">List of parsed attributes</param>
        /// <returns>Returns true if attributes successfully parsed</returns>
        public static bool TryParse(AstParserStream stream, [NotNullWhen(true)] out List<AttributeNode>? result)
        {
            if (!stream.Check(DialogScriptTokenType.LeftBracket))
            {
                result = null;
                return false;
            }

            result = [];

            do
            {
                stream.Eat(DialogScriptTokenType.LeftBracket);

                var attributeType = TypeInfoNode.Parse(stream, false, false);
                AttributeNode attribute = new(attributeType.Token)
                {
                    Type = attributeType
                };

                if (stream.Check(DialogScriptTokenType.LeftParen))
                {
                    stream.Eat(DialogScriptTokenType.LeftParen);

                    while (!stream.Check(DialogScriptTokenType.RightParen))
                    {
                        var arg = ExpressionNode.ParseLiteralOrArray(stream);
                        attribute.Arguments.Add(arg);

                        if (!ArrayExpressionNode.CheckTokenAfterComma(stream))
                        {
                            stream.ThrowPositionException("Required literal or array");
                        }
                    }

                    stream.Eat(DialogScriptTokenType.RightParen);
                }

                result.Add(attribute);
                stream.Eat(DialogScriptTokenType.RightBracket);
            }
            while (stream.Check(DialogScriptTokenType.LeftBracket));

            return true;
        }

        #endregion
    }
}
