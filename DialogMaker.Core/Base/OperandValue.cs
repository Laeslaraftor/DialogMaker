using System.Collections.ObjectModel;
using System.Globalization;

namespace DialogMaker.Core
{
    /// <summary>
    /// Структура, представляющая какое либо значение переменной диалога.
    /// </summary>
    public struct OperandValue : IEquatable<OperandValue>
    {
        /// <summary>
        /// Создать значение переменной на основе <see cref="float"/>
        /// </summary>
        /// <param name="value">Значение переменной</param>
        public OperandValue(float value)
        {
            Value = value;
        }
        /// <summary>
        /// Создать значение переменной на основе <see cref="int"/>
        /// </summary>
        /// <param name="value">Значение переменной</param>
        public OperandValue(int value)
        {
            Value = value;
        }
        /// <summary>
        /// Создать значение переменной на основе <see cref="double"/>
        /// </summary>
        /// <param name="value">Значение переменной</param>
        public OperandValue(double value)
        {
            Value = value;
        }
        /// <summary>
        /// Создать значение переменной на основе <see cref="bool"/>
        /// </summary>
        /// <param name="value">Значение переменной</param>
        public OperandValue(bool value)
        {
            Value = value;
        }
        /// <summary>
        /// Создать значение переменной на основе <see cref="string"/>
        /// </summary>
        /// <param name="value">Значение переменной</param>
        public OperandValue(string? value)
        {
            Value = value;
        }
        /// <summary>
        /// Создать значение переменной на основе <see cref="object"/>
        /// </summary>
        /// <param name="value">Значение переменной</param>
        public OperandValue(object? value)
        {
            Value = value;
        }

        /// <summary>
        /// Тип значения
        /// </summary>
        public DialogVariableType Type { get; private set; }
        /// <summary>
        /// Значение. 
        /// Может быть следующими типами: <see cref="float"/>, <see cref="bool"/>, <see cref="string"/>
        /// </summary>
        public object? Value
        {
            get;
            set
            {
                if (!Equals(field, value))
                {
                    Type = GetType(value);
                    field = ConvertValue(value, Type);
                }
            }
        }

        #region Управление

        /// <summary>
        /// Сравнить с другим значением
        /// </summary>
        /// <param name="other">Значение для сравнения</param>
        /// <param name="comparison">Тип сравнения</param>
        /// <returns>Результат сравнения</returns>
        public readonly bool Compare(OperandValue other, Comparison comparison)
        {
            return Compare(Value, other.Value, comparison);
        }
        /// <summary>
        /// Прибавить значение
        /// </summary>
        /// <param name="other">Прибавляемое значение</param>
        public void Add(OperandValue other)
        {
            ExecuteOperation(other,
                             (s1, s2) => s1 + s2,
                             (v, s) => v + s,
                             (s, v) => s + v,
                             (v1, v2) => v1 + v2);
        }
        /// <summary>
        /// Вычесть значение
        /// </summary>
        /// <param name="other">Вычитаемое</param>
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
        /// <summary>
        /// Умножить значение
        /// </summary>
        /// <param name="other">Множитель</param>
        public void Multiply(OperandValue other)
        {
            ExecuteOperation(other,
                             (s1, s2) => s1,
                             (v, s) => s.Repeat((int)v),
                             (s, v) => s.Repeat((int)v),
                             (v1, v2) => v1 * v2);
        }
        /// <summary>
        /// Разделить значение
        /// </summary>
        /// <param name="other">Делитель</param>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public override readonly bool Equals(object? obj)
        {
            return obj is OperandValue other &&
                   Equals(other);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public readonly bool Equals(OperandValue other)
        {
            return Compare(other, Comparison.Equals);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override readonly string ToString()
        {
            if (Value == null)
            {
                return string.Empty;
            }

            return Value.ToString();
        }
        /// <summary>
        /// Преобразовать в число
        /// </summary>
        /// <returns>Число из текущего значение</returns>
        public readonly float ToNumber() => AsNumber(Value);
        /// <summary>
        /// Преобразовать в булево значение
        /// </summary>
        /// <returns>Булево значение из текущего</returns>
        public readonly bool ToBool() => ToNumber() > 0;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override readonly int GetHashCode()
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator OperandValue(float value) => new(value);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator OperandValue(double value) => new(value);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator OperandValue(int value) => new(value);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator OperandValue(bool value) => new(value);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator OperandValue(string? value) => new(value);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator float(OperandValue value) => value.ToNumber();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator double(OperandValue value) => value.ToNumber();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator int(OperandValue value) => (int)value.ToNumber();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator bool(OperandValue value) => value.ToBool();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"><inheritdoc/></param>
        public static implicit operator string(OperandValue value) => value.ToString();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(OperandValue v1, float v2) => v1.Compare(v2, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(OperandValue v1, int v2) => v1.Compare(v2, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(OperandValue v1, double v2) => v1.Compare(v2, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(OperandValue v1, bool v2) => v1.Compare(v2, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(OperandValue v1, string? v2) => v1.Compare(v2, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(float v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(int v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(double v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(bool v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator ==(string? v1, OperandValue v2) => v2.Compare(v1, Comparison.Equals);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(OperandValue v1, float v2) => v1.Compare(v2, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(OperandValue v1, int v2) => v1.Compare(v2, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(OperandValue v1, double v2) => v1.Compare(v2, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(OperandValue v1, bool v2) => v1.Compare(v2, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(OperandValue v1, string? v2) => v1.Compare(v2, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(float v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(int v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(double v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(bool v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator !=(string? v1, OperandValue v2) => v2.Compare(v1, Comparison.NotEquals);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator >(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.Greater);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator >=(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.GreaterOrEquals);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public static bool operator <(OperandValue v1, OperandValue v2) => v1.Compare(v2, Comparison.Less);
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="v1"><inheritdoc/></param>
        /// <param name="v2"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
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

        /// <summary>
        /// Получить тип значения по объекту
        /// </summary>
        /// <param name="value">Объект, тип которого надо получить</param>
        /// <returns>Тип значения объекта</returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Преобразовать объект в число
        /// </summary>
        /// <param name="value">Объект, который надо преобразовать в число</param>
        /// <returns>Число из объекта</returns>
        public static float AsNumber(object? value)
        {
            if (value is string str)
            {
                if (float.TryParse(str.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
                {
                    return number;
                }

                return str.Length;
            }
            else if (value is float f)
            {
                return f;
            }
            else if (value is bool b)
            {
                return b ? 1 : 0;
            }
            else if (value is int i)
            {
                return i;
            }
            else if (value is double d)
            {
                return (float)d;
            }

            return 0;
        }
        /// <summary>
        /// Сравнить объект со строкой. 
        /// Сравниваемый объект сначала преобразован в число, а затем в строку, после чего и будет сравниваться
        /// </summary>
        /// <param name="value">Сравниваемый объект</param>
        /// <param name="str">Сравниваемая строка</param>
        /// <returns>Равны ли значения</returns>
        public static bool Compare(object? value, string str)
        {
            if (value is string str2)
            {
                return str2 == str;
            }

            float number = AsNumber(value);
            string strNumber = number.ToString();

            return (number == 0 && (str == "0" || str == "null" || str.Equals("False", StringComparison.InvariantCultureIgnoreCase))) ||
                   (number > 0 && (str == strNumber || str == strNumber.Replace(",", ".") || str.Equals("True", StringComparison.InvariantCultureIgnoreCase)));
        }
        /// <summary>
        /// Сравнить объекты
        /// </summary>
        /// <param name="value1">Первое значение для сравнения</param>
        /// <param name="value2">Второе значение для сравнения</param>
        /// <param name="comparison">Тип сравнения</param>
        /// <returns>Результат сравнения</returns>
        public static bool Compare(object? value1, object? value2, Comparison comparison)
        {
            if (comparison == Comparison.Equals)
            {
                if (value1 == null && value2 == null)
                {
                    return true;
                }
                if (value1 is string str1 && value2 is string str2)
                {
                    return str1 == str2;
                }
                else if (value1 is string str11)
                {
                    return Compare(value2, str11);
                }
                else if (value2 is string str22)
                {
                    return Compare(value1, str22);
                }

                var n1 = AsNumber(value1);
                var n2 = AsNumber(value2);

                return n1 == n2;
            }
            else if (comparison == Comparison.NotEquals)
            {
                return !Compare(value1, value2, Comparison.Equals);
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
        /// <summary>
        /// Конвертировать объект в указанный тип
        /// </summary>
        /// <param name="value">Конвертируемый объект</param>
        /// <param name="type">Тип значения в который надо конвертировать объект</param>
        /// <returns>Результат конвертации в указанный тип</returns>
        public static object ConvertValue(object? value, DialogVariableType type)
        {
            return type switch
            {
                DialogVariableType.Number => AsNumber(value),
                DialogVariableType.Bool => AsNumber(value) > 0,
                DialogVariableType.String => value == null ? string.Empty : value.ToString(),
                _ => 0,
            };
        }
        /// <summary>
        /// Конвертировать объект в число
        /// </summary>
        /// <param name="value">Конвертируемый объект</param>
        /// <returns>Число из объекта</returns>
        public static object ObjectToNumber(object? value)
        {
            if (value is float f)
            {
                return f;
            }
            else if (value is int i)
            {
                return (float)i;
            }
            else if (value is string str)
            {
                str = str.Replace(",", ".");

                if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
                {
                    return number;
                }
            }

            return 0;
        }

        #endregion
    }

}
