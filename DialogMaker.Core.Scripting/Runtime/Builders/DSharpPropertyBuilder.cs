namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpPropertyBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken)
    {
        public override string Name 
        { 
            get => base.Name; 
            set
            {
                base.Name = value;
                GetterMethodName = $"get_{value}";
                SetterMethodName = $"set_{value}";
            }
        }
        public override DSharpTypeBuilder DeclaringType { get; } = declaringType;
        public DSharpTypeToken? PropertyType { get; set; }
        public DSharpMethodBuilder? Getter { get; private set; }
        public DSharpMethodBuilder? Setter { get; private set; }
        public string GetterMethodName { get; private set; } = string.Empty;
        public string SetterMethodName { get; private set; } = string.Empty;
        public bool IsVirtual { get; set; }

        #region Управление

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
            Setter = setter;

            return setter;
        }

        public bool RemoveGetter()
        {
            if (Getter != null && DeclaringType.RemoveMethod(Getter))
            {
                Getter = null;
                return true;
            }

            return false;
        }
        public bool RemoveSetter()
        {
            if (Setter != null && DeclaringType.RemoveMethod(Setter))
            {
                Setter = null;
                return true;
            }

            return false;
        }

        #endregion
    }
}
