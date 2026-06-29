using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents values enumerations
    /// </summary>
    /// <param name="token">Token of values enumeration</param>
    public class EnumNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Values list
        /// </summary>
        public List<LiteralExpressionNode> Members { get; set; } = [];

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Members count: {Members.Count}");

            foreach (var member in Members)
            {
                builder.AppendLine(member.ToString());
            }

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse values enumerations starts at current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed enum node</returns>
        public static EnumNode Parse(AstParserStream stream)
        {
            stream.Eat(DSharpTokenType.Enum);
            var nameToken = stream.Eat(DSharpTokenType.Identifier);
            EnumNode enumNode = new(nameToken);

            stream.Eat(DSharpTokenType.LeftBrace);

            int memberIndex = 0;

            while (!stream.Check(DSharpTokenType.RightBrace))
            {
                var memberToken = stream.Eat(DSharpTokenType.Identifier);
                LiteralExpressionNode member;

                if (stream.Check(DSharpTokenType.Assign))
                {
                    stream.Eat(DSharpTokenType.Assign);
                    member = LiteralExpressionNode.Parse(stream);
                }
                else
                {
                    member = new(memberToken)
                    {
                        Value = memberIndex,
                        Type = DSharpLiteralType.Int
                    };
                }

                enumNode.Members.Add(member);

                if (stream.Check(DSharpTokenType.Comma))
                {
                    stream.Eat(DSharpTokenType.Comma);
                }

                memberIndex++;
            }

            stream.Eat(DSharpTokenType.RightBrace);

            return enumNode;
        }

        #endregion
    }
}
