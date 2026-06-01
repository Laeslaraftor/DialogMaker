namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpTypeBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken) 
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken)
    {
        /// <summary>
        /// Type that declared this field
        /// </summary>
        public override DSharpTypeBuilder? DeclaringType { get; } = declaringType;
        public DSharpObjectType Type { get; set; }
        public string? Namespace { get; set; }
        public string FullName
        {
            get
            {
                string result = Name;

                if (GenericParameters.Count > 0)
                {
                    result += "`" + GenericParameters.Count;
                }
                if (DeclaringType != null)
                {
                    result = $"{DeclaringType.FullName}.{result}";
                }
                else if (Namespace != null)
                {
                    result = $"{Namespace}.{result}";
                }

                return result;
            }
        }
        public List<DSharpTypeToken> BaseTypes { get; } = [];
        public List<DSharpTypeToken> GenericParameters { get; } = [];
        public List<DSharpTypeToken> GenericTypes { get; } = [];
        public ReferenceReadOnlyList<DSharpMethodBuilder> Constructors
        {
            get
            {
                field ??= new(_constructors);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpMethodBuilder> Methods
        {
            get
            {
                field ??= new(_methods);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpPropertyBuilder> Properties
        {
            get
            {
                field ??= new(_properties);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpFieldBuilder> Fields
        {
            get
            {
                field ??= new(_fields);
                return field;
            }
        }

        private readonly List<DSharpMethodBuilder> _constructors = [];
        private readonly List<DSharpMethodBuilder> _methods = [];
        private readonly List<DSharpPropertyBuilder> _properties = [];
        private readonly List<DSharpFieldBuilder> _fields = [];

        #region Управление

        internal DSharpMethodBuilder CreateMethod(Func<DSharpTypeToken, DSharpMethodBuilder> fabric)
        {
            return CreateMember(DSharpMetadataTokenType.Method, _methods, fabric);
        }

        public DSharpMethodBuilder CreateConstructor()
        {
            return CreateMember(DSharpMetadataTokenType.Method, _constructors, t => new(Assembly, this, ConstructorName, t));
        }
        public DSharpMethodBuilder CreateMethod(string name)
        {
            return CreateMethod(t => new(Assembly, this, name, t));
        }
        public DSharpPropertyBuilder CreateProperty(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Property, _properties, t => new(Assembly, this, name, t));
        }
        public DSharpFieldBuilder CreateField(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Field, _fields, t => new(Assembly, this, name, t));
        }

        public bool RemoveConstructor(DSharpMethodBuilder constructor)
        {
            return RemoveMember(_constructors, constructor);
        }
        public bool RemoveMethod(DSharpMethodBuilder method)
        {
            return RemoveMember(_methods, method);
        }
        public bool RemoveProperty(DSharpPropertyBuilder property)
        {
            return RemoveMember(_properties, property);
        }
        public bool RemoveField(DSharpFieldBuilder field)
        {
            return RemoveMember(_fields, field);
        }

        #endregion

        #region Константы

        public const string ConstructorName = ".ctor";

        #endregion
    }
}
