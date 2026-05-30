using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class IdentifierExpressionNode(DialogScriptToken token) : ExpressionNode(token)
    {
    }
}
