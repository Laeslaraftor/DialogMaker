using DialogMaker.Core.Editor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DialogMaker.Core
{
    public static class NodeExtensions
    {
        public static string[] GetPath(this DialogNodeType nodeType)
        {
            var path = nodeType.GetEnumAttribute<PathAttribute>()?.GetComponents();
            path ??= [];

            return path;
        }
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

        public static T? GetEnumAttribute<T>(this object? enumValue) where T : Attribute
        {
            return enumValue.GetEnumAttributes<T>().FirstOrDefault();
        }
        public static List<T> GetEnumAttributes<T>(this object? enumValue) where T : Attribute
        {
            if (enumValue == null)
            {
                return [];
            }

            var enumType = enumValue.GetType();
            var valueName = enumValue.ToString();

            if (valueName == null)
            {
                return [];
            }

            var memberInfo = enumType.GetMember(valueName);
            var enumValueMemberInfo = memberInfo.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo?.GetCustomAttributes(typeof(T), false);

            if (valueAttributes != null && valueAttributes.Length > 0)
            {
                return [.. valueAttributes.Cast<T>()];
            }

            return [];
        }
    }
}
