using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
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
            if (valueType == convertType || convertType == DialogNodePortType.Object)
            {
                return value;
            }
            if (value == null)
            {
                return convertType.GetDefaultValue();
            }
            if (convertType == DialogNodePortType.String)
            {
                if (value is IResourceString resourceString)
                {
                    return resourceString;
                }

                var text = value.ToString().Replace(",", ".");

                if (string.IsNullOrEmpty(text))
                {
                    return ResourceString.Empty;
                }

                return new ResourceString(text);
            }

            if (value is IResourceString resource)
            {
                value = resource.Text;
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

            throw new ArgumentException($"Не удалось преобразовать {value} из {valueType} в {convertType}");
        }
        public DialogNodePortType TypeOf(object? instance)
        {
            if (instance == null)
            {
                return DialogNodePortType.Object;
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
            else if (instance is string || 
                     instance is IResourceString)
            {
                return DialogNodePortType.String;
            }
            else if (instance is bool)
            {
                return DialogNodePortType.Bool;
            }

            return DialogNodePortType.Object;
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
