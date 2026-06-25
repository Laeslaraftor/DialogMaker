using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public delegate IDSharpMemberInfo? MemberAccessExpressionEndPointHandler(ExpressionNode? previous, ExpressionNode endPoint, ref DSharpMethodCompileSettings settings, DSharpCompilerContext context);
}
