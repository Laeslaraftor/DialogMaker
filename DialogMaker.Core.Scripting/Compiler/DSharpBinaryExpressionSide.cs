using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler
{
    public readonly struct DSharpBinaryExpressionSide(ExpressionNode value, ExpressionNode? parent)
    {
        public ExpressionNode? Parent { get; } = parent;
        public ExpressionNode Value { get; } = value;
    }
}
