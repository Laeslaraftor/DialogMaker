using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime
{
    public class DSharpAssembly
    {
        /// <summary>
        /// List of names that requires for this assembly
        /// </summary>
        public ReadOnlyCollection<string> RequiredAssembliesReferenceName { get; }
        public string Name { get; }
        public ReadOnlyCollection<DSharpType> Types { get; }

        private readonly Dictionary<string, DSharpAssembly> _references = [];

        #region Управление

        public DSharpMemberInfo GetMember(DSharpMetadataToken metadataToken)
        {
            DSharpAssembly assembly = this;

            if (metadataToken.AssemblyIndex > 0)
            {
                var referencesAssemblyName = RequiredAssembliesReferenceName[metadataToken.AssemblyIndex - 1];

                if (!_references.TryGetValue(referencesAssemblyName, out assembly))
                {
                    throw new InvalidOperationException($"Unable to load reference {referencesAssemblyName}, please load all references before start executing something");
                }
            }

            return GetMember(assembly, metadataToken);
        }
        public DSharpType GetType(string name)
        {
            return Types.First(t => t.FullName == name);
        }
        public DSharpPropertyInfo GetProperty(string name)
        {
            if (!TryFindMember(t => t.Properties, name, out var result))
            {
                throw new ArgumentException($"Unknown property: {name}", nameof(name));
            }

            return result;
        }
        public DSharpFieldInfo GetField(string name)
        {
            if (!TryFindMember(t => t.Fields, name, out var result))
            {
                throw new ArgumentException($"Unknown field: {name}", nameof(name));
            }

            return result;
        }
        public DSharpMethodInfo GetMethod(string name)
        {
            if (!TryFindMember(t => t.Methods, name, out var result))
            {
                throw new ArgumentException($"Unknown method: {name}", nameof(name));
            }

            return result;
        }
        public bool TryGetType(string name, [NotNullWhen(true)] out DSharpType? result)
        {
            result = Types.FirstOrDefault(t => t.FullName == name);
            return result != null;
        }
        public bool TryGetProperty(string name, [NotNullWhen(true)] out DSharpPropertyInfo? result)
        {
            return TryFindMember(t => t.Properties, name, out result);
        }
        public bool TryGetField(string name, [NotNullWhen(true)] out DSharpFieldInfo? result)
        {
            return TryFindMember(t => t.Fields, name, out result);
        }
        public bool TryGetMethod(string name, [NotNullWhen(true)] out DSharpMethodInfo? result)
        {
            return TryFindMember(t => t.Methods, name, out result);
        }

        private bool TryFindMember<T>(Func<DSharpType, IEnumerable<T>> selector, string name, [NotNullWhen(true)] out T? result)
            where T : DSharpMemberInfo
        {
            result = default;

            foreach (var type in Types)
            {
                result = selector(type).FirstOrDefault(p => p.FullName == name);

                if (result != null)
                {
                    return true;
                }
            }

            return false;
        }
        private DSharpMemberInfo GetMember(DSharpAssembly assembly, DSharpMetadataToken metadataToken)
        {
            DSharpMemberInfo Find(Func<DSharpType, IEnumerable<DSharpMemberInfo>> selector)
            {
                foreach (var type in assembly.Types)
                {
                    foreach (var member in selector(type))
                    {
                        if (member.MetadataToken == metadataToken)
                        {
                            return member;
                        }
                    }
                }

                throw new ArgumentException($"Unknown member with metadata token: {metadataToken}", nameof(metadataToken));
            }

            if (metadataToken.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                return assembly.Types.First(t => t.MetadataToken == metadataToken);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Property)
            {
                return Find(t => t.Properties);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Field)
            {
                return Find(t => t.Fields);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Method)
            {
                return Find(t => t.Methods);
            }

            throw new ArgumentException($"Invalid token type: {metadataToken.Type}", nameof(metadataToken));
        }

        #endregion
    }
}
