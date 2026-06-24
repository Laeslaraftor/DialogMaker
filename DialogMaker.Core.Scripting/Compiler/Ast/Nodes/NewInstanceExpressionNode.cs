using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// New object instance expression
    /// </summary>
    /// <param name="token"><inheritdoc/></param>
    public class NewInstanceExpressionNode(DSharpToken token) : NewExpressionNode(token)
    {
        /// <summary>
        /// List of parameters for constructor
        /// </summary>
        public List<ExpressionNode> Parameters { get; set; } = [];
        /// <summary>
        /// List of properties initializers
        /// </summary>
        public List<AssignmentExpressionNode> PropertiesInitializer { get; set; } = [];

        #region Статика

        /// <summary>
        /// Parse new object instance expression
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="token">Token that represents new keyword</param>
        /// <param name="type">Type of new instancing object</param>
        /// <returns>Parsed new object instance expression</returns>
        public static NewInstanceExpressionNode Parse(AstParserStream stream, DSharpToken token, TypeInfoNode? type)
        {
            NewInstanceExpressionNode expression = new(token);
            bool leftParenExists = false;

            /*
            new();
            new()
            {
                Property = value
            };
            new SomeType();
            new SomeType() 
            {
                Property = value
            };
            new SomeType
            {
                Property = value
            };
            */

            if (type == null || stream.Check(DSharpTokenType.LeftParen))
            {
                stream.Eat(DSharpTokenType.LeftParen);
                leftParenExists = true;
                ParseExpressions(stream, expression.Parameters, DSharpTokenType.RightParen, "Required parameter");
                stream.Eat(DSharpTokenType.RightParen);
            }
            if (!stream.Check(DSharpTokenType.Semicolon))
            {
                if (!leftParenExists && !stream.Check(DSharpTokenType.LeftBrace))
                {
                    stream.ThrowPositionException("Required calling or properties initializers");
                }

                if (stream.Check(DSharpTokenType.LeftBrace))
                {
                    stream.Eat(DSharpTokenType.LeftBrace);

                    while (!stream.Check(DSharpTokenType.RightBrace))
                    {
                        var property = IdentifierExpressionNode.Parse(stream);
                        var assignmentOperator = stream.Eat(DSharpTokenType.Assign);
                        var value = ParseExpression(stream);

                        expression.PropertiesInitializer.Add(new(assignmentOperator)
                        {
                            Left = property,
                            Operator = DSharpAssignmentOperator.Assign,
                            Right = value
                        });

                        if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DSharpTokenType.RightBrace))
                        {
                            stream.ThrowPositionException("Required property initializer");
                        }
                    }

                    stream.Eat(DSharpTokenType.RightBrace);
                }
            }

            return expression;
        }

        #endregion
    }
}
