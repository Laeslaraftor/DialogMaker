using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Result of member searching
    /// </summary>
    /// <param name="memberInfo">Member that was found by searching</param>
    /// <param name="parameterInfo">Parameter that was found</param>
    /// <param name="methodCallingInfo">Method calling info for founded method</param>
    public readonly struct DSharpMemberSearchResult(IDSharpMemberInfo memberInfo, IDSharpParameterInfo? parameterInfo, DSharpMethodCallingInfo? methodCallingInfo)
    {
        public DSharpMemberSearchResult(IDSharpMemberInfo memberInfo)
            : this(memberInfo, null, null)
        {
        }
        public DSharpMemberSearchResult(DSharpMethodCallingInfo methodCallingInfo)
            : this(methodCallingInfo.Method, null, methodCallingInfo)
        {
        }
        public DSharpMemberSearchResult(IDSharpParameterInfo parameterInfo)
            : this(parameterInfo.Type, parameterInfo, null)
        {
        }

        /// <summary>
        /// Is search result empty
        /// </summary>
        public bool IsEmpty => MemberInfo == null && ParameterInfo == null && MethodCallingInfo == null;
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
