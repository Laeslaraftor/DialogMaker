#if NETSTANDARD2_1_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    internal sealed class DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes members) : Attribute
    {
        public DynamicallyAccessedMemberTypes MemberTypes { get; } = members;
    }
}

#endif