namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpMethodBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken), IDSharpMethodInfo
    {
        public DSharpMethodBuilder(DSharpPropertyBuilder property, bool isSetter, string name, DSharpTypeToken metadataToken)
            : this(property.Assembly, property.DeclaringType, name, metadataToken)
        {
            LinkedProperty = property;
            MethodType = isSetter ? DSharpMethodType.Setter : DSharpMethodType.Getter;
        }
        public DSharpMethodBuilder(DSharpTypeBuilder type, DSharpTypeToken metadataToken)
            : this(type.Assembly, type, DSharpTypeBuilder.ConstructorName, metadataToken)
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
        /// <summary>
        /// Namespace that contains this function. 
        /// This property should be null when method is child of some type
        /// </summary>
        public virtual string? Namespace { get; set; }
        public override DSharpTypeBuilder? DeclaringType { get; } = declaringType;
        public DSharpTypeToken? ReturnType
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
        public List<DSharpMethodBuilderParameter> Parameters { get; } = [];
        public List<DSharpTypeToken> GenericParameters { get; } = [];
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
        public override bool IsOverride
        {
            get
            {
                if (LinkedType != null)
                {
                    return false;
                }

                return LinkedProperty?.IsOverride ?? base.IsOverride;
            }
            set => base.IsOverride = value;
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

        IDSharpType? IDSharpMethodInfo.ReturnType
        {
            get
            {
                if (ReturnType == null)
                {
                    return null;
                }

                return (IDSharpType)Assembly.GetType(ReturnType);
            }
        }


        private DSharpBytecodeBuilder? _bytecodeBuilder;

        #region Управление

        public DSharpBytecodeBuilder GetBytecodeBuilder()
        {
            if (IsAbstract)
            {
                throw new InvalidOperationException($"Abstract method can not contains bytecode: {this}");
            }
            if (IsExtern)
            {
                throw new InvalidOperationException($"Extern method can not contains bytecode: {this}");
            }

            _bytecodeBuilder ??= new(this);
            return _bytecodeBuilder;
        }

        public IDSharpParameterInfo[] GetParameters() => [.. Parameters];

        public override string ToString()
        {
            string name = Name + '(';
            int parameterIndex = 0;

            foreach (var parameter in Parameters)
            {
                if (parameterIndex > 0)
                {
                    name += ", ";
                }

                if (parameter.Type != null)
                {
                    var type = Assembly.GetType(parameter.Type);
                    name += $"{type} ";
                }

                name += $"{parameter.Name}";

                parameterIndex++;
            }

            name += ')';

            if (DeclaringType == null)
            {
                return name;
            }

            return $"{DeclaringType.FullName}.{name}";
        }

        #endregion
    }
}
