using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class UnaryExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        public DSharpUnaryOperator Operator { get; set; }
        public ExpressionNode? Operand { get; set; }

        #region Статика

        public static ExpressionNode Parse(AstParserStream stream)
        {
            if (stream.Current == null)
            {
                stream.ThrowPositionException("Invalid token");
            }

            if (stream.CheckAll<DSharpUnaryOperator>())
            {
                var op = stream.Eat(stream.Current.Type);
                var operand = Parse(stream);

                return new UnaryExpressionNode(op)
                {
                    Operator = (DSharpUnaryOperator)op.Type,
                    Operand = operand,
                };
            }

            if (stream.Check(DSharpTokenType.Await))
            {
                return AwaitExpressionNode.Parse(stream);
            }

            return ParsePrimary(stream);
        }

        #endregion
    }
}
