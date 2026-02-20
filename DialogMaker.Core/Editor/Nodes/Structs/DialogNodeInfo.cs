using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DialogMaker.Core.Editor.Nodes
{
    public readonly struct DialogNodeInfo(DialogNodeType nodeType, Type type, DialogProjectNodeMetadata metadata, string path, 
                                          IDictionary<PropertyInfo, DialogProjectNodeMetadata> inputs, 
                                          IDictionary<PropertyInfo, DialogProjectNodeMetadata> outputs, 
                                          IDictionary<PropertyInfo, DialogProjectNodeMetadata> properties)
        : IEquatable<DialogNodeInfo>, IComparable, IComparable<DialogNodeInfo>
    {
        public DialogNodeType NodeType { get; } = nodeType;
        public Type Type { get; } = type;
        public DialogProjectNodeMetadata Metadata { get; } = metadata;
        public string Path { get; } = path;
        public ReadOnlyDictionary<PropertyInfo, DialogProjectNodeMetadata> Inputs { get; } = new(inputs);
        public ReadOnlyDictionary<PropertyInfo, DialogProjectNodeMetadata> Outputs { get; } = new(outputs);
        public ReadOnlyDictionary<PropertyInfo, DialogProjectNodeMetadata> Properties { get; } = new(properties);

        #region Управление

        public int CompareTo(object obj)
        {
            if (obj is DialogNodeInfo other)
            {
                return CompareTo(other);
            }
            if (Metadata.Name == null)
            {
                return -1;
            }

            return Metadata.Name.CompareTo(obj);
        }
        public int CompareTo(DialogNodeInfo other)
        {
            return Metadata.CompareTo(other.Metadata);
        }
        public override bool Equals(object obj)
        {
            return obj is DialogNodeInfo other && Equals(other);
        }
        public bool Equals(DialogNodeInfo other)
        {
            return NodeType == other.NodeType &&
                   Type == other.Type &&
                   Metadata.Equals(other.Metadata) &&
                   Path == other.Path &&
                   Inputs == other.Inputs &&
                   Outputs == other.Outputs;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NodeType, Type, Metadata, Path, Inputs, Outputs) ;
        }
        public override string ToString()
        {
            return Metadata.ToString();
        }

        #endregion

        #region Статика

        private static readonly Type _inputPortType = typeof(DialogProjectNodeInput);
        private static readonly Type _outputPortType = typeof(DialogProjectNodeOutput);

        public static bool TryCreate(DialogNodeType type, [NotNullWhen(true)] out DialogNodeInfo result)
        {
            result = default;

            var managedType = type.GetEnumAttribute<NodeAttribute>()?.NodeType;

            if (managedType == null)
            {
                return false;
            }

            var name = type.GetEnumAttribute<NameAttribute>()?.Name ?? type.ToString();
            var description = type.GetEnumAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
            var path = type.GetEnumAttribute<PathAttribute>()?.Path ?? string.Empty;
            Dictionary<PropertyInfo, DialogProjectNodeMetadata> inputs = [];
            Dictionary<PropertyInfo, DialogProjectNodeMetadata> outputs = [];
            Dictionary<PropertyInfo, DialogProjectNodeMetadata> properties = [];

            foreach (var property in managedType.GetProperties())
            {
                var nameAttribute = property.GetCustomAttribute<NameAttribute>();

                if (nameAttribute == null)
                {
                    continue;
                }

                var metadataDictionary = properties;

                if (nameAttribute is NodeInputAttribute &&
                    _inputPortType.IsAssignableFrom(property.PropertyType))
                {
                    metadataDictionary = inputs;
                }
                else if (nameAttribute is NodeOutputAttribute &&
                    _outputPortType.IsAssignableFrom(property.PropertyType))
                {
                    metadataDictionary = outputs;
                }

                var sortValue = property.GetCustomAttribute<SortAttribute>()?.SortValue;
                var propertyDescription = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

                metadataDictionary.Add(property, new(nameAttribute.Name, propertyDescription, sortValue));
            }

            result = new(type, managedType, new(name, description), path, inputs, outputs, properties);

            return true;
        }
        public static DialogNodeInfo Create(DialogNodeType type)
        {
            if (TryCreate(type, out var info))
            {
                return info;
            }

            throw new ArgumentException($"Не удалось получить информацию об узле {type}", nameof(type));
        }

        #endregion
    }
}
