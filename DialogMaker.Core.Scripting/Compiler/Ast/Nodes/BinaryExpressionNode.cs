using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class BinaryExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        public ExpressionNode? Left { get; set; }
        public DSharpBinaryOperator Operator { get; set; }
        public ExpressionNode? Right { get; set; }

        #region Статика

        public static ExpressionNode ParseLogicalOr(AstParserStream stream)
        {
            return ParseOperation(stream, ParseLogicalAnd, DSharpTokenType.Or);
        }
        public static ExpressionNode ParseLogicalAnd(AstParserStream stream)
        {
            return ParseOperation(stream, ParseEquality, DSharpTokenType.And);
        }
        public static ExpressionNode ParseEquality(AstParserStream stream)
        {
            return ParseOperation(stream, ParseComparison, DSharpTokenType.Equal,
                                                           DSharpTokenType.NotEqual);
        }
        public static ExpressionNode ParseComparison(AstParserStream stream)
        {
            return ParseOperation(stream, ParseAdditive, DSharpTokenType.Less,
                                                         DSharpTokenType.Greater,
                                                         DSharpTokenType.LessEqual,
                                                         DSharpTokenType.GreaterEqual);
        }
        public static ExpressionNode ParseAdditive(AstParserStream stream)
        {
            return ParseOperation(stream, ParseMultiplicative, DSharpTokenType.Plus,
                                                               DSharpTokenType.Minus);
        }
        public static ExpressionNode ParseMultiplicative(AstParserStream stream)
        {
            return ParseOperation(stream, UnaryExpressionNode.Parse, DSharpTokenType.Multiply, 
                                                                     DSharpTokenType.Divide, 
                                                                     DSharpTokenType.Mod);
        }

        private static ExpressionNode ParseOperation(AstParserStream stream, Func<AstParserStream, ExpressionNode> parser, params DSharpTokenType[] tokens)
        {
            var left = parser(stream);

            while (stream.Check(tokens))
            {
                if (stream.Current == null)
                {
                    break;
                }

                var op = stream.Eat(stream.Current.Type);
                var right = parser(stream);
                left = new BinaryExpressionNode(op)
                {
                    Left = left,
                    Operator = (DSharpBinaryOperator)op.Type,
                    Right = right,
                };
            }

            return left;
        }

        #endregion
    }
}
