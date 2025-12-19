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
            if (!CanConvert(value))
            {
                return "0";
            }

            return RemoveDots(value?.ToString());
        }
        protected override bool CanConvert(object? value)
        {
            if (value == null)
            {
                return true;
            }

            var type = value.GetType();

            return type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(double);
        }
        protected override object? Convert(object? value)
        {
            if (value is float i)
            {
                return i;
            }
            else if (value is double d)
            {
                return System.Convert.ToInt32(d);
            }
            else if (value is float f)
            {
                return System.Convert.ToInt32(f);
            }
            else if (value is string str && TryHandle(str, out var floatObject))
            {
                return floatObject;
            }

            return 0f;
        }

        private static string RemoveDots(string? value)
        {
            if (value == null)
            {
                return "0";
            }

            return value.Replace(",", string.Empty).Replace(".", string.Empty);
        }

        #endregion
    }
}
