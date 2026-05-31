using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpAssemblyBuilder(string name, IList<DSharpAssembly> references)
    {
        public string Name { get; } = name;
        public ReadOnlyCollection<DSharpAssembly> References { get; } = new(references);
        public ReferenceReadOnlyList<DSharpTypeBuilder> Types
        {
            get
            {
                field ??= new(_types);
                return field;
            }
        }

        private readonly ObservableCollection<DSharpTypeBuilder> _types = [];
        private readonly List<DSharpMetadataToken> _typeDefinitions = [];
        private readonly List<DSharpMetadataToken> _methodsDefinitions = [];
        private readonly List<DSharpMetadataToken> _fieldsDefinitions = [];
        private readonly List<DSharpMetadataToken> _propertiesDefinitions = [];

        #region Управление

        public DSharpMetadataToken AllocateMetadataToken(DSharpMetadataTokenType type, string name)
        {
            List<DSharpMetadataToken> tokensBuffer;

            if (type == DSharpMetadataTokenType.TypeDefinition)
            {
                if (TryGetReference<DSharpType>(t => null, name, out var referenceToken))
                {
                    return referenceToken;
                }

                tokensBuffer = _typeDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Property)
            {
                if (TryGetReference(t => t.Properties, name, out var referenceToken))
                {
                    return referenceToken;
                }

                tokensBuffer = _propertiesDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Field)
            {
                if (TryGetReference(t => t.Fields, name, out var referenceToken))
                {
                    return referenceToken;
                }

                tokensBuffer = _fieldsDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Method)
            {
                if (TryGetReference(t => t.Methods, name, out var referenceToken))
                {
                    return referenceToken;
                }

                tokensBuffer = _methodsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Invalid token type: {type}");
            }

            DSharpMetadataToken token = new(type, tokensBuffer.Count, 0);
            tokensBuffer.Add(token);

            return token;
        }
        public bool RemoveDefinition(DSharpMetadataToken metadataToken)
        {
            // Надо сделать удаление токена со смещением остальных
            return false;
        }
        public DSharpTypeBuilder CreateType(DSharpMetadataToken token, string name)
        {
            CheckToken(token);

            DSharpTypeBuilder type = new(this, name, token);
            _types.Add(type);

            return type;
        }

        private void CheckToken(DSharpMetadataToken token)
        {
            if (token.AssemblyIndex != 0)
            {
                throw new ArgumentException($"Unable to use token that references to member from other assembly");
            }
        }
        private bool TryGetReference<T>(Func<DSharpType, IEnumerable<T>?> selector, string name, [NotNullWhen(true)] out DSharpMetadataToken referenceToken)
            where T : DSharpMemberInfo
        {
            referenceToken = default;
            int referenceIndex = 0;

            foreach (var reference in References)
            {
                foreach (var type in reference.Types)
                {
                    var members = selector(type);

                    if (members == null)
                    {
                        if (type.FullName == name)
                        {
                            referenceToken = new(type.MetadataToken, referenceIndex);
                            return true;
                        }
                    }
                    else
                    {
                        foreach (var member in members)
                        {
                            if (member.FullName == name)
                            {
                                referenceToken = new(member.MetadataToken, referenceIndex);
                                return true;
                            }
                        }
                    }
                }

                referenceIndex++;
            }

            return false;
        }

        #endregion
    }
}
