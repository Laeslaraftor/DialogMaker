using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Array expression node
    /// </summary>
    /// <param name="token">Token that represents start of array</param>
    public class ArrayExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Elements of this array
        /// </summary>
        public List<ExpressionNode> Elements { get; set; } = [];

        #region Управление

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine($"Array of {Elements.Count} elements at {base.ToString()}");
            builder.AppendLine("[");

            foreach (var element in Elements)
            {
                builder.AppendLine(element.ToString());
            }

            builder.AppendLine("]");

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse array starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed array</returns>
        public static ArrayExpressionNode Parse(AstParserStream stream)
        {
            var startToken = stream.Eat(DSharpTokenType.LeftBracket);
            ArrayExpressionNode array = new(startToken);

            while (!stream.Check(DSharpTokenType.RightBracket))
            {
                var element = ParseExpression(stream);
                array.Elements.Add(element);

                if (!CheckTokenAfterComma(stream))
                {
                    stream.ThrowPositionException("Required item of array");
                }
            }

            stream.Eat(DSharpTokenType.RightBracket);

            return array;
        }
        /// <summary>
        /// Check comma and eat it, then checks next value existents
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="endToken">Token which indicates end of list</param>
        /// <param name="eatToken">Flag that indicates intent to eat comma token</param>
        /// <returns>Returns true when comma not existed or value existed, return false when value not presented after comma</returns>
        public static bool CheckTokenAfterComma(AstParserStream stream, DSharpTokenType endToken = DSharpTokenType.RightBracket, bool eatToken = true)
        {
            if (stream.Check(DSharpTokenType.Comma))
            {
                int offset = 0;

                if (eatToken)
                {
                    stream.Eat(DSharpTokenType.Comma);
                }
                else
                {
                    offset = 1;
                }

                if (stream.Check(endToken, offset))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
