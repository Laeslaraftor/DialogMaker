using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics.CodeAnalysis;

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

            /*
            MemberAccess 
            {
                Left = identifier,
                Right = MemberAccess
                {
                    Left = identifier,
                    Right = identifier/MemberAccess
                }
            }

             */

            if (stream.Check(DSharpTokenType.Dot))
            {
                MemberAccessExpressionNode memberAccess;

                do
                {
                    var accessOperation = stream.Eat(DSharpTokenType.Dot);

                    memberAccess = new(accessOperation)
                    {
                        Target = root,
                        Member = ParseExpression(stream)
                    };

                    root = memberAccess;
                }
                while (stream.Check(DSharpTokenType.Dot));

                if (memberAccess.Member is AssignmentExpressionNode assignment)
                {
                    memberAccess.Member = assignment.Left;
                    assignment.Left = memberAccess;

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
                        currentBinary.Left = memberAccess;

                        return rootBinary!;
                    }
                }

                return memberAccess;
            }

            return root;
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

                    expression = new CallExpressionNode(callToken)
                    {
                        Callee = expression,
                        Arguments = args
                    };
                }
                else if (stream.Check(DSharpTokenType.LeftBracket))
                {
                    List<ExpressionNode> args = [];
                    var arrayAccessToken = CallExpressionNode.ParseArguments(stream, args, DSharpTokenType.LeftBracket, DSharpTokenType.RightBracket);

                    expression = new ArrayAccessExpressionNode(arrayAccessToken)
                    {
                        Array = expression,
                        Arguments = args
                    };
                }
                //else if (stream.Check(DSharpTokenType.As, DSharpTokenType.Is))
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
        public static ExpressionNode ParseExpression(AstParserStream stream)
        {
            bool previousIsMemberAccess = stream.Check(DSharpTokenType.Dot, -1);
            var left = BinaryExpressionNode.ParseLogicalOr(stream);

            if (previousIsMemberAccess)
            {
                return left;
            }

            if (AssignmentExpressionNode.TryParse(stream, out var assignment))
            {
                assignment.Left = left;
                return assignment;
            }
            if (stream.Check(DSharpTokenType.Increment))
            {
                var incrementToken = stream.Eat(DSharpTokenType.Increment);
                return new IncrementExpressionNode(incrementToken)
                {
                    Expression = left
                };
            }
            if (stream.Check(DSharpTokenType.Decrement))
            {
                var decrementToken = stream.Eat(DSharpTokenType.Decrement);
                return new DecrementExpressionNode(decrementToken)
                {
                    Expression = left
                };
            }

            if (!previousIsMemberAccess)
            {
                while (stream.Check(DSharpTokenType.Dot))
                {
                    var accessOperation = stream.Eat(DSharpTokenType.Dot);
                    left = new MemberAccessExpressionNode(accessOperation)
                    {
                        Target = left,
                        Member = ParseExpression(stream)
                    };
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
            if (stream.Check(DSharpTokenType.LeftParen))
            {
                return ParenContainedExpressionNode.Parse(stream);
            }
            if (stream.Check(DSharpTokenType.This) ||
                stream.Check(DSharpTokenType.Base) ||
                stream.Check(DSharpTokenType.Identifier))
            {
                return ParseIdentifierAccess(stream);
            }
            if (stream.Check(DSharpTokenType.New))
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
