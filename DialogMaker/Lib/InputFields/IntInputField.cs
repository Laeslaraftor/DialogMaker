using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DialogMaker.Lib.InputFields
{
    public class IntInputField : TextInputField
    {
        protected override Type ValueType { get; } = typeof(int);

        #region Управление

        protected override bool TryHandle(string newValue, [NotNullWhen(true)] out object value)
        {
            value = 0;
            newValue = RemoveDots(newValue.Trim());

            if (int.TryParse(newValue, CultureInfo.InvariantCulture, out var number))
            {
                value = number;
                return true;
            }

            return false;
        }
        protected override string ValueToString(object? value)
        {
            if (value is not int number)
            {
                return "0";
            }

            return RemoveDots(number.ToString());
        }

        private static string RemoveDots(string value)
        {
            return value.Replace(",", string.Empty).Replace(".", string.Empty);
        }

        #endregion
    }
}
