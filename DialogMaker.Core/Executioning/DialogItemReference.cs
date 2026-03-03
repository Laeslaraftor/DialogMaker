using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DialogMaker.Core.Executioning
{
    public readonly struct DialogItemReference(DialogItemType type, OperandValue value) : IEquatable<DialogItemReference>
    {
        public DialogItemType Type { get; } = type;
        public OperandValue Value { get; } = value;

        #region Управление

        public IResourceItem GetItem(IResourcesOwner resourcesOwner)
        {
            if (Type == DialogItemType.Variable)
            {
                return new LocalVariable(Value);
            }
            else if (Type == DialogItemType.String)
            {
                string voicePathValue = string.Empty;
                string text = string.Empty;
                string value = Value.ToString();
                bool isSecondPart = false;

                foreach (var c in value)
                {
                    if (c == ';')
                    {
                        isSecondPart = true;
                        continue;
                    }
                    if (isSecondPart)
                    {
                        text += c;
                        continue;
                    }

                    voicePathValue += c;
                }

                if (!string.IsNullOrEmpty(voicePathValue) &&
                    ResourcePath.TryParse(voicePathValue, out var voicePath))
                {
                    return new ResourceString(text, voicePath);
                }

                return new ResourceString(text);
            }
            else if (Type == DialogItemType.StringCollection)
            {
                List<IResourceString> stringsCollection = [];
                string lastFullValue = string.Empty;
                string id = string.Empty;
                bool isSecondPart = false;
                bool ignoreNext = false;

                void AddLastValue()
                {
                    var reference = Parse(lastFullValue);
                    var gettedItem = reference.GetItem(resourcesOwner);

                    if (gettedItem is IResourceString str)
                    {
                        stringsCollection.Add(str);
                    }

                    lastFullValue = string.Empty;
                }

                foreach (var c in Value.ToString())
                {
                    if (!isSecondPart)
                    {
                        if (c == ':')
                        {
                            isSecondPart = true;
                            continue;
                        }

                        id += c;
                        continue;
                    }
                    if (c == '\\')
                    {
                        ignoreNext = true;
                        continue;
                    }
                    if (c == '|' && !ignoreNext)
                    {
                        AddLastValue();
                        continue;
                    }

                    ignoreNext = false;
                    lastFullValue += c;
                }

                AddLastValue();

                return new LocalStringCollection(id, stringsCollection);
            }
            else if (Type == DialogItemType.JoinInfo)
            {
                List<string> parts = [];
                string currentPart = string.Empty;

                foreach (var symbol in Value.ToString())
                {
                    if (symbol == '|' && parts.Count < 2)
                    {
                        parts.Add(currentPart);
                        currentPart = string.Empty;
                        continue;
                    }

                    currentPart += symbol;
                }

                parts.Add(currentPart);

                if (parts.Count != 3)
                {
                    return new JoinOperationInfo([], []);
                }

                var inputParts = parts[1].Split(',');
                var outputParts = parts[2].Split(';');
                List<int> inputs = [.. inputParts.Select(int.Parse)];
                List<DialogPosition> outputs = [];

                foreach (var output in outputParts)
                {
                    var outParts = output.Split(',');

                    if (outParts.Length != 2)
                    {
                        continue;
                    }

                    int section = int.Parse(outParts[0]);
                    int operation = int.Parse(outParts[1]);

                    outputs.Add(new(section, operation));
                }

                return new JoinOperationInfo(parts[0], inputs, outputs);
            }
            else if (Type == DialogItemType.Character)
            {
                string id = string.Empty;
                string name = string.Empty;
                string value = Value.ToString();
                bool skipNext = false;
                bool isSecondPart = false;

                foreach (var c in value)
                {
                    if (c == ':' && !skipNext)
                    {
                        isSecondPart = true;
                        skipNext = false;
                        continue;
                    }
                    if (c == '\\' && !skipNext)
                    {
                        skipNext = true;
                        continue;
                    }

                    skipNext = false;

                    if (isSecondPart)
                    {
                        name += c;
                    }
                    else
                    {
                        id += c;
                    }
                }

                return new LocalCharacter(id, name);
            }
            else if (Type == DialogItemType.Trigger)
            {
                return TriggerMetadata.Parse(Value.ToString());
            }
            if (!ResourcePath.TryParse(Value.ToString(), out var path))
            {
                throw new ArgumentException($"Не удалось получить путь ресурса");
            }

            return IResourcesOwner.FindResource(resourcesOwner, path);
        }

        public override bool Equals(object obj)
        {
            return obj is DialogItemReference other &&
                   Equals(other);
        }
        public bool Equals(DialogItemReference other)
        {
            return Type == other.Type &&
                   Value == other.Value;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
        public override string ToString()
        {
            return $"{Type}:{Value.Type}:{Value}";
        }

        #endregion

        #region Операторы

        public static bool operator ==(DialogItemReference r1, DialogItemReference r2) => r1.Equals(r2);
        public static bool operator !=(DialogItemReference r1, DialogItemReference r2) => !r1.Equals(r2);

        #endregion

        #region Статика

        public static readonly DialogItemReference Empty = new(DialogItemType.Variable, 0);

        public static bool Parse(string value, [NotNullWhen(true)] out DialogItemReference result)
        {
            result = default;
            var parts = GetParts(value);

            if (parts.Count != 3)
            {
                return false;
            }

            if (!Enum.TryParse<DialogItemType>(parts[0], out var itemType) ||
                !Enum.TryParse<DialogVariableType>(parts[1], out var valueType))
            {
                return false;
            }

            object itemValue = parts[2];

            if (valueType == DialogVariableType.Bool)
            {
                if (!bool.TryParse(parts[2], out var boolValue))
                {
                    return false;
                }

                itemValue = boolValue;
            }
            else if (valueType == DialogVariableType.Number)
            {
                itemValue = OperandValue.StringToNumber(parts[2]);
            }

            result = new(itemType, new(itemValue));

            return true;
        }
        public static DialogItemReference Parse(string value)
        {
            var parts = GetParts(value);

            if (parts.Count != 3)
            {
                throw new ArgumentException($"Не удалось считать ссылку на объект диалога из \"{value}\". Требуется частей: 3, получено: {parts.Count}", nameof(value));
            }

            var itemType = Enum.Parse<DialogItemType>(parts[0]);
            var valueType = Enum.Parse<DialogVariableType>(parts[1]);
            object itemValue = parts[2];

            if (valueType == DialogVariableType.Bool)
            {
                itemValue = bool.Parse(parts[2]);
            }
            else if (valueType == DialogVariableType.Number)
            {
                itemValue = OperandValue.StringToNumber(parts[2]);
            }

            return new(itemType, new(itemValue));
        }

        public static DialogItemReference Create(IResourceItem item)
        {
            return new(DialogItemType.Resource, item.GetPath().ToString());
        }
        public static DialogItemReference Create(IVariable variable)
        {
            if (variable.IsSeparated)
            {
                return new(DialogItemType.Variable, variable.Value);
            }

            return Create(variable);
        }
        public static DialogItemReference Create(OperandValue value)
        {
            return new(DialogItemType.Variable, value);
        }
        public static DialogItemReference Create(IResourceString item)
        {
            if (!item.IsSeparated)
            {
                return Create((IResourceItem)item);
            }

            string value = ";";

            if (item.Voice?.IsSeparated == false)
            {
                value = $"{item.Voice.GetPath()};";
            }

            value += $"{item.Text}";

            return new(DialogItemType.String, value);
        }
        public static DialogItemReference Create(IStringCollection item)
        {
            if (!item.IsSeparated)
            {
                return Create((IResourceItem)item);
            }

            string values = string.Empty;

            foreach (var str in item.Strings)
            {
                if (values != string.Empty)
                {
                    values += "|";
                }

                values += Create(str).ToString().Replace("|", @"\|");
            }

            return new(DialogItemType.StringCollection, $"{item.Id}:{values}");
        }
        public static DialogItemReference Create(IJoinOperationInfo info)
        {
            if (!info.IsSeparated)
            {
                return Create((IResourceItem)info);
            }

            string inputs = string.Empty;
            string outputs = string.Empty;

            foreach (var input in info.InputSections)
            {
                if (inputs != string.Empty)
                {
                    inputs += ",";
                }

                inputs += input;
            }
            foreach (var output in info.Outputs)
            {
                if (outputs != string.Empty)
                {
                    outputs += ";";
                }

                outputs += $"{output.Section},{output.Operation}";
            }

            return new(DialogItemType.JoinInfo, $"{info.Id}|{inputs}|{outputs}");
        }
        public static DialogItemReference Create(ICharacter character)
        {
            if (!character.IsSeparated)
            {
                return Create(character);
            }

            string id = character.Id.Trim().Replace(@"\", @"\\").Replace(":", @"\:");
            string name = character.Name.Trim();

            return new(DialogItemType.Character, $"{id}:{name}");
        }
        public static DialogItemReference CreateFromProject(DialogProjectReference projectReference)
        {
            return new(DialogItemType.Resource, projectReference.ResourcesPath.ToString());
        }

        public static DialogItemReference CreateUnknown(object? item)
        {
            if (item is IVariable variable)
            {
                return Create(variable);
            }
            else if (item is OperandValue operand)
            {
                return Create(operand);
            }
            else if (item is IResourceString str)
            {
                return Create(str);
            }
            else if (item is IStringCollection collection)
            {
                return Create(collection);
            }
            else if (item is IJoinOperationInfo info)
            {
                return Create(info);
            }
            else if (item is IResourceItem resource)
            {
                return Create(resource);
            }
            else if (item is TriggerMetadata trigger)
            {
                return trigger.CreateReference();
            }
            else if (item is string ||
                     item is float ||
                     item is int ||
                     item is bool)
            {
                return new(DialogItemType.Variable, new(item));
            }

            throw new ArgumentException($"Невозможно создать ссылку для неизвестного типа: {item?.GetType().FullName}", nameof(item));
        }

        private static List<string> GetParts(string value)
        {
            List<string> result = new(3);
            string lastValue = string.Empty;

            value = value.Trim();

            for (int i = 0; i < value.Length; i++)
            {
                var currentChar = value[i];

                if (currentChar == ':' && result.Count < 2)
                {
                    result.Add(lastValue.Trim());
                    lastValue = string.Empty;
                    continue;
                }

                lastValue += currentChar;
            }

            result.Add(lastValue.Trim());

            return result;
        }

        #endregion
    }
}
