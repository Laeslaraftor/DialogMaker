using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

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

            if (Attributes != null && Attributes.Count > 0)
            {
                builder.AppendLine("Attributes:");

                foreach (var attribute in Attributes)
                {
                    builder.AppendLine(attribute.ToString());
                }
            }
            if (Initializer != null)
            {
                builder.AppendLine("Initializer:");
                builder.Append(Initializer.ToString());
            }

            return builder.ToString().TrimEnd();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Check next tokens is variable definition;
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Is variable definition</returns>
        public static bool IsVariable(AstParserStream stream)
        {
            bool canParseIdentifier = TypeInfoNode.CanParseIdentifier(stream);

            return canParseIdentifier && stream.Check(DSharpTokenType.Identifier, 1) && stream.Check(DSharpTokenType.Semicolon, 2) ||
                   canParseIdentifier && stream.Check(DSharpTokenType.Identifier, 1) && stream.Check(DSharpTokenType.Assign, 2);
        }
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
