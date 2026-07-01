namespace DialogMaker.Core.Scripting.Runtime
{
    /// <summary>
    /// Type of type metadata token
    /// </summary>
    public enum DSharpMetadataTokenType
    {
        /// <summary>
        /// Module metadata
        /// </summary>
        Module = 0x00000000,
        /// <summary>
        /// Reference type metadata
        /// </summary>
        TypeReference = 0x01000000,
        /// <summary>
        /// Type definition metadata
        /// </summary>
        TypeDefinition = 0x02000000,
        /// <summary>
        /// Field metadata 
        /// </summary>
        Field = 0x04000000,
        /// <summary>
        /// Method metadata
        /// </summary>
        Method = 0x06000000,
        /// <summary>
        /// Property metadata
        /// </summary>
        Property = 0x17000000,
        /// <summary>
        /// Parameter metadata
        /// </summary>
        Parameter = 0x08000000,
        /// <summary>
        /// Generic parameter metadata
        /// </summary>
        Operator = 0x2A000000,
    }
}
