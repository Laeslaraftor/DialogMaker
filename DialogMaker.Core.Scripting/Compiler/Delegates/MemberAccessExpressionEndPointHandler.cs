using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public delegate IDSharpMemberInfo? MemberAccessExpressionEndPointHandler(ExpressionNode? previous, ExpressionNode endPoint, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context);
}
