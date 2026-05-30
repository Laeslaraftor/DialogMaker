using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class CallExpressionNode(DialogScriptToken token) : ExpressionNode(token)
    {
        public ExpressionNode? Callee { get; set; }
        public List<ExpressionNode> Arguments { get; set; } = [];
    }
}
