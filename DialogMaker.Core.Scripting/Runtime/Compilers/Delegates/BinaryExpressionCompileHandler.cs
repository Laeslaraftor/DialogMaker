using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public delegate void BinaryExpressionCompileHandler(ExpressionNode left, ExpressionNode right, DSharpBinaryOperator @operator);
}
