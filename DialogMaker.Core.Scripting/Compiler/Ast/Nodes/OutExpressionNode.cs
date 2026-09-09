using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents out expression
    /// </summary>
    /// <param name="token">Token that represents out keyword</param>
    public class OutExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Type of variable that will be created for output value
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Identifier of variable/field/property for storing output value
        /// </summary>
        public ExpressionNode? Identifier { get; set; }

        #region Static

        /// <summary>
        /// Parse out expression node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed out expression node</returns>
        public static OutExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Out);
            OutExpressionNode result = new(token);

            if (TypeInfoNode.CanParse(stream, 0))
            {
                result.Type = TypeInfoNode.Parse(stream, true, true);
                result.Type.Parent = result;
            }

            if (result.Type != null &&
                result.Type.GenericParameters.Count == 0 &&
                !stream.Check(DSharpTokenType.Identifier))
            {
                result.Identifier = new IdentifierExpressionNode(result.Type.Token)
                {
                    Parent = result
                };
                result.Type = null;
            }
            else
            {
                result.Identifier = result.Type == null ? ParseIdentifier(stream, false) : IdentifierExpressionNode.Parse(stream, false);
                result.Identifier.Parent = result;
            }


            return result;
        }

        #endregion
    }
}
