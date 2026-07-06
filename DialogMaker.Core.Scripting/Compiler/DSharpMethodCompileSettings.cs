using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler
{
    public struct DSharpMethodCompileSettings
    {
        public Dictionary<string, DSharpFieldBuilder>? IdentifiersAsField { get; set; }
        public Dictionary<string, DSharpPropertyBuilder>? IdentifiersAsProperties { get; set; }
        public Dictionary<string, DSharpMethodBuilderParameter>? LocalVariables { get; set; }
        public HashSet<DSharpMethodBuilder>? AlwaysReturnMethods { get; set; }
        public HashSet<IDSharpMethodInfo>? CallingsToAwait { get; set; }
        public HashSet<ExpressionNode>? BannedExpressions { get; set; }
        public bool DoNotCompileEndPointMember { get; set; }
        public bool NextNonVirtualizedAccess { get; set; }
        public bool LastOperationIsReturnsValue { get; set; }

        public readonly bool BanExpression(ExpressionNode expression) => BannedExpressions?.Add(expression) == true;
        public readonly bool IsExpressionBanned(ExpressionNode? expression)
        {
            if (expression == null || BannedExpressions == null)
            {
                return false;
            }

            return BannedExpressions.Contains(expression);
        }
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
        public readonly bool Await(IDSharpMethodInfo method) => CallingsToAwait?.Contains(method) == true;
        public readonly bool AddReturnMethod(DSharpMethodBuilder method) => AlwaysReturnMethods?.Add(method) == true;
        public readonly bool AlwaysReturn(DSharpMethodBuilder method) => AlwaysReturnMethods?.Contains(method) == true;
    }
}
