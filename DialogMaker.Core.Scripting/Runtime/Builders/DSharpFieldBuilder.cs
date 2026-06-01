namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    /// <summary>
    /// Field member builder
    /// </summary>
    /// <param name="assembly"><inheritdoc/></param>
    /// <param name="declaringType">Type that declared this field</param>
    /// <param name="name"><inheritdoc/></param>
    /// <param name="metadataToken"><inheritdoc/></param>
    public class DSharpFieldBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpMemberInfoBuilder(assembly, name, metadataToken)
    {
        /// <summary>
        /// Type that declared this field. Empty field means this field is global variable in assembly
        /// </summary>
        public override DSharpTypeBuilder? DeclaringType => declaringType;
        /// <summary>
        /// Type of value that stored by this field
        /// </summary>
        public DSharpTypeToken? FieldType { get; set; }
        /// <summary>
        /// Is read only flag. This flag means that this field can not be changed at runtime outside of constructor 
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// Default values that sets at compile time. Other values must be setted in constructor
        /// </summary>
        public DSharpLiteralValue RawValue { get; set; }
    }
}
