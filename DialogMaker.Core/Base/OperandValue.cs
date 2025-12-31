using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogMaker.Core
{
    public struct OperandValue : IEquatable<OperandValue>
    {
        public OperandValue(float value)
        {
            Value = value;
        }
        public OperandValue(int value)
        {
            Value = value;
        }
        public OperandValue(double value)
        {
            Value = value;
        }
        public OperandValue(bool value)
        {
            Value = value;
        }
        public OperandValue(string? value)
        {
            Value = value;
        }
        public OperandValue(object? value)
        {
            Value = value;
        }

        public DialogVariableType Type { get; private set; }
        public object? Value
        {
            get => field;
            set
            {
                if (field?.Equals(value) != true)
                {
                    Type = GetType(value);
                    value = ConvertValue(value, Type);

                    field = value;
                }
            }
        }

        #region Управление

        public bool Compare(OperandValue other, Comparison comparison)
        {
            return Compare(Value, other.Value, comparison);
        }
        public void Add(OperandValue other)
        {
            ExecuteOperation(other,
                             (s1, s2) => s1 + s2,
                             (v, s) => v + s,
                             (s, v) => s + v,
                             (v1, v2) => v1 + v2);
        }
        public void Subtract(OperandValue other)
        {
            ExecuteOperation(other,
                             (s1, s2) =>
                             {
                                 if (!string.IsNullOrEmpty(s2))
                                 {
                                     return s1.Replace(s2, string.Empty);
                                 }

                                 return s1;
                             },
                             (v, s) => v - s.Length,
                             (s, v) => s.Length - v,
                             (v1, v2) => v1 - v2);
        }
        public void Multiply(OperandValue other)
        {
            ExecuteOperation(other,
                             (s1, s2) => s1,
                             (v, s) => s.Repeat((int)v),
                             (s, v) => s.Repeat((int)v),
                             (v1, v2) => v1 * v2);
        }
        public void Divide(OperandValue other)
        {
            static float Divide(float v1, float v2)
            {
                if (v2 == 0)
                {
                    return 0;
                }

                return v1 / v2;
            }

            ExecuteOperation(other,
                             (s1, s2) => s1,
                             (v, s) => Divide(v, s.Length),
                             (s, v) => Divide(s.Length, v),
                             (v1, v2) => Divide(v1, v2));
        }

        public override bool Equals(object obj)
        {
            return obj is OperandValue other &&
                   Equals(other);
        }
        public bool Equals(OperandValue other)
        {
            return Compare(other, Comparison.Equals);
        }
        public override string ToString()
        {
            if (Value == null)
            {
                return string.Empty;
            }

            return Value.ToString();
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        private void ExecuteOperation(OperandValue other, Func<string, string?, object> strOperation, Func<float, string, object> numberStrOperation, Func<string, float, object> numberStrOperation2, Func<float, float, float> numberOperation)
        {
            if (other.Value == null)
            {
                return;
            }
            if (Value == null)
            {
                Value = other.Value;
                return;
            }
            if (Value is string str)
            {
                if (other.Value is string otherStr)
                {
                    Value = strOperation(str, otherStr);
                    return;
                }

                Value = numberOperation(AsNumber(other.Value), AsNumber(other.Value));
            }
            else if (Value is float f)
            {
                if (other.Value is string otherStr)
                {
                    Value = numberStrOperation(f, otherStr);
                    return;
                }

                Value = numberOperation(f, AsNumber(other.Value));
            }
            else if (Type == DialogVariableType.Bool)
            {
                var number = numberOperation(AsNumber(Value), AsNumber(other.Value));
                Value = number > 0;
            }
        }

        #endregion

        #region Операторы

        public static implicit operator OperandValue(float value) => new(value);
        public static implicit operator OperandValue(double value) => new(value);
        public static implicit operator OperandValue(int value) => new(value);
        public static implicit operator OperandValue(bool value) => new(value);
        public static implicit operator OperandValue(string? value) => new(value);

        public static bool operator ==(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.Equals);
        public static bool operator ==(OperandValue v1, float v2) => v1.Compare(v2, Comparison.Equals);
        public static bool operator ==(OperandValue v1, int v2) => v1.Compare(v2, Comparison.Equals);
        public static bool operator ==(OperandValue v1, double v2) => v1.Compare(v2, Comparison.Equals);
        public static bool operator ==(OperandValue v1, bool v2) => v1.Compare(v2, Comparison.Equals);
        public static bool operator ==(OperandValue v1, string? v2) => v1.Compare(v2, Comparison.Equals);
        public static bool operator ==(float v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        public static bool operator ==(int v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        public static bool operator ==(double v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        public static bool operator ==(bool v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        public static bool operator ==(string? v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);

        public static bool operator !=(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.NotEquals);
        public static bool operator !=(OperandValue v1, float v2) => v1.Compare(v2, Comparison.NotEquals);
        public static bool operator !=(OperandValue v1, int v2) => v1.Compare(v2, Comparison.NotEquals);
        public static bool operator !=(OperandValue v1, double v2) => v1.Compare(v2, Comparison.NotEquals);
        public static bool operator !=(OperandValue v1, bool v2) => v1.Compare(v2, Comparison.NotEquals);
        public static bool operator !=(OperandValue v1, string? v2) => v1.Compare(v2, Comparison.NotEquals);
        public static bool operator !=(float v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        public static bool operator !=(int v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        public static bool operator !=(double v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        public static bool operator !=(bool v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        public static bool operator !=(string? v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);

        public static bool operator >(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.Greater);
        public static bool operator >=(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.GreaterOrEquals);
        public static bool operator <(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.Less);
        public static bool operator <=(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.LessOrEquals);

        #endregion

        #region Статика

        private static ReadOnlyDictionary<DialogVariableType, ReadOnlyCollection<Type>> VariableValueTypes
        {
            get
            {
                if (field == null)
                {
                    Dictionary<DialogVariableType, List<Type>> types = [];

                    foreach (var value in Enum.GetValues(typeof(DialogVariableType)))
                    {
                        var typesAttribute = value.GetEnumAttribute<TypesAttribute>();
                        List<Type> valueTypes = [];

                        if (typesAttribute != null)
                        {
                            valueTypes.AddRange(typesAttribute.Types);
                        }

                        types.Add((DialogVariableType)value, valueTypes);
                    }

                    Dictionary<DialogVariableType, ReadOnlyCollection<Type>> result = [];

                    foreach (var info in types)
                    {
                        result.Add(info.Key, new(info.Value));
                    }

                    field = new(result);
                }

                return field;
            }
        }

        public static DialogVariableType GetType(object? value)
        {
            if (value == null)
            {
                return DialogVariableType.Number;
            }

            Type valueType = value.GetType();

            foreach (var info in VariableValueTypes)
            {
                foreach (var type in info.Value)
                {
                    if (type == valueType)
                    {
                        return info.Key;
                    }
                }
            }

            throw new ArgumentException($"Неподдерживаемый тип: {valueType}");
        }

        public static float AsNumber(object? value)
        {
            if (value is string str)
            {
                return str.Length;
            }
            else if (value is float f)
            {
                return f;
            }
            else if (value is bool b)
            {
                return b ? 1f : 0f;
            }
            else if (value is int i)
            {
                return i;
            }
            else if (value is double d)
            {
                return (float)d;
            }

            return 0f;
        }
        public static bool Compare(object? value1, object? value2, Comparison comparison)
        {
            if (comparison == Comparison.Equals)
            {
                return (value1 == null && value2 == null) ||
                       value1?.Equals(value2) == true;
            }
            else if (comparison != Comparison.NotEquals)
            {
                return (value1 == null && value2 != null) ||
                       (value1 != null && value2 == null) ||
                       value1?.Equals(value2) != true;
            }

            float number1 = AsNumber(value1);
            float number2 = AsNumber(value2);

            return comparison switch
            {
                Comparison.Greater => number1 > number2,
                Comparison.GreaterOrEquals => number1 >= number2,
                Comparison.Less => number1 < number2,
                Comparison.LessOrEquals => number1 <= number2,
                _ => false,
            };
        }
        public static object ConvertValue(object? value, DialogVariableType type)
        {
            return type switch
            {
                DialogVariableType.Number => AsNumber(value),
                DialogVariableType.Bool => AsNumber(value) > 0,
                DialogVariableType.String => value == null ? string.Empty : value.ToString(),
                _ => 0f,
            };
        }

        #endregion
    }

}
