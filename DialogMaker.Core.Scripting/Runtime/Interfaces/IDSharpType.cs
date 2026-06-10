namespace DialogMaker.Core.Scripting.Runtime
{
    public interface IDSharpType : IDSharpMemberInfo
    {
        public string? Namespace { get; }
        public string FullName { get; }

        public IDSharpType[] GetBaseTypes();
        public IDSharpMethodInfo[] GetConstructors();
        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate);
        public IDSharpMethodInfo[] GetMethods();
        public IDSharpMethodInfo[] GetMethods(Predicate<IDSharpMethodInfo> predicate);
        public IDSharpPropertyInfo[] GetProperties();
        public IDSharpPropertyInfo[] GetProperties(Predicate<IDSharpPropertyInfo> predicate);
        public IDSharpFieldInfo[] GetFields();
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate);
    }
}
