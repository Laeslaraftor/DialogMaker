using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public struct DSharpMethodCompileSettings
    {
        public Dictionary<string, DSharpFieldBuilder>? IdentifiersAsField { get; set; }
        public Dictionary<string, DSharpPropertyBuilder>? IdentifiersAsProperties { get; set; }
        public Dictionary<VariableNode, DSharpMethodBuilderParameter>? LocalVariables { get; set; }
        public HashSet<CallExpressionNode>? CallingsToAwait { get; set; }

        public readonly bool TryGetVariable(string name, [NotNullWhen(true)] out DSharpMethodBuilderParameter? result)
        {
            result = null;

            if (LocalVariables != null)
            {
                foreach (var variable in LocalVariables.Values)
                {
                    if (variable.Name == name)
                    {
                        result = variable;
                        return true;
                    }
                }
            }

            return false;
        }
        public readonly DSharpMethodBuilderParameter GetVariable(string name)
        {
            if (TryGetVariable(name, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unknown variable: {name}", nameof(name));
        }
        public bool Await(CallExpressionNode callExpression) => CallingsToAwait?.Contains(callExpression) == true;
    }
}
