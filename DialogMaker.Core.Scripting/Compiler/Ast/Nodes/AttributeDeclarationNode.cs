using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Attribute declaration node
    /// </summary>
    /// <param name="token">Token that represents attribute name</param>
    public class AttributeDeclarationNode(DialogScriptToken token) : NamedNode(token)
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
            stream.Eat(DialogScriptTokenType.Attribute);
            var identifier = stream.Eat(DialogScriptTokenType.Identifier);
            AttributeDeclarationNode attribute = new(identifier);

            stream.Eat(DialogScriptTokenType.LeftParen);

            while (!stream.Check(DialogScriptTokenType.RightParen))
            {
                var field = AttributeFieldNode.Parse(stream);
                attribute.Fields.Add(field);

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DialogScriptTokenType.RightParen))
                {
                    stream.ThrowPositionException("Required attribute field");
                }
            }

            stream.Eat(DialogScriptTokenType.RightParen);
            stream.Eat(DialogScriptTokenType.Semicolon);

            return attribute;
        }

        #endregion
    }
}
