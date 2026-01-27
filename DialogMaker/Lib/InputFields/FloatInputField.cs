using Acly;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DialogMaker.Lib.InputFields
{
    public class FloatInputField : SliderInputField
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
            if (!CanConvert(value))
            {
                return "0";
            }

            return value?.ToString()?.Replace(",", ".") ?? "0";
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
            return Helper.Clamp(ToFloat(value), MinValue, MaxValue);
        }

        private float ToFloat(object? value)
        {
            if (value is float f)
            {
                return f;
            }
            else if (value is double d)
            {
                return System.Convert.ToSingle(d);
            }
            else if (value is int i)
            {
                return System.Convert.ToSingle(i);
            }
            else if (value is string str && TryHandle(str, out var floatObject))
            {
                return (float)floatObject;
            }

            return 0;
        }

        #endregion
    }
}
