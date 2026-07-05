using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Compiler.Scopes
{
    /// <summary>
    /// File scope
    /// </summary>
    /// <param name="assembly">Compiling assembly</param>
    /// <param name="parent">Parent scope</param>
    public class DSharpCompilerFileScope(DSharpAssemblyBuilder assembly, DSharpCompilerScope? parent)
        : DSharpCompilerScope(assembly, parent)
    {
        /// <summary>
        /// Used namespaces in this scope (by <c>using</c> or declaration)
        /// </summary>
        public List<string> Namespaces { get; } = [];

        protected override IEnumerable<IDSharpMemberInfo> GetMembers()
        {
            return Assembly.GlobalVariables.Cast<IDSharpMemberInfo>().Union(Assembly.GlobalFunctions);
        }
        protected override IEnumerable<IDSharpType> GetTypes(string name)
        {
            bool IsValid(IDSharpType type)
            {
                if (type.IsGeneric)
                {
                    return false;
                }

                var nameEquality = IsNameEquals(type, name);

                if (nameEquality == TypeNameEqualityType.None)
                {
                    return false;
                }
                if (nameEquality != TypeNameEqualityType.Name)
                {
                    return true;
                }

                var @namespace = type.Namespace;

                return @namespace == null ||
                       @namespace != null && Namespaces.Contains(@namespace);
            }

            foreach (var type in Assembly.Types)
            {
                if (IsValid(type))
                {
                    yield return type;
                }
            }
            foreach (var reference in Assembly.References)
            {
                foreach (var type in reference.Types)
                {
                    if (IsValid(type))
                    {
                        yield return type;
                    }
                }
            }
        }
        protected override IEnumerable<IDSharpParameterInfo> GetVariables()
        {
            yield break;
        }
        protected override IDSharpParameterInfo? CreateLocalVariable(string name, IDSharpType type)
        {
            return null;
        }

        #region Статика

        /// <summary>
        /// Get type name that includes declaring type and namespace
        /// </summary>
        /// <param name="type">Type for getting full name</param>
        /// <returns>Full name of specified type</returns>
        public static string GetTypeFullName(IDSharpType type)
        {
            string name = GetDeclaringFullName(type, out var declaringType);

            if (declaringType?.Namespace != null)
            {
                name = $"{declaringType.Namespace}.{name}";
            }

            return name;
        }
        /// <summary>
        /// Get type name that includes all declaring type names
        /// </summary>
        /// <param name="type">Type for getting name with declaring type names</param>
        /// <returns>Name of specified type with declaring type names</returns>
        public static string GetDeclaringFullName(IDSharpType type)
        {
            return GetDeclaringFullName(type, out _);
        }
        /// <summary>
        /// Get type name that includes all declaring type names
        /// </summary>
        /// <param name="type">Type for getting name with declaring type names</param>
        /// <param name="rootType">Root declaring type</param>
        /// <returns>Name of specified type with declaring type names</returns>
        public static string GetDeclaringFullName(IDSharpType type, out IDSharpType? rootType)
        {
            string name = string.Empty;
            rootType = type;

            while (rootType != null)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = rootType.Name;
                }
                else
                {
                    name = $"{rootType.Name}.{name}";
                }

                if (rootType.DeclaringType == null)
                {
                    break;
                }

                rootType = rootType.DeclaringType;
            }

            return name;
        }
        /// <summary>
        /// Compare specified name with type name.
        /// Specified name can be short (only type name) or contains namespace or/and declaring types
        /// </summary>
        /// <param name="type">Type to compare name</param>
        /// <param name="name">Name to compare</param>
        /// <param name="onlyShortName">Compare only type name without namespace and declaring type</param>
        /// <returns>Name equality type</returns>
        public static TypeNameEqualityType IsNameEquals(IDSharpType type, string name, TypeNameEqualityCheckMode mode = TypeNameEqualityCheckMode.Full)
        {
            if (mode != TypeNameEqualityCheckMode.SkipShortName && 
                type.Name == name)
            {
                return TypeNameEqualityType.Name;
            }
            if (mode == TypeNameEqualityCheckMode.OnlyShortName)
            {
                return TypeNameEqualityType.None;
            }

            var declaringFullName = GetDeclaringFullName(type, out var declaringType);

            if (declaringType != type &&
                declaringFullName == name)
            {
                return TypeNameEqualityType.DeclaringTypes;
            }
            if (declaringType?.Namespace != null)
            {
                declaringFullName = $"{declaringType.Namespace}.{declaringFullName}";

                if (declaringFullName == name)
                {
                    return TypeNameEqualityType.Namespace;
                }
            }

            return TypeNameEqualityType.None;
        }

        #endregion
    }
}
