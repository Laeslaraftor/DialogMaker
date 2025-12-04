using DialogMaker.Core.Editor.Nodes;
using System;

namespace DialogMaker.Core
{
    public static class NodeExtensions
    {
        public static DialogNodeConnectionType ToConnectionType(this DialogNodePortType portType)
        {
            if (portType == DialogNodePortType.Action)
            {
                return DialogNodeConnectionType.Action;
            }

            return DialogNodeConnectionType.Data;
        }
        public static object GetDefaultValue(this DialogNodePortType portType)
        {
            if (portType == DialogNodePortType.Action)
            {
                throw new ArgumentException($"Невозможно получить значение по умолчанию для {portType}");
            }
            if (portType == DialogNodePortType.String)
            {
                return string.Empty;
            }
            if (portType == DialogNodePortType.Bool)
            {
                return false;
            }
            if (portType == DialogNodePortType.Float)
            {
                return 0f;
            }

            return 0;
        }
        public static object? Convert(this IPortDataConverter converter, object? value, DialogNodePortType convertType)
        {
            return converter.Convert(converter.TypeOf(value), value, convertType);
        }
    }
}
