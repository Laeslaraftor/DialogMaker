using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class ExpressionStatementNode(DialogScriptToken token) : StatementNode(token)
    {
        public ExpressionNode? Expression { get; set; }
    }
}
