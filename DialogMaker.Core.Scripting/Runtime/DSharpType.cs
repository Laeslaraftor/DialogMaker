using System.Collections.ObjectModel;

namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpType : DSharpMemberInfo
    {
        public string? Namespace { get; }
        public override string FullName
        {
            get
            {
                field ??= string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
                return field;
            }
        }
        public ReadOnlyCollection<DSharpPropertyInfo> Properties { get; }
        public ReadOnlyCollection<DSharpFieldInfo> Fields { get; }
        public ReadOnlyCollection<DSharpMethodInfo> Methods { get; }
    }
}
