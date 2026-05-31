using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class TypeCheckExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        public ExpressionNode? Expression { get; set; }
        public string? Operator { get; set; } // "is" или "as"
        public TypeInfoNode? TypeName { get; set; }
    }
}
