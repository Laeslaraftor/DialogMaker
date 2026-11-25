using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public delegate void BinaryExpressionCompileHandler(DSharpBinaryExpressionSide left, DSharpBinaryExpressionSide right, DSharpBinaryOperator @operator, ref DSharpMethodCompileSettings settings);
}
