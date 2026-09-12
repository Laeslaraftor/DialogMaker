using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static DialogMaker.Core.Scripting.CodeAnalyzer.CodeGeneratorHelper;

namespace DialogMaker.Core.Scripting.CodeAnalyzer
{
    internal class EnumInformationGenerator(INamedTypeSymbol enumType, ImmutableArray<IFieldSymbol> values)
    {
        public INamedTypeSymbol EnumType { get; } = enumType;
        public string Name { get; } = enumType.Name;
        public string? Namespace { get; } = enumType.Namespace;
        public string EnumTypeFullName { get; } = enumType.FullName;
        public string InfoTypeName { get; } = enumType.Name + "Info";
        public ImmutableArray<IFieldSymbol> Values { get; } = values;

        private AttributesInfo AttributesFieldsInfo
        {
            get
            {
                field ??= AttributesInfo.Create(this);
                return field;
            }
        }

        #region Controls

        public void WriteValuesField(StringBuilder builder, int indentIndex, string fieldName)
        {
            var indent = GetIndent(indentIndex);

            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// Array of <see cref=\"{Name}\"/> values");
            builder.AppendLine($"{indent}/// </summary>");

            if (Values.Length == 0)
            {
                builder.AppendLine($"{indent}public static readonly {EnumTypeFullName} {fieldName} = {ImmutableArrayFullName}<{EnumTypeFullName}>.Empty;");
                return;
            }

            var indent2 = GetIndent(indentIndex + 1);
            builder.AppendLine($"{indent}public static readonly {ImmutableArrayFullName}<{EnumTypeFullName}> {fieldName} = {ImmutableArrayFullName}.Create(");

            for (int i = 0; i < Values.Length; i++)
            {
                var field = Values[i];

                builder.Append($"{indent2}{EnumTypeFullName}.{field.Name}");

                if (i + 1 < Values.Length)
                {
                    builder.Append(',');
                }

                builder.AppendLine();
            }

            builder.AppendLine($"{indent});");
        }
        public void WriteValueInfoDeclaration(StringBuilder builder, int indentIndex)
        {
            var fieldsInfo = AttributesFieldsInfo;
            var indent = GetIndent(indentIndex);

            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// Information about value of <see cref=\"{Name}\"/>");
            builder.AppendLine($"{indent}/// </summary>");
            builder.AppendLine($"{indent}public readonly struct {InfoTypeName}");
            builder.AppendLine($"{indent}{{");
            var indent2 = GetIndent(indentIndex + 1);
            var indent3 = GetIndent(indentIndex + 2);

            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Create information about value of <see cref=\"{Name}\"/>");
            builder.AppendLine($"{indent2}/// </summary>");

            if (fieldsInfo.Fields.Count == 0)
            {
                builder.Append($"{indent2}public {InfoTypeName}({EnumTypeFullName} value)");
                builder.AppendLine($"{indent2}{{");
                builder.AppendLine($"{indent3}Value = value;");
                builder.AppendLine($"{indent2}}}");
                builder.AppendLine();
            }
            else
            {
                builder.Append($"{indent2}public {InfoTypeName}(");
                builder.Append($"{EnumTypeFullName} value, ");
                Dictionary<string, string> parameters = [];

                builder.Append(string.Join(", ", fieldsInfo.Fields.Select(info =>
                {
                    var parameterName = info.Name.ToCamelCase();
                    parameters.Add(info.Name, parameterName);

                    return $"{info.TypeName} {parameterName}";
                })));

                builder.Append(')');
                builder.AppendLine();
                builder.AppendLine($"{indent2}{{");

                builder.AppendLine($"{indent3}Value = value;");

                foreach (var info in parameters)
                {
                    builder.AppendLine($"{indent3}{info.Key} = {info.Value};");
                }

                builder.AppendLine($"{indent2}}}");
                builder.AppendLine();
            }

            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Value of <see cref=\"{Name}\"/> which attached current information");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public readonly {EnumTypeFullName} Value;");

            foreach (var info in fieldsInfo.Fields)
            {
                builder.AppendLine($"{indent2}/// <summary>");
                builder.AppendLine($"{indent2}/// Attribute <see cref=\"{info.TypeName}\"/> that attached to value which describes by current information");
                builder.AppendLine($"{indent2}/// </summary>");
                builder.AppendLine($"{indent2}public readonly {info.TypeName} {info.Name};");
            }

            builder.AppendLine();
            builder.AppendLine($"{indent2}/// <summary>");
            builder.AppendLine($"{indent2}/// Extract value of <see cref=\"{Name}\"/> from information");
            builder.AppendLine($"{indent2}/// </summary>");
            builder.AppendLine($"{indent2}public static implicit operator {EnumTypeFullName}({InfoTypeName} info) => info.Value;");
            builder.AppendLine($"{indent}}}");
        }
        public void WriteValuesInfo(StringBuilder builder, int indentIndex, string fieldName)
        {
            var fieldsInfo = AttributesFieldsInfo;
            var indent = GetIndent(indentIndex);

            builder.AppendLine($"{indent}/// <summary>");
            builder.AppendLine($"{indent}/// Dictionary with information about values of <see cref=\"{Name}\"/>");
            builder.AppendLine($"{indent}/// </summary>");

            if (Values.Length == 0)
            {
                builder.AppendLine($"{indent}public static readonly {ImmutableArrayFullName}<{InfoTypeName}> {fieldName} = {ImmutableArrayFullName}<{InfoTypeName}>.Empty;");
                return;
            }

            var indent2 = GetIndent(indentIndex + 1);
            var indent3 = GetIndent(indentIndex + 2);

            builder.AppendLine($"{indent}public static readonly {ReadOnlyDictionaryFullName}<{EnumTypeFullName}, {InfoTypeName}> {fieldName} = new(new {DictionaryFullName}<{EnumTypeFullName}, {InfoTypeName}>()");
            builder.AppendLine($"{indent}{{");

            for (int i = 0; i < Values.Length; i++)
            {
                var field = Values[i];
                var attributes = field.GetAttributes();

                builder.Append($"{indent2}[{EnumTypeFullName}.{field.Name}] = ");

                if (attributes.Length == 0 && fieldsInfo.Fields.Count == 0)
                {
                    builder.Append($"new {InfoTypeName}({EnumTypeFullName}.{field.Name})");
                }
                else
                {
                    builder.AppendLine($"new {InfoTypeName}(");
                    builder.Append($"{indent3}{EnumTypeFullName}.{field.Name}");
                    builder.AppendLine(", ");

                    for (int a = 0; a < fieldsInfo.Fields.Count; a++)
                    {
                        var attributeInfo = fieldsInfo.Fields[a];
                        var attribute = attributes.FirstOrDefault(a => attributeInfo.Type.Compare(a.AttributeClass));

                        if (attribute == null)
                        {
                            builder.Append($"{indent3}null");
                        }
                        else
                        {
                            if (attributeInfo.IsImmutableArray)
                            {

                            }
                            else
                            {
                                builder.Append($"{indent3}new(");
                                builder.Append(string.Join(", ", attribute.ConstructorArguments.Select(arg => arg.ToDisplayString())));
                                builder.Append(")");
                            }
                        }

                        if (a + 1 < fieldsInfo.Fields.Count)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                    }

                    builder.Append($"{indent2})");
                }


                if (i + 1 < Values.Length)
                {
                    builder.Append(',');
                }

                builder.AppendLine();
            }

            builder.AppendLine($"{indent}}});");
        }

        #endregion

        #region Static

        public static EnumInformationGenerator Create(INamedTypeSymbol enumType)
        {
            var values = enumType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.HasConstantValue)
                .ToImmutableArray();

            return new(enumType, values);
        }

        #endregion

        #region Classes

        private class AttributesInfo(List<FieldInfo> fields)
        {
            public ReadOnlyCollection<FieldInfo> Fields = new(fields);

            private readonly List<FieldInfo> _fields = fields;
            private const string AttributeWord = "Attribute";

            public int IndexOf(INamedTypeSymbol? attributeType)
            {
                if (attributeType == null)
                {
                    return -1;
                }

                int index = 0;

                foreach (var field in _fields)
                {
                    if (field.Type.Compare(attributeType))
                    {
                        return index;
                    }

                    index++;
                }

                return -1;
            }
            public bool TryGetFieldName(INamedTypeSymbol? attributeType, [NotNullWhen(true)] out string? result)
            {
                if (attributeType == null)
                {
                    result = null;
                    return false;
                }

                result = _fields.FirstOrDefault(i => i.Type.Compare(attributeType)).Name;
                return result != null;
            }

            public static AttributesInfo Create(EnumInformationGenerator generator)
            {
                Dictionary<INamedTypeSymbol, FieldInfo> attributesFields = [];
                HashSet<INamedTypeSymbol> currentFieldAttributes = [];

                foreach (var field in generator.Values)
                {
                    currentFieldAttributes.Clear();
                    var attributes = field.GetAttributes();

                    foreach (var attribute in attributes)
                    {
                        var attributeClass = attribute.AttributeClass;

                        if (attributeClass == null)
                        {
                            continue;
                        }
                        if (attributesFields.TryGetValue(attributeClass, out var fieldInfo))
                        {
                            if (!fieldInfo.IsImmutableArray)
                            {
                                if (currentFieldAttributes.Contains(attributeClass))
                                {
                                    fieldInfo = new(attributeClass, true, $"{ImmutableArrayFullName}<{fieldInfo.TypeName.Replace("?", string.Empty)}>", fieldInfo.Name);
                                    attributesFields[attributeClass] = fieldInfo;
                                }
                                else
                                {
                                    currentFieldAttributes.Add(attributeClass);
                                }
                            }

                            continue;
                        }

                        currentFieldAttributes.Add(attributeClass);
                        var name = FormatAttributeName(attributeClass.Name);
                        fieldInfo = new(attributeClass, false, attributeClass.FullName + '?', name);
                        attributesFields.Add(attributeClass, fieldInfo);
                    }
                }

                return new([.. attributesFields.Values]);
            }

            private static string FormatAttributeName(string name)
            {
                if (!name.EndsWith(AttributeWord) ||
                    name.Length <= AttributeWord.Length)
                {
                    return name;
                }

                return name.Substring(0, name.Length - AttributeWord.Length);
            }
        }
        private readonly struct FieldInfo(INamedTypeSymbol type, bool isImmutableArray, string typeName, string name)
        {
            public INamedTypeSymbol Type { get; } = type;
            public bool IsImmutableArray { get; } = isImmutableArray;
            public string TypeName { get; } = typeName;
            public string Name { get; } = name;
        }

        #endregion
    }
}
