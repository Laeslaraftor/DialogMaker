namespace DialogMaker.Core.Editor
{
    public class DialogProjectVariableNumber : DialogProjectVariable<float>
    {
        public DialogProjectVariableNumber(DialogProjectResources resources) : base(resources)
        {
        }
        public DialogProjectVariableNumber(DialogProjectResources resources, DialogProjectVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogVariableType Type => DialogVariableType.Number;

        #region Управление

        protected override bool CanConvertValue(object? value)
        {
            return value == null || value is int || value is float || value is double;
        }
        protected override object? ConvertValue(object? value)
        {
            if (value == null)
            {
                return 0f;
            }
            if (value is float f)
            {
                return f;
            }
            else if (value is double d)
            {
                return d;
            }
            else if(value is int i)
            {
                return i;
            }

            return 0f;
        }

        #endregion
    }
}
