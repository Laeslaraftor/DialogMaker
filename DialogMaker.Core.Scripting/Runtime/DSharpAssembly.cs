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
        public ReadOnlyCollection<DSharpFieldInfo> GlobalVariables { get; }
        public ReadOnlyCollection<DSharpMethodInfo> GlobalFunctions { get; }

        private readonly Dictionary<string, DSharpAssembly> _references = [];

        #region Управление

        public DSharpMemberInfo GetMember(DSharpMetadataToken metadataToken)
        {
            DSharpAssembly assembly = this;

            if (metadataToken.AssemblyIndex > 0)
            {
                var referencesAssemblyName = RequiredAssembliesReferenceName[metadataToken.AssemblyIndex - 1];
                metadataToken = new(metadataToken, 0);

                if (!_references.TryGetValue(referencesAssemblyName, out assembly))
                {
                    throw new InvalidOperationException($"Unable to load reference {referencesAssemblyName}, please load all references before start executing something");
                }
            }

            return GetMember(assembly, metadataToken);
        }
        public DSharpType GetType(string fullName)
        {
            return Types.First(t => t.FullName == fullName);
        }
        public DSharpFieldInfo GetGlobalVariable(string name)
        {
            if (!TryGetGlobalVariable(name, out var result))
            {
                throw new ArgumentException($"Unknown global variable: {name}", nameof(name));
            }

            return result;
        }
        public DSharpMethodInfo GetGlobalFunction(string name)
        {
            if (!TryGetGlobalFunction(name, out var result))
            {
                throw new ArgumentException($"Unknown global function: {name}", nameof(name));
            }

            return result;
        }
        public bool TryGetType(string fullName, [NotNullWhen(true)] out DSharpType? result)
        {
            result = Types.FirstOrDefault(t => t.FullName == fullName);
            return result != null;
        }
        public bool TryGetGlobalVariable(string name, [NotNullWhen(true)] out DSharpFieldInfo? result)
        {
            result = GlobalVariables.FirstOrDefault(v => v.Name == name);
            return result != null;
        }
        public bool TryGetGlobalFunction(string name, [NotNullWhen(true)] out DSharpMethodInfo? result)
        {
            result = GlobalFunctions.FirstOrDefault(f => f.Name == name);
            return result != null;
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

                throw new ArgumentException($"Unknown member with token: {metadataToken}", nameof(metadataToken));
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
                var result = assembly.GlobalVariables.FirstOrDefault(v => v.MetadataToken == metadataToken);

                if (result != null)
                {
                    return result;
                }

                return Find(t => t.Fields);
            }
            else if (metadataToken.Type == DSharpMetadataTokenType.Method)
            {
                var result = assembly.GlobalFunctions.FirstOrDefault(f => f.MetadataToken == metadataToken);

                if (result != null)
                {
                    return result;
                }

                return Find(t => t.Methods);
            }

            throw new ArgumentException($"Invalid token type: {metadataToken.Type}", nameof(metadataToken));
        }

        #endregion
    }
}
