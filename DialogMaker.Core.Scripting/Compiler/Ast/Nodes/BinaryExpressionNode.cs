using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class BinaryExpressionNode(DialogScriptToken token) : ExpressionNode(token)
    {
        public ExpressionNode? Left { get; set; }
        public DialogScriptBinaryOperator Operator { get; set; }
        public ExpressionNode? Right { get; set; }

        #region Статика

        public static ExpressionNode ParseLogicalOr(AstParserStream stream)
        {
            return ParseOperation(stream, ParseLogicalAnd, DialogScriptTokenType.Or);
        }
        public static ExpressionNode ParseLogicalAnd(AstParserStream stream)
        {
            return ParseOperation(stream, ParseEquality, DialogScriptTokenType.And);
        }
        public static ExpressionNode ParseEquality(AstParserStream stream)
        {
            return ParseOperation(stream, ParseComparison, DialogScriptTokenType.Equal,
                                                           DialogScriptTokenType.NotEqual);
        }
        public static ExpressionNode ParseComparison(AstParserStream stream)
        {
            return ParseOperation(stream, ParseAdditive, DialogScriptTokenType.Less,
                                                         DialogScriptTokenType.Greater,
                                                         DialogScriptTokenType.LessEqual,
                                                         DialogScriptTokenType.GreaterEqual);
        }
        public static ExpressionNode ParseAdditive(AstParserStream stream)
        {
            return ParseOperation(stream, ParseMultiplicative, DialogScriptTokenType.Plus,
                                                               DialogScriptTokenType.Minus);
        }
        public static ExpressionNode ParseMultiplicative(AstParserStream stream)
        {
            return ParseOperation(stream, UnaryExpressionNode.Parse, DialogScriptTokenType.Multiply, 
                                                                     DialogScriptTokenType.Divide, 
                                                                     DialogScriptTokenType.Mod);
        }

        private static ExpressionNode ParseOperation(AstParserStream stream, Func<AstParserStream, ExpressionNode> parser, params DialogScriptTokenType[] tokens)
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
                    Operator = (DialogScriptBinaryOperator)op.Type,
                    Right = right,
                };
            }

            return left;
        }

        #endregion
    }
}
