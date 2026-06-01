namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpMethodBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken)
    {
        public DSharpMethodBuilder(DSharpPropertyBuilder property, bool isSetter, string name, DSharpTypeToken metadataToken)
            : this(property.Assembly, property.DeclaringType, name, metadataToken)
        {
            LinkedProperty = property;
            MethodType = isSetter ? DSharpMethodType.Setter : DSharpMethodType.Getter;
        }
        public DSharpMethodBuilder(DSharpTypeBuilder type, string name, DSharpTypeToken metadataToken)
            : this(type.Assembly, type, name, metadataToken)
        {
            LinkedType = type;
            MethodType = DSharpMethodType.Constructor;
        }

        public DSharpMethodType MethodType { get; } = DSharpMethodType.Default;
        /// <summary>
        /// This method is getter or setter only if this property not null 
        /// </summary>
        public DSharpPropertyBuilder? LinkedProperty { get; }
        /// <summary>
        /// This method is constructor only if this property not null 
        /// </summary>
        public DSharpTypeBuilder? LinkedType { get; }
        public override string Name
        {
            get
            {
                if (LinkedType != null)
                {
                    return DSharpTypeBuilder.ConstructorName;
                }
                else if (LinkedProperty != null)
                {
                    if (MethodType == DSharpMethodType.Getter)
                    {
                        return LinkedProperty.GetterMethodName;
                    }

                    return LinkedProperty.SetterMethodName;
                }

                return base.Name;
            }
            set => base.Name = value;
        }
        public override DSharpTypeBuilder? DeclaringType { get; } = declaringType;
        public DSharpTypeBuilder? ReturnType
        {
            get
            {
                if (LinkedType != null)
                {
                    return null;
                }
                if (LinkedProperty != null)
                {
                    if (MethodType == DSharpMethodType.Setter)
                    {
                        return null;
                    }

                    return LinkedProperty.PropertyType;
                }

                return field;
            }
            set;
        }
        public List<DSharpTypeBuilder> Parameters { get; } = [];
        public override bool IsStatic
        {
            get => LinkedProperty?.IsStatic ?? base.IsStatic;
            set => base.IsStatic = value;
        }
        public override bool IsAbstract
        {
            get
            {
                if (LinkedType != null)
                {
                    return false;
                }

                return LinkedProperty?.IsAbstract ?? base.IsAbstract;
            }
            set => base.IsAbstract = value;
        }
        public override bool IsSealed
        {
            get
            {
                if (LinkedType != null)
                {
                    return false;
                }

                return LinkedProperty?.IsSealed ?? base.IsSealed;
            }
            set => base.IsSealed = value;
        }
        public bool IsVirtual
        {
            get
            {
                if (LinkedType != null)
                {
                    return false;
                }
                if (LinkedProperty != null)
                {
                    return LinkedProperty.IsVirtual;
                }

                return field;
            }
            set;
        }
        public bool IsExtern
        {
            get
            {
                if (LinkedProperty != null && LinkedType != null)
                {
                    return false;
                }

                return field;
            }
            set;
        }
    }
}
