using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    internal class ExpressionTypeProvider(DSharpAssemblyBuilder assembly, ExpressionNode expression) : IDSharpTypeProvider
    {
        public IDSharpType GetType(DSharpCompilerContext context, params object?[] parameters)
        {
            IDSharpType? directRequiredType = null;

            if (parameters != null && parameters.Length == 1 && parameters[0] is IDSharpType typeParameter)
            {
                directRequiredType = typeParameter;
            }

            return expression.GetRealExpressionType(assembly, context, directRequiredType);
        }
    }
}
