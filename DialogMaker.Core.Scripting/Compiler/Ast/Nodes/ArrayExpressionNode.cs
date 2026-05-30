using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Array expression node
    /// </summary>
    /// <param name="token">Token that represents start of array</param>
    public class ArrayExpressionNode(DialogScriptToken token) : ExpressionNode(token)
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
            var startToken = stream.Eat(DialogScriptTokenType.LeftBracket);
            ArrayExpressionNode array = new(startToken);

            while (!stream.Check(DialogScriptTokenType.RightBracket))
            {
                var element = ParseExpression(stream);
                array.Elements.Add(element);

                if (!CheckTokenAfterComma(stream))
                {
                    stream.ThrowPositionException("Required item of array");
                }
            }

            stream.Eat(DialogScriptTokenType.RightBracket);

            return array;
        }
        /// <summary>
        /// Check comma and eat it, then checks next value existents
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="endToken">Token which indicates end of list</param>
        /// <returns>Returns true when comma not existed or value existed, return false when value not presented after comma</returns>
        public static bool CheckTokenAfterComma(AstParserStream stream, DialogScriptTokenType endToken = DialogScriptTokenType.RightBracket)
        {
            if (stream.Check(DialogScriptTokenType.Comma))
            {
                stream.Eat(DialogScriptTokenType.Comma);

                if (stream.Check(endToken))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
