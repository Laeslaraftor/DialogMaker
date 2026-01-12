namespace DialogMaker.Core.Editor
{
    public class DialogProjectVariableString : DialogProjectVariable<string>
    {
        public DialogProjectVariableString(DialogProjectResources resources) : base(resources)
        {
        }
        public DialogProjectVariableString(DialogProjectResources resources, DialogProjectVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public override DialogVariableType Type => DialogVariableType.String;

        #region Управление

        protected override bool CanConvertValue(object? value)
        {
            return value == null || value is string || value is OperandValue;
        }
        protected override object? ConvertValue(object? value)
        {
            if (value is string s)
            {
                return s;
            }
            else if (value is OperandValue operand)
            {
                return operand.ToString();
            }

            return string.Empty;
        }

        #endregion
    }
}
