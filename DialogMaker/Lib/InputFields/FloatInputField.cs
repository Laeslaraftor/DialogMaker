using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DialogMaker.Lib.InputFields
{
    public class FloatInputField : TextInputField
    {
        protected override Type ValueType { get; } = typeof(float);

        #region Управление

        protected override bool TryHandle(string newValue, [NotNullWhen(true)] out object value)
        {
            value = 0;
            newValue = newValue.Trim().Replace(",", ".");

            if (float.TryParse(newValue, CultureInfo.InvariantCulture, out var number))
            {
                value = number;
                return true;
            }

            return false;
        }
        protected override string ValueToString(object? value)
        {
            if (value is not float number)
            {
                return "0";
            }

            return number.ToString().Replace(",", ".");
        }

        #endregion
    }
}
