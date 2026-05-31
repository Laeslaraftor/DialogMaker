using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Variable node
    /// </summary>
    /// <param name="token">Token that represents variable name</param>
    public class VariableNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Attributes of this variable
        /// </summary>
        public List<AttributeNode>? Attributes { get; set; }
        /// <summary>
        /// Type of this variable
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Initializer for this variable
        /// </summary>
        public ExpressionNode? Initializer { get; set; }

        #region Управление

        /// <summary>
        /// Parse variable start with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="attributes">List of attributes</param>
        /// <param name="eatEnding">A flag that indicates that the final token must be eaten.</param>
        /// <returns>Parsed variable</returns>
        public static VariableNode ParseVariable(AstParserStream stream, List<AttributeNode>? attributes, bool eatEnding = true)
        {
            TypeInfoNode variableType;

            if (stream.Check(DSharpTokenType.Var))
            {
                variableType = new(stream.Eat(DSharpTokenType.Var));
            }
            else
            {
                variableType = TypeInfoNode.Parse(stream, true, true);
            }

            var nameToken = stream.Eat(DSharpTokenType.Identifier);
            VariableNode variable = new(nameToken)
            {
                Type = variableType,
                Attributes = attributes
            };

            if (stream.Check(DSharpTokenType.Assign))
            {
                stream.Eat(DSharpTokenType.Assign);
                variable.Initializer = ExpressionNode.ParseExpression(stream);
            }
            if (eatEnding)
            {
                stream.Eat(DSharpTokenType.Semicolon);
            }           

            return variable;
        }

        #endregion
    }
}
