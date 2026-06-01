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
        public ReferenceReadOnlyList<DSharpFieldBuilder> GlobalVariables
        {
            get
            {
                field ??= new(_globalVariables);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpMethodBuilder> GlobalFunctions
        {
            get
            {
                field ??= new(_globalFunctions);
                return field;
            }
        }
        public DSharpTypeToken StringType
        {
            get
            {
                field ??= GetTypeToken(StringTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken NumberType
        {
            get
            {
                field ??= GetTypeToken(NumberTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken CharType
        {
            get
            {
                field ??= GetTypeToken(CharTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken BoolType
        {
            get
            {
                field ??= GetTypeToken(BoolTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken EnumType
        {
            get
            {
                field ??= GetTypeToken(EnumTypeFullName);
                return field;
            }
        }

        private readonly ObservableCollection<DSharpTypeBuilder> _types = [];
        private readonly ObservableCollection<DSharpFieldBuilder> _globalVariables = [];
        private readonly ObservableCollection<DSharpMethodBuilder> _globalFunctions = [];
        private readonly List<DSharpTypeToken> _typeDefinitions = [];
        private readonly List<DSharpTypeToken> _methodsDefinitions = [];
        private readonly List<DSharpTypeToken> _fieldsDefinitions = [];
        private readonly List<DSharpTypeToken> _propertiesDefinitions = [];

        #region Управление

        internal DSharpTypeToken AllocateMetadataToken(DSharpMetadataTokenType type)
        {
            List<DSharpTypeToken> tokensBuffer;

            if (type == DSharpMetadataTokenType.TypeDefinition)
            {
                tokensBuffer = _typeDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Property)
            {
                tokensBuffer = _propertiesDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Field)
            {
                tokensBuffer = _fieldsDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Method)
            {
                tokensBuffer = _methodsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Invalid token type: {type}");
            }

            DSharpTypeToken token = new(type, tokensBuffer.Count, 0);
            tokensBuffer.Add(token);

            return token;
        }
        internal bool RemoveMember(DSharpMemberInfoBuilder member)
        {
            List<DSharpTypeToken> tokens;

            if (member.MetadataToken.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                tokens = _typeDefinitions;
            } 
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Method)
            {
                tokens = _methodsDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Property)
            {
                tokens = _propertiesDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Field)
            {
                tokens = _fieldsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Member with invalid metadata token type: {member.MetadataToken}");
            }

            bool result = RemoveMember(tokens, member.MetadataToken);

            if (result && member is DSharpTypeBuilder typeBuilder)
            {
                _types.Remove(typeBuilder);

                foreach (var type in _types)
                {
                    type.BaseTypes.Remove(typeBuilder.MetadataToken);
                }
            }

            return result;
        }

        public bool RemoveGlobalVariable(DSharpFieldBuilder variable)
        {
            if (_globalVariables.Remove(variable))
            {
                return RemoveMember(_fieldsDefinitions, variable.MetadataToken);
            }

            return false;
        }
        public bool RemoveGlobalFunction(DSharpMethodBuilder function)
        {
            if (_globalFunctions.Remove(function))
            {
                return RemoveMember(_methodsDefinitions, function.MetadataToken);
            }

            return false;
        }
        public bool RemoveType(DSharpTypeBuilder type) => RemoveMember(type);
        public DSharpTypeBuilder CreateType(string name, DSharpTypeBuilder? parent = null)
        {
            return CreateMember(DSharpMetadataTokenType.TypeDefinition, _types, t => new(this, parent, name, t));
        }
        public DSharpFieldBuilder CreateGlobalVariable(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Field, _globalVariables, t => new(this, null, name, t));
        }
        public DSharpMethodBuilder CreateGlobalFunction(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Method, _globalFunctions, t => new(this, null, name, t));
        }

        private T CreateMember<T>(DSharpMetadataTokenType tokenType, IList<T> members, Func<DSharpTypeToken, T> fabric)
        {
            var token = AllocateMetadataToken(tokenType);
            var type = fabric(token);
            members.Add(type);

            return type;
        }
        private bool RemoveMember(List<DSharpTypeToken> membersToken, DSharpTypeToken token)
        {
            var index = membersToken.IndexOf(token);

            if (index == -1)
            {
                return false;
            }

            membersToken.RemoveAt(index);
            token.Index = -1;

            for (int i = index; i < membersToken.Count; i++)
            {
                membersToken[i].Index--;
            }

            return true;
        }

        #endregion

        #region Поиск

        public DSharpTypeToken GetTypeToken(string fullName)
        {
            if (TryGetTypeToken(fullName, out var token))
            {
                return token;
            }

            throw new ArgumentException($"Unknown type: {fullName}", nameof(fullName));
        }
        public bool TryGetTypeToken(string fullName, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = _types.FirstOrDefault(t => t.FullName == fullName);

            if (result != null)
            {
                return true;
            }

            int referenceIndex = 0;

            foreach (var reference in References)
            {
                if (reference.TryGetType(fullName, out var referencedType))
                {
                    result = new(referencedType.MetadataToken, referenceIndex);
                    return true;
                }

                referenceIndex++;
            }

            return false;
        }
        public DSharpTypeBuilder GetType(string fullName)
        {
            var type = _types.FirstOrDefault(t => t.FullName == fullName);

            return type ?? throw new ArgumentException($"Unknown type: {fullName}", nameof(fullName));
        }

        #endregion

        #region Константы

        public const string StringTypeFullName = "System.String";
        public const string NumberTypeFullName = "System.Number";
        public const string BoolTypeFullName = "System.Bool";
        public const string CharTypeFullName = "System.Char";
        public const string EnumTypeFullName = "System.Enum";

        #endregion
    }
}
