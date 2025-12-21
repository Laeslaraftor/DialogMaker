namespace DialogMaker.Core.Editor
{
    public class DialogProjectVariableBool : DialogProjectVariable<bool>
    {
        public DialogProjectVariableBool(DialogProjectResources resources) : base(resources)
        {
        }
        public DialogProjectVariableBool(DialogProjectResources resources, DialogProjectVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogVariableType Type => DialogVariableType.Bool;

        #region Управление

        protected override bool CanConvertValue(object? value)
        {
            return value == null || value is bool;
        }
        protected override object? ConvertValue(object? value)
        {
            if (value is bool b)
            {
                return b;
            }

            return false;
        }

        #endregion
    }
}
