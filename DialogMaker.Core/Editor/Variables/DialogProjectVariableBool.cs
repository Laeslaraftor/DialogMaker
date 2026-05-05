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
            return value == null ||
                   value is bool ||
                   value is OperandValue ||
                   value is int ||
                   value is double ||
                   value is float;
        }
        protected override object? ConvertValue(object? value)
        {
            if (value is bool b)
            {
                return b;
            }
            else if (value is int || value is double || value is float)
            {
                return Convert.ToSingle(value) > 0;
            }
            else if (value is OperandValue operand)
            {
                if (operand.Value is bool bValue)
                {
                    return bValue;
                }

                return operand.ToNumber() > 0;
            }

            return false;
        }

        #endregion
    }
}
