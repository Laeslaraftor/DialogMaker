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

        public int ArrayDimensions => throw new NotImplementedException();

        public DSharpObjectType ObjectType => throw new NotImplementedException();

        public bool IsGeneric => throw new NotImplementedException();

        public bool IsAbstract => throw new NotImplementedException();

        public bool IsSealed => throw new NotImplementedException();

        public IDSharpType? GenericTemplate => throw new NotImplementedException();

        public IDSharpMethodInfo? Finalizer => throw new NotImplementedException();

        public int Size => throw new NotImplementedException();

        #region Управление

        public IDSharpMethodInfo[] GetMethods() => [.. Methods];
        public IDSharpMethodInfo[] GetMethods(Predicate<IDSharpMethodInfo> predicate) => [.. Methods.Where(m => predicate(m))];
        public IDSharpPropertyInfo[] GetProperties() => [.. Properties];
        public IDSharpPropertyInfo[] GetProperties(Predicate<IDSharpPropertyInfo> predicate) => [.. Properties.Where(p => predicate(p))];
        public IDSharpFieldInfo[] GetFields() => [.. Fields];
        public IDSharpFieldInfo[] GetFields(Predicate<IDSharpFieldInfo> predicate) => [.. Fields.Where(f => predicate(f))];
        public IDSharpType[] GetBaseTypes() => [.. BaseTypes];

        public IDSharpMethodInfo[] GetConstructors()
        {
            throw new NotImplementedException();
        }

        public IDSharpMethodInfo[] GetConstructors(Predicate<IDSharpMethodInfo> predicate)
        {
            throw new NotImplementedException();
        }

        public IDSharpType[] GetGenericParameters()
        {
            throw new NotImplementedException();
        }

        public IDSharpType[] GetGenericTypes()
        {
            throw new NotImplementedException();
        }

        public IDSharpType[] GetChildrenTypes()
        {
            throw new NotImplementedException();
        }

        public IDSharpIndexerInfo[] GetIndexers()
        {
            throw new NotImplementedException();
        }

        public IDSharpIndexerInfo[] GetIndexers(Predicate<IDSharpIndexerInfo> predicate)
        {
            throw new NotImplementedException();
        }

        public IDSharpOperatorInfo[] GetCastOperators()
        {
            throw new NotImplementedException();
        }

        public IDSharpOperatorInfo[] GetOperators()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
