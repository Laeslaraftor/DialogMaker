namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    /// <summary>
    /// Type token with dynamic index. This type used by builders
    /// </summary>
    /// <param name="type"><inheritdoc cref="DSharpMetadataToken.Type"/></param>
    /// <param name="index"><inheritdoc cref="DSharpMetadataToken.Index"/></param>
    /// <param name="assemblyIndex"><inheritdoc cref="DSharpMetadataToken.AssemblyIndex"/></param>
    public class DSharpTypeToken(DSharpMetadataTokenType type, int index, int assemblyIndex)
    {
        /// <summary>
        /// Create new instance of type token
        /// </summary>
        /// <param name="metadataToken">Metadata token that will be used to create new type token</param>
        public DSharpTypeToken(DSharpMetadataToken metadataToken)
            : this(metadataToken.Type, metadataToken.Index, metadataToken.AssemblyIndex)
        {
        }
        /// <summary>
        /// Create new instance of type token
        /// </summary>
        /// <param name="metadataToken">Metadata token that will be used to create new type token</param>
        /// <param name="assemblyIndex"><inheritdoc cref="DSharpMetadataToken.AssemblyIndex"/></param>
        public DSharpTypeToken(DSharpMetadataToken metadataToken, int assemblyIndex)
            : this(metadataToken.Type, metadataToken.Index, assemblyIndex)
        {
        }

        /// <summary>
        /// <inheritdoc cref="DSharpMetadataToken.Type"/>
        /// </summary>
        public DSharpMetadataTokenType Type { get; } = type;
        /// <summary>
        /// <inheritdoc cref="DSharpMetadataToken.Index"/>
        /// </summary>
        public int Index { get; internal set; } = index;
        /// <summary>
        /// <inheritdoc cref="DSharpMetadataToken.AssemblyIndex"/>
        /// </summary>
        public int AssemblyIndex { get; } = assemblyIndex;

        #region Операторы

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="metadataToken"><inheritdoc/></param>
        public static implicit operator DSharpTypeToken(DSharpMetadataToken metadataToken) => new(metadataToken);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="memberBuilder"><inheritdoc/></param>
        public static implicit operator DSharpTypeToken(DSharpMemberInfoBuilder memberBuilder) => memberBuilder.MetadataToken;

        #endregion
    }
}
