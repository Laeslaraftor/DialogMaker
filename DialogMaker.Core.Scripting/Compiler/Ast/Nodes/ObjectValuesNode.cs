using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents block with declaring object values
    /// </summary>
    /// <param name="token">Token that represents start of block</param>
    public class ObjectValuesNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Declared values for fields or properties
        /// </summary>
        public Dictionary<IdentifierExpressionNode, ExpressionNode> Values { get; set; } = [];

        public override string ToString()
        {
            if (Values.Count == 0)
            {
                return base.ToString();
            }

            StringBuilder builder = new(base.ToString());
            builder.AppendLine();

            foreach (var info in Values)
            {
                builder.AppendLine($"Value identifier: {info.Key}");
                builder.AppendLine($"Value expression: {info.Value}");
            }

            return builder.ToString().TrimEnd();
        }

        #region Static

        /// <summary>
        /// Parse object values declaration starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax treen parser stream</param>
        /// <param name="separator">Separator that should be between field/property identifier and expression value</param>
        /// <returns>Parsed object values declaration</returns>
        public static ObjectValuesNode Parse(AstParserStream stream, DSharpTokenType separator = DSharpTokenType.Assign)
        {
            var token = stream.Eat(DSharpTokenType.LeftBrace);
            ObjectValuesNode result = new(token);

            while (!stream.Check(DSharpTokenType.RightBrace))
            {
                var identifier = IdentifierExpressionNode.Parse(stream, false);
                stream.Eat(separator);
                var value = ExpressionNode.ParseExpression(stream);

                result.Values.Add(identifier, value);

                if (stream.Check(DSharpTokenType.Comma))
                {
                    stream.Eat(DSharpTokenType.Comma);
                }

                break;
            }

            stream.Eat(DSharpTokenType.RightBrace);

            return result;
        }

        #endregion
    }
}
