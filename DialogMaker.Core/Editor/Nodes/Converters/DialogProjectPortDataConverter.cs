using System;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectPortDataConverter : IPortDataConverter
    {
        public bool CanConvert(DialogNodePortType from, DialogNodePortType to)
        {
            if ((to == DialogNodePortType.Object && from != DialogNodePortType.Action) ||
                (from == DialogNodePortType.Object && to != DialogNodePortType.Action))
            {
                return true;
            }
            if (AtLeastOneEquals(from, to, DialogNodePortType.Action) ||
                (from == DialogNodePortType.String && to != DialogNodePortType.String))
            {
                return false;
            }

            return true;
        }
        public object? Convert(DialogNodePortType valueType, object? value, DialogNodePortType convertType)
        {
            if (valueType == convertType)
            {
                return value;
            }
            if (value == null)
            {
                throw new ArgumentNullException("Значение не указано", nameof(value));
            }
            if (convertType == DialogNodePortType.Bool)
            {
                if (valueType == DialogNodePortType.Number ||
                    valueType == DialogNodePortType.Number)
                {
                    if (value is IComparable comparable)
                    {
                        return comparable.CompareTo(0) > 0;
                    }
                }
            }
            else if (convertType == DialogNodePortType.Number)
            {
                if (valueType == DialogNodePortType.Number ||
                    valueType == DialogNodePortType.Bool)
                {
                    return System.Convert.ToInt32(value);
                }
            }
            else if (convertType == DialogNodePortType.Number)
            {
                if (valueType == DialogNodePortType.Number ||
                    valueType == DialogNodePortType.Bool)
                {
                    return System.Convert.ToSingle(value);
                }
            }
            else if (convertType == DialogNodePortType.String)
            {
                return value.ToString().Replace(",", ".");
            }

            throw new ArgumentException($"Не удалось преобразовать {value} из {valueType} в {convertType}");
        }
        public DialogNodePortType TypeOf(object? instance)
        {
            if (instance == null)
            {
                return DialogNodePortType.Action;
            }
            if (instance is int ||
                instance is byte ||
                instance is short ||
                instance is long)
            {
                return DialogNodePortType.Number;
            }
            else if (instance is float ||
                     instance is double)
            {
                return DialogNodePortType.Number;
            }
            else if (instance is string)
            {
                return DialogNodePortType.String;
            }
            else if (instance is bool)
            {
                return DialogNodePortType.Bool;
            }

            throw new NotSupportedException($"Неизвестный тип: {instance.GetType()}");
        }

        private bool AtLeastOneEquals(DialogNodePortType from, DialogNodePortType to, DialogNodePortType type)
        {
            return (from == type && to != type) ||
                   (from != type && to == type);
        }

        #region Статика

        public static readonly DialogProjectPortDataConverter Instance = new();

        #endregion
    }
}
