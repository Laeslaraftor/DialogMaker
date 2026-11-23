using DialogMaker.Core.Scripting.Compiler;
using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Builders;

namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// D# preset value that can be used for parameters default value or attributes value
    /// </summary>
    /// <param name="Member">Accessed static member or type</param>
    /// <param name="CallingInfo">Information about calling method that return value</param>
    /// <param name="LiteralValue">Constant value</param>
    /// <param name="IsMethodAsDelegate">Member is delegate and required create delegate</param>
    public record struct DSharpPresetValue(IDSharpMemberInfo? Member, DSharpMethodCallingInfo? CallingInfo, DSharpLiteralValue? LiteralValue, bool IsMethodAsDelegate)
    {
        public DSharpPresetValue(IDSharpMemberInfo member, bool IsMethodAsDelegate = false)
            : this(member, null, null, IsMethodAsDelegate)
        {
        }
        public DSharpPresetValue(DSharpLiteralValue literalValue)
            : this(null, null, literalValue, false)
        {
        }
        public DSharpPresetValue(DSharpMethodCallingInfo callingInfo)
            : this(null, callingInfo, null, false)
        {
        }

        /// <summary>
        /// Is null preset value
        /// </summary>
        public readonly bool IsNull => LiteralValue?.IsNull == true;

        /// <summary>
        /// Get type that returns current preset value
        /// </summary>
        /// <param name="assembly">Assembly for searching some types</param>
        /// <returns>Type of value that returns by current preset value</returns>
        /// <exception cref="InvalidOperationException">Unable to get type for literal</exception>
        /// <exception cref="InvalidOperationException">Unable to get return type</exception>
        public readonly IDSharpType GetReturnType(IDSharpAssembly assembly)
        {
            if (Member is IDSharpType)
            {
                return assembly.GetType(DSharpBuildInTypes.Extra.Type);
            }
            if (IsMethodAsDelegate)
            {
                if (Member is not IDSharpMethodInfo method)
                {
                    throw new InvalidOperationException("Delegate value requires method");
                }
                if (assembly is not DSharpAssemblyBuilder assemblyBuilder)
                {
                    throw new InvalidOperationException("Delegate value requires assembly builder");
                }

                return method.GetDelegateType(assemblyBuilder);
            }
            else if (LiteralValue != null)
            {
                if (DSharpBuildInTypes.TryGetType(assembly, LiteralValue.Value.Type.ToToken(), out var type))
                {
                    return type;
                }

                throw new InvalidOperationException($"Unable to get type for literal: \"{LiteralValue}\"");
            }
            else if (Member != null && Member.TryGetReturnType(out var returnType))
            {
                return returnType;
            }

            throw new InvalidOperationException("Unable to get return type");
        }
    }
}
