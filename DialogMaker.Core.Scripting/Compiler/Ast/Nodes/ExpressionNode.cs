using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Base class of all expressions
    /// </summary>
    /// <param name="token">Token that represents some expression</param>
    public abstract class ExpressionNode(DialogScriptToken token) : NamedNode(token)
    {
        #region Статика

        /// <summary>
        /// Parse member access or raw type name like instance.SomeProperty, MyType.ContentData
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed member access node (<see cref="MemberAccessExpressionNode"/>) or identifier node (<see cref="IdentifierExpressionNode"/>)</returns>
        public static ExpressionNode ParseIdentifier(AstParserStream stream)
        {
            var identifier = stream.Eat(DialogScriptTokenType.Identifier);

            if (stream.Check(DialogScriptTokenType.Dot))
            {
                ExpressionNode root = new IdentifierExpressionNode(identifier);
                MemberAccessExpressionNode memberAccess;

                do
                {
                    var accessOperation = stream.Eat(DialogScriptTokenType.Dot);
                    var memberIdentifier = stream.Eat(DialogScriptTokenType.Identifier);

                    memberAccess = new(accessOperation)
                    {
                        Target = root,
                        Member = new IdentifierExpressionNode(memberIdentifier)
                    };

                    root = memberAccess;
                }
                while (stream.Check(DialogScriptTokenType.Dot));

                return memberAccess;
            }

            return new IdentifierExpressionNode(identifier);
        }
        /// <summary>
        /// parse member access or raw type name with possibility of method calling, array access, type checking
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ExpressionNode ParseIdentifierAccess(AstParserStream stream)
        {
            ExpressionNode expression = ParseIdentifier(stream);

            while (true)
            {
                if (stream.Check(DialogScriptTokenType.LeftParen))
                {
                    var callToken = stream.Eat(DialogScriptTokenType.LeftParen);
                    List<ExpressionNode> args = [];

                    while (!stream.Check(DialogScriptTokenType.RightParen))
                    {
                        args.Add(ParseExpression(stream));

                        if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DialogScriptTokenType.RightParen))
                        {
                            stream.ThrowPositionException("Required argument expression");
                        }
                    }

                    stream.Eat(DialogScriptTokenType.RightParen);
                    expression = new CallExpressionNode(callToken)
                    {
                        Callee = expression,
                        Arguments = args
                    };
                }
                else if (stream.Check(DialogScriptTokenType.LeftBracket))
                {
                    var arrayAccessToken = stream.Eat(DialogScriptTokenType.LeftBracket);
                    var index = ParseExpression(stream);

                    stream.Eat(DialogScriptTokenType.RightBracket);
                    expression = new ArrayAccessExpressionNode(arrayAccessToken)
                    {
                        Array = expression,
                        Index = index
                    };
                }
                //else if (stream.Check(DialogScriptTokenType.As, DialogScriptTokenType.Is))
                //{
                //}
                else
                {
                    break;
                }
            }

            return expression;
        }
        /// <summary>
        /// Parse literal value or array
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed literal value or array</returns>
        public static ExpressionNode ParseLiteralOrArray(AstParserStream stream)
        {
            if (stream.Check(DialogScriptTokenType.LeftBracket))
            {
                return ArrayExpressionNode.Parse(stream);
            }

            return LiteralExpressionNode.Parse(stream);
        }
        /// <summary>
        /// Parse some expression
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed expression</returns>
        public static ExpressionNode ParseExpression(AstParserStream stream)
        {
            var left = BinaryExpressionNode.ParseLogicalOr(stream);

            if (AssignmentExpressionNode.TryParse(stream, out var assignment))
            {
                assignment.Left = left;
                return assignment;
            }
            if (stream.Check(DialogScriptTokenType.Increment))
            {
                var incrementToken = stream.Eat(DialogScriptTokenType.Increment);
                return new IncrementExpressionNode(incrementToken)
                {
                    Expression = left
                };
            }
            if (stream.Check(DialogScriptTokenType.Decrement))
            {
                var decrementToken = stream.Eat(DialogScriptTokenType.Decrement);
                return new DecrementExpressionNode(decrementToken)
                {
                    Expression = left
                };
            }
            while (stream.Check(DialogScriptTokenType.Dot))
            {
                var accessOperation = stream.Eat(DialogScriptTokenType.Dot);
                left = new MemberAccessExpressionNode(accessOperation)
                {
                    Target = left,
                    Member = ParseExpression(stream)
                };
            }

            return left;
        }
        
        /// <summary>
        /// Parse primary expression like literal value, array, variable/property access or method/function invocation
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ExpressionNode ParsePrimary(AstParserStream stream)
        {
            if (stream.Check(DialogScriptTokenType.LeftParen))
            {
                stream.Eat(DialogScriptTokenType.LeftParen);
                var expr = ParseExpression(stream);
                stream.Eat(DialogScriptTokenType.RightParen);
                return expr;
            }
            if (LiteralExpressionNode.TryParse(stream, out var literalExpression))
            {
                return literalExpression;
            }
            if (stream.Check(DialogScriptTokenType.LeftBracket))
            {
                return ArrayExpressionNode.Parse(stream);
            }
            if (stream.Check(DialogScriptTokenType.Identifier))
            {
                return ParseIdentifierAccess(stream);
            }

            stream.ThrowUnexpectedTokenException();

            return null;
        }

        #endregion
    }
}
