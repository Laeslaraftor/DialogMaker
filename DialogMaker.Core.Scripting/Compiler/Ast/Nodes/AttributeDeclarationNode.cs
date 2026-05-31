using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Attribute declaration node
    /// </summary>
    /// <param name="token">Token that represents attribute name</param>
    public class AttributeDeclarationNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Fields of this attribute
        /// </summary>
        public List<AttributeFieldNode> Fields { get; set; } = [];

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Fields count: {Fields.Count}");

            foreach (var field in Fields)
            {
                builder.AppendLine(field.ToString());
            }

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse attribute declaration starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed attribute declaration</returns>
        public static AttributeDeclarationNode Parse(AstParserStream stream)
        {
            stream.Eat(DSharpTokenType.Attribute);
            var identifier = stream.Eat(DSharpTokenType.Identifier);
            AttributeDeclarationNode attribute = new(identifier);

            stream.Eat(DSharpTokenType.LeftParen);

            while (!stream.Check(DSharpTokenType.RightParen))
            {
                var field = AttributeFieldNode.Parse(stream);
                attribute.Fields.Add(field);

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DSharpTokenType.RightParen))
                {
                    stream.ThrowPositionException("Required attribute field");
                }
            }

            stream.Eat(DSharpTokenType.RightParen);
            stream.Eat(DSharpTokenType.Semicolon);

            return attribute;
        }

        #endregion
    }
}
