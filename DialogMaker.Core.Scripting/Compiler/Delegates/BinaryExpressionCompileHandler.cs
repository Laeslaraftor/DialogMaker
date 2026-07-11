using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler
{
    public delegate void BinaryExpressionCompileHandler(DSharpBinaryExpressionSide left, DSharpBinaryExpressionSide right, DSharpBinaryOperator @operator, ref DSharpMethodCompileSettings settings);
}
