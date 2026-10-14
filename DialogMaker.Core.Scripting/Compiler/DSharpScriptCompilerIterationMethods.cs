using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        private DSharpMethodBuilder CompileIterationMethod(DSharpMethodBuilder method, BlockStatementNode body, ref DSharpMethodCompileSettings settings, ref DSharpCompilerContext context)
        {
            var iterationMethodInfo = DSharpIterationMethodInfo.Create(method, (AstNode?)_createdMethods.GetValueOrDefault(method) ?? body);

            return method;
        }
    }
}
