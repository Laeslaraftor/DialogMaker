using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpPropertyBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken), IDSharpPropertyInfo
    {
        public override DSharpTypeBuilder DeclaringType { get; } = declaringType;
        public DSharpTypeToken? PropertyType
        {
            get
            {
                if (field == null && OriginalProperty != null)
                {
                    field = GetReplacedType(OriginalProperty.PropertyType);
                }

                return field;
            }
            set;
        }
        public DSharpMethodBuilder? Getter
        {
            get
            {
                if (field == null && !_triedToFindGetter)
                {
                    _triedToFindGetter = true;
                    field = DeclaringType.Methods.FirstOrDefault(m => m.Name == GetterMethodName);
                }

                return field;
            }
            private set;
        }
        public DSharpMethodBuilder? Setter
        {
            get
            {
                if (field == null && !_triedToFindSetter)
                {
                    _triedToFindSetter = true;
                    field = DeclaringType.Methods.FirstOrDefault(m => m.Name == SetterMethodName);
                }

                return field;
            }
            private set;
        }
        public IDSharpPropertyInfo? OverrideProperty
        {
            get;
            set
            {
                if (field != value)
                {
                    if (value != null)
                    {
                        if (value.IsSealed)
                        {
                            throw new ArgumentException($"Unable to override sealed property \"{value}\" by \"{this}\"", nameof(value));
                        }
                        if (!value.IsVirtual && !value.IsAbstract)
                        {
                            throw new ArgumentException($"Unable to override property that not virtual or abstract \"{value}\" by \"{this}\"", nameof(value));
                        }
                        if (value.PropertyType != PropertyType)
                        {
                            throw new ArgumentException($"Unable to override property with different type \"{value}\" by \"{this}\"");
                        }
                    }

                    field = value;
                }
            }
        }
        public string GetterMethodName { get; private set; } = string.Empty;
        public string SetterMethodName { get; private set; } = string.Empty;
        public bool CanRead
        {
            get
            {
                if (DeclaringType.ObjectType != DSharpObjectType.Interface)
                {
                    return Getter != null;
                }

                return field;
            }
            set;
        }
        public bool CanWrite
        {
            get
            {
                if (DeclaringType.ObjectType != DSharpObjectType.Interface)
                {
                    return Setter != null;
                }

                return field;
            }
            set;
        }
        public override bool IsVirtual 
        { 
            get => base.IsVirtual || OriginalProperty != null; 
            set => base.IsVirtual = value; 
        }
        public override bool IsDeclaration
        {
            get
            {
                var getter = Getter;
                var setter = Setter;

                if (getter == null && setter == null)
                {
                    return true;
                }

                return getter?.IsDeclaration == true ||
                       setter?.IsDeclaration == true;
            }
        }
        internal IDSharpPropertyInfo? OriginalProperty { get; set; }

        IDSharpType IDSharpPropertyInfo.PropertyType
        {
            get
            {
                if (PropertyType == null)
                {
                    throw new InvalidOperationException($"Can not get property type because it was not specified: {this}");
                }

                return (IDSharpType)Assembly.GetType(PropertyType);
            }
        }
        IDSharpMethodInfo? IDSharpPropertyInfo.Getter => Getter;
        IDSharpMethodInfo? IDSharpPropertyInfo.Setter => Setter;

        private bool _triedToFindGetter;
        private bool _triedToFindSetter;

        #region Управление

        internal override void Update()
        {
            base.Update();
            _ = PropertyType;
        }

        public DSharpMethodBuilder CreateGetter()
        {
            if (Getter != null)
            {
                return Getter;
            }

            var getter = DeclaringType.CreateMethod(t => new(this, false, GetterMethodName, t));
            Getter = getter;

            return getter;
        }
        public DSharpMethodBuilder CreateSetter()
        {
            if (Setter != null)
            {
                return Setter;
            }

            var setter = DeclaringType.CreateMethod(t => new(this, true, SetterMethodName, t));
            setter.Parameters.Add(new(Assembly)
            {
                TypeGetter = () => PropertyType,
                Name = "value"
            });
            Setter = setter;

            return setter;
        }

        public bool RemoveGetter()
        {
            if (DeclaringType is not DSharpTypeBuilder builder)
            {
                return false;
            }
            if (Getter != null && builder.RemoveMethod(Getter))
            {
                Getter = null;
                return true;
            }

            return false;
        }
        public bool RemoveSetter()
        {
            if (DeclaringType is not DSharpTypeBuilder builder)
            {
                return false;
            }
            if (Setter != null && builder.RemoveMethod(Setter))
            {
                Setter = null;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{DeclaringType.FullName}.{Name}";
        }

        #endregion

        #region События

        protected override void OnNameChanged(string name)
        {
            base.OnNameChanged(name);
            GetterMethodName = $"get_{name}";
            SetterMethodName = $"set_{name}";
        }

        #endregion
    }
}
