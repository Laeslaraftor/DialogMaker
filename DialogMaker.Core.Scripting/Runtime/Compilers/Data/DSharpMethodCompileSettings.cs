using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Compilers
{
    public struct DSharpMethodCompileSettings
    {
        public Dictionary<string, DSharpFieldBuilder>? IdentifiersAsField { get; set; }
        public Dictionary<string, DSharpPropertyBuilder>? IdentifiersAsProperties { get; set; }
        public Dictionary<string, DSharpMethodBuilderParameter>? LocalVariables { get; set; }
        public HashSet<DSharpMethodBuilder>? AlwaysReturnMethods { get; set; }
        public HashSet<CallExpressionNode>? CallingsToAwait { get; set; }
        public bool DoNotCompileEndPointMember { get; set; }

        public readonly bool TryGetVariable(string name, [NotNullWhen(true)] out DSharpMethodBuilderParameter? result)
        {
            result = null;

            if (LocalVariables != null && LocalVariables.TryGetValue(name, out result))
            {
                return true;
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
        public readonly bool RemoveVariable(DSharpMethodBuilderParameter variable)
        {
            if (LocalVariables != null)
            {
                string? key = null;

                foreach (var info in LocalVariables)
                {
                    if (info.Value == variable)
                    {
                        key = info.Key;
                        break;
                    }
                }

                if (key != null)
                {
                    return LocalVariables.Remove(key);
                }

                return false;
            }

            return false;
        }
        public readonly bool RemoveVariable(string name)
        {
            if (LocalVariables != null)
            {
                return LocalVariables.Remove(name);
            }

            return false;
        }
        public readonly bool Await(CallExpressionNode callExpression) => CallingsToAwait?.Contains(callExpression) == true;
        public readonly bool AddReturnMethod(DSharpMethodBuilder method) => AlwaysReturnMethods?.Add(method) == true;
        public readonly bool AlwaysReturn(DSharpMethodBuilder method) => AlwaysReturnMethods?.Contains(method) == true;
    }
}
