using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Compiler.Scopes;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public record struct DSharpCompiledDelegateInfo(DelegateExpressionNode Expression, DSharpCompilerMethodScope.CaptureInfo Closure, DSharpDelegateType DelegateInfo, DSharpMethodBuilder DelegateMethod);
}
