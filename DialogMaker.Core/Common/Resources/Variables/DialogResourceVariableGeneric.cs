using DialogMaker.Core.Common.SavedStates;
using DialogMaker.Core.Editor;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Common
{
    public abstract class DialogResourceVariableGeneric<T> : DialogResourceVariable
    {
        public DialogResourceVariableGeneric(DialogResources resources, DialogProjectVariable variable) : base(resources, variable)
        {
        }
        public DialogResourceVariableGeneric(DialogResources resources, DialogResourceVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public new T Value
        {
            get
            {
                if (base.Value is not T value)
                {
                    return DefaultValue;
                }

                return value;
            }
            set => base.Value = new(value);
        }

        [NotNull]
        protected abstract T DefaultValue { get; }

        #region Управление

        protected override object ConvertValue(object? value)
        {
            if (value == null)
            {
                return DefaultValue;
            }
            if (value is T)
            {
                return value;
            }

            return Convert.ChangeType(value, typeof(T));
        }

        #endregion
    }
}
