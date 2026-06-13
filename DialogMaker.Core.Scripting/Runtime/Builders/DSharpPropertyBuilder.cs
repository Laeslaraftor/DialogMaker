using Newtonsoft.Json.Linq;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpPropertyBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken), IDSharpPropertyInfo
    {
        public override IDSharpType DeclaringType { get; } = declaringType;
        public DSharpTypeToken? PropertyType { get; set; }
        public DSharpMethodBuilder? Getter { get; private set; }
        public DSharpMethodBuilder? Setter { get; private set; }
        public string GetterMethodName { get; private set; } = string.Empty;
        public string SetterMethodName { get; private set; } = string.Empty;
        public bool IsVirtual { get; set; }
        public bool CanRead => Getter != null;
        public bool CanWrite => Setter != null;

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

        #region Управление

        public DSharpMethodBuilder CreateGetter()
        {
            if (Getter != null)
            {
                return Getter;
            }
            if (DeclaringType is not DSharpTypeBuilder builder)
            {
                throw new InvalidOperationException($"Unable to create getter because declaring type is not a type builder");
            }

            var getter = builder.CreateMethod(t => new(this, false, GetterMethodName, t));
            Getter = getter;

            return getter;
        }
        public DSharpMethodBuilder CreateSetter()
        {
            if (Setter != null)
            {
                return Setter;
            }
            if (DeclaringType is not DSharpTypeBuilder builder)
            {
                throw new InvalidOperationException($"Unable to create setter because declaring type is not a type builder");
            }

            var setter = builder.CreateMethod(t => new(this, true, SetterMethodName, t));
            setter.Parameters.Add(new(Assembly)
            {
                Type = PropertyType,
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
