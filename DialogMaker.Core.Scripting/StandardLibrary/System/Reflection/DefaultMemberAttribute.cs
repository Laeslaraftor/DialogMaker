namespace System.Reflection;

public class DefaultMemberAttribute : Attribute
{
    public DefaultMemberAttribute(string memberName)
    {
        MemberName = memberName;
    }

    public string MemberName { get; }
}