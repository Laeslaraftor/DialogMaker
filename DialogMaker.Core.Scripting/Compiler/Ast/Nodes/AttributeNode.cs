using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Attribute node
    /// </summary>
    /// <param name="token">Token that represents name of attribute</param>
    public class AttributeNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Type of this attribute
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Attribute arguments
        /// </summary>
        public List<ExpressionNode> Arguments { get; set; } = [];

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Type == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Type: {Type}");
            
            if (Arguments.Count > 0)
            {
                builder.AppendLine("Arguments:");

                foreach (var arg in Arguments)
                {
                    builder.AppendLine(arg.ToString().Trim());
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Try parse attributes
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="result">List of parsed attributes</param>
        /// <returns>Returns true if attributes successfully parsed</returns>
        public static bool TryParse(AstParserStream stream, [NotNullWhen(true)] out List<AttributeNode>? result)
        {
            if (!stream.Check(DSharpTokenType.LeftBracket))
            {
                result = null;
                return false;
            }

            result = [];

            do
            {
                stream.Eat(DSharpTokenType.LeftBracket);

                var attributeType = TypeInfoNode.Parse(stream, false, false);
                AttributeNode attribute = new(attributeType.Token)
                {
                    Type = attributeType
                };

                if (stream.Check(DSharpTokenType.LeftParen))
                {
                    stream.Eat(DSharpTokenType.LeftParen);

                    while (!stream.Check(DSharpTokenType.RightParen))
                    {
                        var arg = ExpressionNode.ParseLiteralOrArray(stream);
                        attribute.Arguments.Add(arg);

                        if (!ArrayExpressionNode.CheckTokenAfterComma(stream))
                        {
                            stream.ThrowPositionException("Required literal or array");
                        }
                    }

                    stream.Eat(DSharpTokenType.RightParen);
                }

                result.Add(attribute);
                stream.Eat(DSharpTokenType.RightBracket);
            }
            while (stream.Check(DSharpTokenType.LeftBracket));

            return true;
        }

        #endregion
    }
}
