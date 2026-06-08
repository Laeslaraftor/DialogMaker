using System.Collections.ObjectModel;

namespace DialogMaker.Core.Scripting.Runtime
{
    public abstract class DSharpType : DSharpMemberInfo, IDSharpType
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
        public ReadOnlyCollection<DSharpType> BaseTypes { get; }
        public ReadOnlyCollection<DSharpPropertyInfo> Properties { get; }
        public ReadOnlyCollection<DSharpFieldInfo> Fields { get; }
        public ReadOnlyCollection<DSharpMethodInfo> Methods { get; }

        #region Управление

        public IDSharpMethodInfo[] GetMethods() => [.. Methods];
        public IDSharpMethodInfo? GetMethodOrDefault(Predicate<IDSharpMethodInfo> predicate) => Methods.FirstOrDefault(m => predicate(m));
        public IDSharpPropertyInfo[] GetProperties() => [.. Properties];
        public IDSharpFieldInfo[] GetFields() => [.. Fields];
        public IDSharpPropertyInfo? GetPropertyOrDefault(Predicate<IDSharpPropertyInfo> predicate) => Properties.FirstOrDefault(p => predicate(p));
        public IDSharpFieldInfo? GetFieldOrDefault(Predicate<IDSharpFieldInfo> predicate) => Fields.FirstOrDefault(f => predicate(f));
        public IDSharpType[] GetBaseTypes() => [.. BaseTypes];

        #endregion
    }
}
