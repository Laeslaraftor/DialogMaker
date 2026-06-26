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
        : DSharpMemberInfoBuilder(assembly, name, metadataToken), IDSharpFieldInfo
    {
        /// <summary>
        /// Namespace that contains this field. 
        /// This property should be null when field is child of some type
        /// </summary>
        public virtual string? Namespace { get; set; }
        /// <summary>
        /// Type that declared this field. Empty field means this field is global variable in assembly
        /// </summary>
        public override DSharpTypeBuilder? DeclaringType { get; } = declaringType;
        /// <summary>
        /// Type of value that stored by this field
        /// </summary>
        public DSharpTypeToken? FieldType
        {
            get
            {
                if (field == null && FieldTypeResolver != null)
                {
                    field = FieldTypeResolver();
                    FieldTypeResolver = null;
                }
                if (field == null && DeclaringType != null && OriginalField != null)
                {
                    field = GetReplacedType(OriginalField.FieldType);
                }

                return field;
            }
            set;
        }
        /// <summary>
        /// Is read only flag. This flag means that this field can not be changed at runtime outside of constructor 
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// Default values that sets at compile time. Other values must be setted in constructor
        /// </summary>
        public DSharpLiteralValue? RawValue { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool IsDeclaration => false;
        internal IDSharpFieldInfo? OriginalField { get; set; }
        internal Func<DSharpTypeToken>? FieldTypeResolver { get; set; }

        IDSharpType IDSharpFieldInfo.FieldType
        {
            get
            {
                if (FieldType == null)
                {
                    throw new InvalidOperationException("Can not get field type while it was not specified");
                }

                return (IDSharpType)Assembly.GetType(FieldType);
            }
        }

        #region Управление

        internal override void Update()
        {
            base.Update();
            _ = FieldType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (DeclaringType == null)
            {
                return Name;
            }

            return $"{DeclaringType.FullName}.{Name}";
        }

        #endregion
    }
}
