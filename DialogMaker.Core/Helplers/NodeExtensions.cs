using DialogMaker.Core.Editor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DialogMaker.Core
{
    public static class NodeExtensions
    {
        private static readonly Dictionary<DialogNodePortType, ReadOnlyCollection<TypeAttribute>> _typesInfo = [];

        public static bool CanConvert(this IPortDataConverter converter, DialogProjectNodePort? port1, DialogProjectNodePort? port2)
        {
            if (port1 == null || port2 == null)
            {
                return false;
            }

            DialogNodePortType from = DialogNodePortType.Action;
            DialogNodePortType to = DialogNodePortType.Action;

            if (port1 is DialogProjectNodeInput input &&
                port2 is DialogProjectNodeOutput output)
            {
                from = output.DataType;
                to = input.DataType;
            }
            else if (port2 is DialogProjectNodeInput input2 &&
                     port1 is DialogProjectNodeOutput output2)
            {
                from = output2.DataType;
                to = input2.DataType;
            }
            else
            {
                return false;
            }

            return converter.CanConvert(from, to);
        }
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
        public static ReadOnlyCollection<TypeAttribute> GetInfo(this DialogNodePortType portType)
        {
            if (!_typesInfo.TryGetValue(portType, out var typeInfo))
            {
                var typeInfos = portType.GetEnumAttributes<TypeAttribute>();

                if (typeInfos == null || typeInfos.Count == 0)
                {
                    typeInfo = new([]);
                }
                else
                {
                    typeInfo = new(typeInfos);
                }

                _typesInfo.Add(portType, typeInfo);
            }

            return typeInfo;
        }
        public static object GetDefaultValue(this DialogNodePortType portType)
        {
            foreach (var info in portType.GetInfo())
            {
                return info.DefaultValue;
            }

            throw new ArgumentException($"Не удалось получить значение по умолчанию для {portType}", nameof(portType));
        }
        public static Type GetDefaultType(this DialogNodePortType portType)
        {
            foreach (var info in portType.GetInfo())
            {
                return info.Type;
            }

            throw new ArgumentException($"Не удалось получить тип для {portType}", nameof(portType));
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
