using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Builders;
using DialogMaker.Core.Scripting.Runtime.Compilers;

namespace DialogMaker.Core.Scripting.Compiler.Scopes
{
    /// <summary>
    /// Type scope
    /// </summary>
    /// <param name="type">Type that contain current scope</param>
    /// <param name="parent">Parent scope</param>
    public class DSharpCompilerTypeScope(DSharpAssemblyBuilder assembly, IDSharpType type, DSharpCompilerScope? parent)
        : DSharpCompilerScope(assembly, parent)
    {
        /// <summary>
        /// Type that contain current scope
        /// </summary>
        public IDSharpType Type { get; } = type;

        protected override IEnumerable<IDSharpType> GetTypes(string name)
        {
            var typeFullName = DSharpCompilerFileScope.GetTypeFullName(Type);

            if (Type.Name == name ||
                typeFullName == name)
            {
                yield return Type;
            }

            foreach (var genericType in Type.GetGenericTypes().Where(t => t.Name == name))
            {
                yield return genericType;
            }

            IDSharpType? declaringType = Type.DeclaringType;
            IDSharpType? previousDeclaringType = Type;

            foreach (var child in GetBaseTypes(Type, name))
            {
                yield return child;
            }

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

                foreach (var childType in GetChildTypes(declaringType, name, true, TypeNameEqualityCheckMode.SkipShortName))
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
        protected override IDSharpParameterInfo? CreateLocalVariable(string name, IDSharpType type)
        {
            return null;
        }

        private IEnumerable<IDSharpType> GetBaseTypes(IDSharpType parent, string name, TypeNameEqualityCheckMode mode = TypeNameEqualityCheckMode.Full)
        {
            foreach (var baseType in parent.GetBaseTypes().Where(t => t.ObjectType == parent.ObjectType))
            {
                foreach (var baseTypeChild in GetChildTypes(baseType, name, false, mode))
                {
                    if (baseTypeChild.Access == DSharpAccessModifier.Private ||
                        (baseTypeChild.Access == DSharpAccessModifier.Internal && baseTypeChild.Assembly != Assembly))
                    {
                        continue;
                    }

                    yield return baseTypeChild;
                }

                foreach (var baseChilds in GetBaseTypes(baseType, name, mode))
                {
                    yield return baseChilds;
                }
            }
        }
        private IEnumerable<IDSharpType> GetChildTypes(IDSharpType parent, string name, bool skipPrivate = false, TypeNameEqualityCheckMode mode = TypeNameEqualityCheckMode.Full)
        {
            foreach (var child in parent.GetChildrenTypes())
            {
                if (skipPrivate &&
                    child.Access == DSharpAccessModifier.Private ||
                    child.Access == DSharpAccessModifier.Protected ||
                    (child.Access == DSharpAccessModifier.Internal && child.Assembly != Assembly))
                {
                    continue;
                }

                var equality = DSharpCompilerFileScope.IsNameEquals(child, name, mode);

                if (equality.IsNotEquals())
                {
                    continue;
                }

                yield return child;

                foreach (var childrenChild in GetChildTypes(child, name, skipPrivate, mode))
                {
                    yield return childrenChild;
                }
            }
        }
    }
}
