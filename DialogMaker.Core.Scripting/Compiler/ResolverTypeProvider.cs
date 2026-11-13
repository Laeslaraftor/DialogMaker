using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    internal class ResolverTypeProvider(DSharpAssemblyBuilder assembly, TypeInfoNode typeInfo) : IDSharpTypeProvider
    {
        public IDSharpType GetType(DSharpCompilerContext context, params object?[] parameters)
        {
            return (IDSharpType)assembly.GetType(context.ResolveType(typeInfo));
        }
    }
}
