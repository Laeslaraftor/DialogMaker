using DialogMaker.Core.Scripting.Compiler.Ast;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    /// <summary>
    /// Base class of member builder
    /// </summary>
    public abstract class DSharpMemberInfoBuilder : IDSharpMemberInfo
    {
        /// <param name="assembly">Assembly that contains this member</param>
        /// <param name="name">Name of member</param>
        /// <param name="metadataToken">Metadata token for identifying member in bytecode</param>
        public DSharpMemberInfoBuilder(DSharpAssemblyBuilder assembly, string name, DSharpTypeToken metadataToken)
        {
            Assembly = assembly;
            Name = name;
            MetadataToken = metadataToken;
        }

        /// <summary>
        /// Assembly that contains this member
        /// </summary>
        public DSharpAssemblyBuilder Assembly { get; }
        /// <summary>
        /// Name of this member
        /// </summary>
        public virtual string Name
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnNameChanged(value);
                }
            }
        }
        /// <summary>
        /// Metadata token for identifying member in bytecode
        /// </summary>
        public DSharpTypeToken MetadataToken { get; }
        /// <summary>
        /// Type that declared this member
        /// </summary>
        public abstract DSharpTypeBuilder? DeclaringType { get; }
        /// <summary>
        /// List of member attributes
        /// </summary>
        public List<DSharpAttributeDataBuilder> Attributes { get; } = [];
        /// <summary>
        /// Static flag. This flag means that member can be accessed without object instance
        /// </summary>
        public virtual bool IsStatic { get; set; }
        /// <summary>
        /// Access modifier of this member
        /// </summary>
        public DSharpAccessModifier Access { get; set; } = DSharpAccessModifier.Private;

        DSharpMetadataToken IDSharpMemberInfo.MetadataToken => MetadataToken;
        IDSharpType? IDSharpMemberInfo.DeclaringType => DeclaringType;
        IDSharpAssembly IDSharpMemberInfo.Assembly => Assembly;

        #region Управление

        internal virtual void Update()
        {
        }

        #endregion

        #region Дополнительно

        protected T CreateMember<T>(DSharpMetadataTokenType tokenType, Func<DSharpTypeToken, T> fabric)
        {
            var token = Assembly.AllocateMetadataToken(tokenType);
            var member = fabric(token);
            return member;
        }
        protected T CreateMember<T>(DSharpMetadataTokenType tokenType, IList<T> members, Func<DSharpTypeToken, T> fabric)
        {
            var member = CreateMember(tokenType, fabric);
            members.Add(member);

            return member;
        }
        protected bool RemoveMember<T>(T member)
            where T : DSharpMemberInfoBuilder
        {
            return Assembly.RemoveMember(member);
        }
        protected bool RemoveMember<T>(IList<T> members, T member)
            where T : DSharpMemberInfoBuilder
        {
            if (members.Remove(member))
            {
                return RemoveMember(member);
            }

            return false;
        }

        protected DSharpTypeToken GetReplacedType(IDSharpType originalType)
        {
            if (DeclaringType == null)
            {
                throw new InvalidOperationException($"Unable to get replaced type for member without declaring type: \"{this}\"");
            }

            var replacedTypes = DeclaringType.GetReplacedTypes();
            return Assembly.GetTypeToken(Assembly.ReplaceGenericParameters(originalType, replacedTypes));
        }

        #endregion

        #region События

        protected virtual void OnNameChanged(string name)
        {
        }

        #endregion
    }
}
