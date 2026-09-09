using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Base class of all expressions
    /// </summary>
    /// <param name="token">Token that represents some expression</param>
    public abstract class ExpressionNode(DSharpToken token) : AstNode(token)
    {
        #region Статика

        /// <summary>
        /// Parse expression that wrote with comma separator and write it's into buffer
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="buffer">Buffer for writing parsed expression</param>
        /// <param name="endToken">Token which indicated end of expressions list</param>
        /// <param name="noValueAfterCommaMessage">Message about not presenting value after comma</param>
        public static void ParseExpressions(AstParserStream stream, List<ExpressionNode> buffer, DSharpTokenType endToken, string noValueAfterCommaMessage)
        {
            while (!stream.Check(endToken))
            {
                var expression = ParseExpression(stream);
                buffer.Add(expression);

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream))
                {
                    stream.ThrowPositionException(noValueAfterCommaMessage);
                }
            }
        }
        /// <summary>
        /// Parse expression that wrote with comma separator and write it's into buffer
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="endToken">Token which indicated end of expressions list</param>
        /// <param name="noValueAfterCommaMessage">Message about not presenting value after comma</param>
        /// <returns>Parsed expressions</returns>
        public static List<ExpressionNode> ParseExpressions(AstParserStream stream, DSharpTokenType endToken, string noValueAfterCommaMessage)
        {
            List<ExpressionNode> buffer = [];
            ParseExpressions(stream, buffer, endToken, noValueAfterCommaMessage);

            return buffer;
        }

        /// <summary>
        /// Parse member access or raw type name like instance.SomeProperty, MyType.ContentData
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="parseGenerics">Flag which indicates that generic parameters must be parsed</param>
        /// <returns>Parsed member access node (<see cref="MemberAccessExpressionNode"/>) or identifier node (<see cref="IdentifierExpressionNode"/>)</returns>
        public static ExpressionNode ParseIdentifier(AstParserStream stream, bool parseGenerics = true)
        {
            ExpressionNode root;

            if (stream.Check(DSharpTokenType.This))
            {
                root = ThisExpressionNode.Parse(stream);
            }
            else if (stream.Check(DSharpTokenType.Base))
            {
                root = BaseExpressionNode.Parse(stream);
            }
            else
            {
                root = IdentifierExpressionNode.Parse(stream, parseGenerics);
            }

            return ParseMemberAccess(stream, root);
        }
        /// <summary>
        /// Parse member access if possible
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="target">Target access expression</param>
        /// <returns>Member access or target expression</returns>
        public static ExpressionNode ParseMemberAccess(AstParserStream stream, ExpressionNode target)
        {
            if (!stream.Check(DSharpTokenType.Dot))
            {
                return target;
            }

            MemberAccessExpressionNode memberAccess;

            do
            {
                var accessOperation = stream.Eat(DSharpTokenType.Dot);

                memberAccess = new(accessOperation)
                {
                    Target = target,
                    Member = ParseExpression(stream, true)
                };

                target.Parent = memberAccess;
                memberAccess.Member.Parent = memberAccess;

                target = memberAccess;
            }
            while (stream.Check(DSharpTokenType.Dot));

            if (memberAccess.Member is AssignmentExpressionNode assignment)
            {
                memberAccess.Member = assignment.Left;
                assignment.Left?.Parent = memberAccess;
                assignment.Left = memberAccess;
                memberAccess.Parent = assignment;

                return assignment;
            }

            var rootBinary = memberAccess.Member as BinaryExpressionNode;
            var currentBinary = rootBinary;

            while (currentBinary != null)
            {
                if (currentBinary.Left is BinaryExpressionNode nextBinary)
                {
                    currentBinary = nextBinary;
                }
                else
                {
                    memberAccess.Member = currentBinary.Left;
                    currentBinary.Left?.Parent = memberAccess;
                    currentBinary.Left = memberAccess;
                    memberAccess.Parent = currentBinary;

                    return rootBinary!;
                }
            }

            return memberAccess;
        }
        /// <summary>
        /// Parse member access or raw type name with possibility of method calling, array access, type checking
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed member access</returns>
        public static ExpressionNode ParseIdentifierAccess(AstParserStream stream)
        {
            ExpressionNode expression = ParseIdentifier(stream);

            while (true)
            {
                if (stream.Check(DSharpTokenType.LeftParen))
                {
                    List<ExpressionNode> args = [];
                    var callToken = CallExpressionNode.ParseArguments(stream, args);
                    CallExpressionNode callExpression = new(callToken)
                    {
                        Callee = expression,
                        Arguments = args
                    };

                    expression.Parent = callExpression;
                    args.SetParent(expression);

                    expression = callExpression;
                }
                else if (stream.Check(DSharpTokenType.LeftBracket))
                {
                    List<ExpressionNode> args = [];
                    var arrayAccessToken = CallExpressionNode.ParseArguments(stream, args, DSharpTokenType.LeftBracket, DSharpTokenType.RightBracket);
                    ArrayAccessExpressionNode arrayAccessExpression = new(arrayAccessToken)
                    {
                        Array = expression,
                        Arguments = args
                    };

                    expression.Parent = arrayAccessExpression;
                    args.SetParent(arrayAccessExpression);

                    expression = arrayAccessExpression;
                }
                else
                {
                    break;
                }
            }

            return ParseMemberAccess(stream, expression);
        }
        /// <summary>
        /// Parse literal value or array
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed literal value or array</returns>
        public static ExpressionNode ParseLiteralOrArray(AstParserStream stream)
        {
            if (stream.Check(DSharpTokenType.LeftBracket))
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
        public static ExpressionNode ParseExpression(AstParserStream stream, bool ignoreBinaryExpression = false)
        {
            bool previousIsMemberAccess = stream.Check(DSharpTokenType.Dot, -1);
            ExpressionNode left;

            if (ignoreBinaryExpression)
            {
                left = UnaryExpressionNode.Parse(stream);
            }
            else
            {
                left = BinaryExpressionNode.Parse(stream);
            }

            if (previousIsMemberAccess || stream.Check(DSharpTokenType.Colon))
            {
                return left;
            }

            if (AssignmentExpressionNode.TryParse(stream, out var assignment))
            {
                assignment.Left = left;
                left.Parent = assignment;
                return assignment;
            }
            if (stream.Check(DSharpTokenType.Question))
            {
                var conditionalExpression = ConditionalExpressionNode.Parse(stream);
                conditionalExpression.Condition = left;
                left.Parent = conditionalExpression;

                return conditionalExpression;
            }
            else if (stream.Check(DSharpTokenType.As))
            {
                var asExpression = AsExpressionNode.Parse(stream);
                asExpression.Expression = left;
                left.Parent = asExpression;

                return asExpression;
            }
            else if (stream.Check(DSharpTokenType.Increment))
            {
                var incrementToken = stream.Eat(DSharpTokenType.Increment);
                IncrementExpressionNode result = new(incrementToken)
                {
                    Expression = left
                };
                left.Parent = result;

                return result;
            }
            else if (stream.Check(DSharpTokenType.Decrement))
            {
                var decrementToken = stream.Eat(DSharpTokenType.Decrement);
                DecrementExpressionNode result = new(decrementToken)
                {
                    Expression = left
                };
                left.Parent = result;

                return result;
            }

            if (!previousIsMemberAccess)
            {
                while (stream.Check(DSharpTokenType.Dot))
                {
                    var accessOperation = stream.Eat(DSharpTokenType.Dot);
                    MemberAccessExpressionNode memberAccess = new(accessOperation)
                    {
                        Target = left,
                        Member = ParseExpression(stream)
                    };
                    left.Parent = memberAccess;

                    left = memberAccess;
                }
            }

            return left;
        }

        /// <summary>
        /// Parse primary expression like literal value, array, variable/property access, 
        /// method/function invocation or new instance creation
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns></returns>
        public static ExpressionNode ParsePrimary(AstParserStream stream)
        {
            if (stream.Check(DSharpTokenType.Throw))
            {
                return ThrowExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Out))
            {
                return OutExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.Ref))
            {
                return RefExpressionNode.Parse(stream);
            }
            if (DelegateExpressionNode.IsDelegate(stream))
            {
                return DelegateExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.TypeOf))
            {
                return TypeOfExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.NameOf))
            {
                return NameOfExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.SizeOf))
            {
                return SizeOfExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.LeftParen))
            {
                if (CastExpressionNode.IsCast(stream))
                {
                    return CastExpressionNode.Parse(stream);
                }

                return ParenContainedExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.This) ||
                stream.Check(DSharpTokenType.Base) ||
                stream.Check(DSharpTokenType.Identifier))
            {
                return ParseIdentifierAccess(stream);
            }
            if (stream.Check(DSharpTokenType.New) ||
                stream.Check(DSharpTokenType.Stackalloc))
            {
                return NewExpressionNode.Parse(stream);
            }
            if (LiteralExpressionNode.TryParse(stream, out var literalExpression))
            {
                return literalExpression;
            }
            if (stream.Check(DSharpTokenType.LeftBracket))
            {
                return ArrayExpressionNode.Parse(stream);
            }
            if (TypeInfoNode.CanParseIdentifier(stream) && stream.Check(DSharpTokenType.Dot, 1))
            {
                return ParseIdentifier(stream);
            }

            stream.ThrowUnexpectedTokenException();

            return null;
        }

        #endregion
    }
}
