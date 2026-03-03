using DialogMaker.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DialogMaker.Core.Executioning.Internal
{
    internal readonly struct TriggerMetadata(string id, IDictionary<string, int> inputs, IDictionary<string, int> outputs) 
        : IResourceItem, IEquatable<TriggerMetadata>
    {
        public string Id { get; } = id;
        public DialogResourceType ResourceType => DialogResourceType.String;
        public bool IsSeparated => true;
        public ReadOnlyDictionary<string, int> Inputs { get; } = new(inputs);
        public ReadOnlyDictionary<string, int> Outputs { get; } = new(outputs);

        #region Управление

        public Dictionary<string, object> GetInputValues(IDialogExecutionResources resources)
        {
            Dictionary<string, object> result = new(Inputs.Count);

            foreach (var info in Inputs)
            {
                object value = resources.GetResource(info.Value);

                if (value is IVariable variable)
                {
                    value = variable.Value;
                }

                result.Add(info.Key, value);
            }

            return result;
        }

        public ResourcePath GetPath()
        {
            throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
        }
        public DialogItemReference CreateReference()
        {
            return new(DialogItemType.Trigger, ToString());
        }
        public IVariable ToVariable()
        {
            return new LocalVariable(Id);
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return "Empty trigger::";
            }

            return $"{Format(Id)}:{DictionaryToString(Inputs)}:{DictionaryToString(Outputs)}";
        }

        public override bool Equals(object obj)
        {
            return obj is TriggerMetadata other && Equals(other);
        }
        public bool Equals(TriggerMetadata other)
        {
            return Id == other.Id &&
                   ResourceType == other.ResourceType &&
                   IsSeparated == other.IsSeparated &&
                   Inputs == other.Inputs &&
                   Outputs == other.Outputs;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ResourceType, IsSeparated, Inputs, Outputs);
        }

        #endregion

        #region Статика

        public static TriggerMetadata Parse(string value)
        {
            string[] values = new string[3];
            int currentValueIndex = 0;

            ParseHelper.Parse(value, ':', part =>
            {
                values[currentValueIndex] = part;
                currentValueIndex++;
            });

            if (currentValueIndex != 3)
            {
                throw new ArgumentException($"Не удалось прочитать метаданные триггера. Получено {currentValueIndex} частей из {values.Length}");
            }

            var inputs = ParseDictionary(values[1]);
            var outputs = ParseDictionary(values[2]);


            return new(values[0], inputs, outputs);
        }

        private static string Format(string value)
        {
            return value.Trim().Replace(@"\", @"\\").Replace(":", @"\:").Replace(",", @"\,").Replace(";", @"\;");
        }
        private static string DictionaryToString(ReadOnlyDictionary<string, int>? dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return string.Empty; 
            }

            string result = string.Empty;

            foreach (var info in dictionary)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += ";";
                }

                result += $"{Format(info.Key)},{info.Value}";
            }

            return result;
        }
        private static Dictionary<string, int> ParseDictionary(string? value)
        {
            Dictionary<string, int> result = [];
            value = value?.Trim();

            if (string.IsNullOrEmpty(value))
            {
                return result; 
            }

            string currentPair = string.Empty;

            void PushPair(string currentPair)
            {
                var parts = currentPair.Split(',');

                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Не удалось прочитать пару название-индекс. Значение: {currentPair}", nameof(value));
                }
                if (parts[0].Length == 0)
                {
                    throw new ArgumentException($"Название не может быть пустым. Значение: {currentPair}");
                }
                if (int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var index))
                {
                    result.Add(parts[0], index);
                }
                else
                {
                    throw new ArgumentException($"Не удалось преобразовать текстовое представление индекса в число. Значение: {currentPair}", nameof(value));
                }

                currentPair = string.Empty;
            }

            ParseHelper.Parse(value, ';', PushPair);

            return result;
        }

        #endregion
    }
}
