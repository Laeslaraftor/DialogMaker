using DialogMaker.Core.Editor.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialogNodeSavedState : JsonData, IIdReplaceable
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("nodeType")]
        public DialogNodeType NodeType { get; set; }
        [JsonProperty("position")]
        public Vector2 Position { get; set; }
        [JsonProperty("inverted")]
        public bool Inverted { get; set; }
        [JsonProperty("inputs")]
        public Dictionary<int, DialogProjectNodePortSavedState> Inputs { get; set; } = [];
        [JsonProperty("outputs")]
        public Dictionary<int, DialogProjectNodePortSavedState> Outputs { get; set; } = [];
        [JsonProperty("properties")]
        public Dictionary<string, object?> Properties { get; set; } = [];

        #region Управление

        public bool ContainsId(Guid id)
        {
            if (id == Id)
            {
                return true;
            }

            bool Contains(IEnumerable<IIdReplaceable> replaceables)
            {
                foreach (var replaceable in replaceables)
                {
                    if (replaceable.ContainsId(id))
                    {
                        return true;
                    }
                }

                return false;
            }

            return Contains(Inputs.Values) || Contains(Outputs.Values);
        }
        public void ReplaceIds(IDictionary<Guid, Guid> identifiersMap)
        {
            if (identifiersMap.TryGetValue(Id, out var newId))
            {
                Id = newId;
            }

            void ReplaceAll(IEnumerable<IIdReplaceable> replaceables)
            {
                foreach (var replaceable in replaceables)
                {
                    replaceable.ReplaceIds(identifiersMap);
                }
            }

            ReplaceAll(Inputs.Values);
            ReplaceAll(Outputs.Values);
        }

        public float GetNumberProperty(string propertyName)
        {
            if (!Properties.TryGetValue(propertyName, out var result))
            {
                return 0f;
            }

            if (result is int i)
            {
                return i;
            }
            if (result is double d)
            {
                return (float)d;
            }
            if (result is float f)
            {
                return f;
            }

            return 0f;
        }
        public T? GetProperty<T>(string propertyName)
        {
            if (!Properties.TryGetValue(propertyName, out var value))
            {
                return default;
            }

            var typeInfo = typeof(T);

            if (typeInfo.IsEnum && (value is int || value is long))
            {
                return (T?)GetEnumValue(typeInfo, Convert.ToInt32((long)value));
            }
            if (value is T typedValue)
            {
                return typedValue;
            }
            if (value is not JToken token)
            {
                return default;
            }

            return token.ToObject<T>();
        }

        public DialogProjectReference? RestoreReference(DialogProject project, string name)
        {
            var savedState = GetProperty<DialogProjectReferenceSavedState>(name);

            if (savedState == null)
            {
                return null;
            }

            try
            {
                return DialogProjectReference.Restore(project, savedState);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }

            return null;
        }
        public DialogProjectReference<T>? RestoreReference<T>(DialogProject project, string name)
            where T : DialogProjectResourceObject
        {
            if (RestoreReference(project, name) is DialogProjectReference<T> typedReference)
            {
                return typedReference;
            }

            return null;
        }

        private object? GetEnumValue(Type enumType, int intValue)
        {
            var values = Enum.GetValues(enumType);

            if (values.Length == 0)
            {
                return null;
            }

            var underlyingType = enumType.GetEnumUnderlyingType();
            Func<object, bool> predicate = value =>
            {
                return (int)value == intValue;
            };

            if (underlyingType == typeof(byte))
            {
                predicate = value => (byte)value == intValue;
            }

            foreach (var value in values)
            {
                if (predicate(value))
                {
                    return value;
                }
            }

            return values.GetValue(0);
        }

        #endregion
    }
}
