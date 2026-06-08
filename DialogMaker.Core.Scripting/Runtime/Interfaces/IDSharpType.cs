namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpType : IDSharpMemberInfo
    {
        public string? Namespace { get; }
        public string FullName { get; }

        public IDSharpType[] GetBaseTypes();
        public IDSharpMethodInfo[] GetMethods();
        public IDSharpMethodInfo? GetMethodOrDefault(Predicate<IDSharpMethodInfo> predicate);
        public IDSharpPropertyInfo[] GetProperties();
        public IDSharpPropertyInfo? GetPropertyOrDefault(Predicate<IDSharpPropertyInfo> predicate);
        public IDSharpFieldInfo[] GetFields();
        public IDSharpFieldInfo? GetFieldOrDefault(Predicate<IDSharpFieldInfo> predicate);
    }
}
