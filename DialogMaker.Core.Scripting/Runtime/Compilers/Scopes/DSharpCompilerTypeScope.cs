using DialogMaker.Core.Scripting.Runtime.Builders;

namespace DialogMaker.Core.Scripting.Runtime.Compilers.Scopes
{
    /// <summary>
    /// Type scope
    /// </summary>
    /// <param name="type">Type that contain current scope</param>
    /// <param name="parent">Parent scope</param>
    public class DSharpCompilerTypeScope(DSharpTypeBuilder type, DSharpCompilerScope? parent) 
        : DSharpCompilerScope(type.Assembly, parent)
    {
        /// <summary>
        /// Type that contain current scope
        /// </summary>
        public DSharpTypeBuilder Type { get; } = type;

        protected override IEnumerable<IDSharpType> GetTypes(string name)
        {
            var typeFullName = DSharpCompilerFileScope.GetTypeFullName(Type);

            if (Type.Name == name ||
                typeFullName == name)
            {
                yield return Type;
            }

            if (Type.GenericTypes.Count > 0)
            {
                foreach (var genericType in Type.GenericTypes.Where(t => t.Name == name))
                {
                    yield return genericType;
                }
            }

            IDSharpType? declaringType = Type.DeclaringType;
            IDSharpType? previousDeclaringType = Type;

            while (declaringType != null)
            {
                var nameEquality = DSharpCompilerFileScope.IsNameEquals(declaringType, name);

                if (declaringType.Name == name ||
                    nameEquality.IsEquals())
                {
                    yield return declaringType;
                }

                foreach (var genericType in declaringType.GetGenericTypes().Where(t => t.Name == name))
                {
                    yield return genericType;
                }

                foreach (var childType in GetChildTypes(declaringType, name, TypeNameEqualityCheckMode.SkipShortName))
                {
                    if (childType == previousDeclaringType)
                    {
                        continue;
                    }

                    yield return childType;
                }

                previousDeclaringType = declaringType;
                declaringType = declaringType.DeclaringType;
            }

            foreach (var child in GetChildTypes(Type, name))
            {
                yield return child;
            }
        }
        protected override IEnumerable<IDSharpMemberInfo> GetMembers()
        {
            foreach (var member in Type.GetAllMembers())
            {
                if (member.DeclaringType != Type &&
                    !member.IsStatic)
                {
                    continue;
                }

                yield return member;
            }
        }
        protected override IEnumerable<IDSharpParameterInfo> GetVariables()
        {
            yield break;
        }
        protected override IDSharpParameterInfo? CreateVariable(string name, IDSharpType type)
        {
            return null;
        }

        private static IEnumerable<IDSharpType> GetChildTypes(IDSharpType parent, string name, TypeNameEqualityCheckMode mode = TypeNameEqualityCheckMode.Full)
        {
            foreach (var child in parent.GetChildrenTypes())
            {
                var equality = DSharpCompilerFileScope.IsNameEquals(child, name, mode);

                if (equality.IsNotEquals())
                {
                    continue;
                }

                yield return child;

                foreach (var childrenChild in GetChildTypes(child, name, mode))
                {
                    yield return childrenChild;
                }
            }
        }
    }
}
