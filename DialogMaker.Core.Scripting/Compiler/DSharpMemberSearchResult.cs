using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Result of member searching
    /// </summary>
    /// <param name="memberInfo">Member that was found by searching</param>
    /// <param name="parameterInfo">Parameter that was found</param>
    /// <param name="methodCallingInfo">Method calling info for founded method</param>
    public readonly struct DSharpMemberSearchResult(IDSharpMemberInfo memberInfo, bool isNullable, IDSharpParameterInfo? parameterInfo, DSharpMethodCallingInfo? methodCallingInfo)
    {
        public DSharpMemberSearchResult(IDSharpMemberInfo memberInfo, bool isNullable = false)
            : this(memberInfo, isNullable, null, null)
        {
        }
        public DSharpMemberSearchResult(DSharpMethodCallingInfo methodCallingInfo, bool isNullable = false)
            : this(methodCallingInfo.Method, isNullable, null, methodCallingInfo)
        {
        }
        public DSharpMemberSearchResult(IDSharpParameterInfo parameterInfo)
            : this(parameterInfo.Type, false, parameterInfo, null)
        {
        }
        public DSharpMemberSearchResult(DSharpMemberSearchResult other, bool isNullable)
            : this(other.MemberInfo, isNullable, other.ParameterInfo, other.MethodCallingInfo)
        {
        }

        /// <summary>
        /// Is search result empty
        /// </summary>
        public bool IsEmpty => MemberInfo == null && ParameterInfo == null && MethodCallingInfo == null;
        /// <summary>
        /// Is member should be nullable
        /// </summary>
        public bool IsNullable { get; } = isNullable;
        /// <summary>
        /// Founded member
        /// </summary>
        public IDSharpMemberInfo MemberInfo { get; } = memberInfo;
        /// <summary>
        /// Founded parameter
        /// </summary>
        public IDSharpParameterInfo? ParameterInfo { get; } = parameterInfo;
        /// <summary>
        /// Calling info for founded method
        /// </summary>
        public DSharpMethodCallingInfo? MethodCallingInfo { get; } = methodCallingInfo;
    }
}
