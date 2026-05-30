using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Attribute field node
    /// </summary>
    /// <param name="token">Token that represents field name</param>
    public class AttributeFieldNode(DialogScriptToken token) : NamedNode(token)
    {
        /// <summary>
        /// Type of this field
        /// </summary>
        public TypeInfoNode? Type { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"Type: {Type}. {base.ToString()}";
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse attribute field starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed attribute field</returns>
        public static AttributeFieldNode Parse(AstParserStream stream)
        {
            var type = TypeInfoNode.Parse(stream, true, true);
            var identifier = stream.Eat(DialogScriptTokenType.Identifier);

            return new(identifier)
            {
                Type = type
            };
        }

        #endregion
    }
}
