using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler
{
    internal class DelegateExpressionTypeProvider(DelegateExpressionNode delegateExpression) : IDSharpTypeProvider
    {
        private readonly DelegateExpressionNode _delegateExpression = delegateExpression;

        public IDSharpType GetType(DSharpCompilerContext context, params object?[] parameters)
        {
            if (context.Assembly == null)
            {
                throw new DSharpCompilerException("Unable to get delegate type: assembly not provided", _delegateExpression);
            }

            return _delegateExpression.GetRealExpressionType(context.Assembly, context, parameters.Length > 0 ? parameters[0] as IDSharpType : null);
        }
    }
}
