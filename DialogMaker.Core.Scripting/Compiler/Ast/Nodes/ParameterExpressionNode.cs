using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Expression that represents parameter declaration
    /// </summary>
    /// <param name="token">Token that represents identifier of parameter</param>
    public class ParameterExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Type of parameter
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Parameter mode
        /// </summary>
        public DSharpMethodParameterMode Mode { get; set; }
        /// <summary>
        /// Expression of default value
        /// </summary>
        public ExpressionNode? DefaultValueExpression { get; set; }

        #region Статика

        /// <summary>
        /// Parse parameter expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="allowType">Allow parameter type definition</param>
        /// <param name="allowDefaultValue">Allow default value definition</param>
        /// <returns>Parsed parameter expression</returns>
        public static ParameterExpressionNode Parse(AstParserStream stream, bool allowType = true, bool allowDefaultValue = true)
        {
            DSharpMethodParameterMode mode = DSharpMethodParameterMode.Default;

            if (stream.Check(DSharpTokenType.Ref))
            {
                mode = DSharpMethodParameterMode.Ref;
                stream.Eat(DSharpTokenType.Ref);
            }
            else if (stream.Check(DSharpTokenType.Out))
            {
                mode = DSharpMethodParameterMode.Out;
                stream.Eat(DSharpTokenType.Out);
            }

            TypeInfoNode? typeInfo = null;

            if (allowType)
            {
                typeInfo = TypeInfoNode.Parse(stream, true, true);
            }
            
            var identifier = stream.Eat(DSharpTokenType.Identifier);

            ParameterExpressionNode result = new(identifier)
            {
                Mode = mode,
                Type = typeInfo
            };

            if (stream.Check(DSharpTokenType.Assign))
            {
                if (!allowDefaultValue)
                {
                    stream.ThrowPositionException("Parameter default value not allowed in current context");
                }

                stream.Check(DSharpTokenType.Assign);
                result.DefaultValueExpression = ParseExpression(stream);
            }

            return result;
        }
        /// <summary>
        /// Parse list of parameters
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="endToken">Token that indicated end of parameters list</param>
        /// <param name="allowType">Allow parameter type definition</param>
        /// <param name="allowDefaultValue">Allow default value definition</param>
        /// <returns>List of parsed parameters</returns>
        public static List<ParameterExpressionNode> ParseMultiple(AstParserStream stream, DSharpTokenType endToken = DSharpTokenType.RightParen, bool allowType = true, bool allowDefaultValue = true)
        {
            List<ParameterExpressionNode> buffer = [];
            ParseMultiple(stream, buffer, endToken, allowType, allowDefaultValue);

            return buffer;
        }
        /// <summary>
        /// Parse list of parameters
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="buffer">Buffer for writing parsed parameters</param>
        /// <param name="endToken">Token that indicated end of parameters list</param>
        /// <param name="allowType">Allow parameter type definition</param>
        /// <param name="allowDefaultValue">Allow default value definition</param>
        public static void ParseMultiple(AstParserStream stream, List<ParameterExpressionNode> buffer, DSharpTokenType endToken = DSharpTokenType.RightParen, bool allowType = true, bool allowDefaultValue = true)
        {
            while (!stream.Check(endToken))
            {
                var parameter = Parse(stream, allowType, allowDefaultValue);
                buffer.Add(parameter);

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream, endToken))
                {
                    stream.ThrowPositionException("Expected parameter");
                }
            }
        }

        #endregion
    }
}
