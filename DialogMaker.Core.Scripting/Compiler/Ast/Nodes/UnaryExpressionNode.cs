using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class UnaryExpressionNode(DialogScriptToken token) : ExpressionNode(token)
    {
        public DialogScriptUnaryOperator Operator { get; set; }
        public ExpressionNode? Operand { get; set; }

        #region Статика

        public static ExpressionNode Parse(AstParserStream stream)
        {
            if (stream.Current == null)
            {
                stream.ThrowPositionException("Invalid token");
            }

            if (stream.CheckAll<DialogScriptUnaryOperator>())
            {
                var op = stream.Eat(stream.Current.Type);
                var operand = Parse(stream);

                return new UnaryExpressionNode(op)
                {
                    Operator = (DialogScriptUnaryOperator)op.Type,
                    Operand = operand,
                };
            }

            if (stream.Check(DialogScriptTokenType.Await))
            {
                return AwaitExpressionNode.Parse(stream);
            }

            return ParsePrimary(stream);
        }

        #endregion
    }
}
